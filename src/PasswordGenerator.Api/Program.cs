using PasswordGenerator.Api.Features.GeneratePassword;
using PasswordGenerator.Api.Features.GetPasswordByGuid;
using PasswordGenerator.Api.Shared.Errors;
using PasswordGenerator.Api.Shared.Observability;
using PasswordGenerator.Api.Shared.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddStructuredLogging();

builder.Services
    .AddPersistence()
    .AddGlobalProblemDetails()
    .AddObservability()
    .AddGeneratePassword(builder.Configuration)
    .AddGetPasswordByGuid()
    .AddOpenApi();

var app = builder.Build();

app.Services.InitializeDatabase();

app.UseExceptionHandler();
app.UseRouteTemplateRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapGeneratePassword();
app.MapGetPasswordByGuid();

app.Run();

public partial class Program;
