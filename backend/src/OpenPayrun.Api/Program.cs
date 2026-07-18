using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenPayrun.Application;
using OpenPayrun.Application.Features.Payroll;
using OpenPayrun.Application.Features.TaxRates;
using OpenPayrun.Infrastructure;
using OpenPayrun.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}

app.UseCors("AllowedOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", (LoginRequest req, IConfiguration config) =>
{
    if (req.Username != config["Admin:Username"] || req.Password != config["Admin:Password"])
        return Results.Unauthorized();

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        claims: [new Claim(ClaimTypes.Role, "Admin")]);

    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

app.MapPost("/api/pay-runs/calculate", async (CalculatePayRunQuery query, ISender sender) =>
{
    try { return Results.Ok(await sender.Send(query)); }
    catch (NotSupportedException e) { return Results.BadRequest(new { e.Message }); }
});

app.MapGet("/api/tax-rates", async (ISender sender) =>
    await sender.Send(new GetTaxRateSetsQuery()));

app.MapPost("/api/tax-rates", async (TaxRateSetBody body, ISender sender) =>
{
    var result = await sender.Send(new CreateTaxRateSetCommand(body));
    return Results.Created($"/api/tax-rates/{result.Id}", result);
}).RequireAuthorization();

app.MapPut("/api/tax-rates/{id:int}", async (int id, TaxRateSetBody body, ISender sender) =>
{
    try { return Results.Ok(await sender.Send(new UpdateTaxRateSetCommand(id, body))); }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
}).RequireAuthorization();

app.MapDelete("/api/tax-rates/{id:int}", async (int id, ISender sender) =>
{
    try { await sender.Send(new DeleteTaxRateSetCommand(id)); return Results.NoContent(); }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
}).RequireAuthorization();

app.Run();

record LoginRequest(string Username, string Password);

public partial class Program { }
