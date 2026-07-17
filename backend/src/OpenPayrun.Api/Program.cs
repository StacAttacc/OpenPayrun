using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application;
using OpenPayrun.Application.Features.Payroll;
using OpenPayrun.Application.Features.TaxRates;
using OpenPayrun.Infrastructure;
using OpenPayrun.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

app.UseCors("AllowedOrigins");

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
});

app.MapPut("/api/tax-rates/{id:int}", async (int id, TaxRateSetBody body, ISender sender) =>
{
    try { return Results.Ok(await sender.Send(new UpdateTaxRateSetCommand(id, body))); }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
});

app.Run();
