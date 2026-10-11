using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Shared.Observability;

namespace PasswordGenerator.Api.Features.GeneratePassword;

public static class GeneratePasswordEndpoint
{
    public static IEndpointRouteBuilder MapGeneratePassword(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/passwords", async Task<Created<GeneratePasswordResponse>> (
            IGeneratePasswordService service,
            PasswordMetrics metrics,
            CancellationToken ct) =>
        {
            var response = await service.GenerateAsync(ct);
            metrics.PasswordGenerated();
            return TypedResults.Created($"/api/v1/passwords/{response.Id}", response);
        });

        return app;
    }
}
