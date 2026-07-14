using MediatR;
using OpenPayrun.Application;
using OpenPayrun.Application.Features.Payroll;
using OpenPayrun.Infrastructure;

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

app.UseCors("AllowedOrigins");

app.MapPost("/api/payruns", async (CreatePayRunCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return Results.Created($"/api/payruns/{result.Id}", result);
});

app.Run();
