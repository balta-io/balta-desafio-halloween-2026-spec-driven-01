using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Shared.Observability;

namespace PasswordGenerator.Api.Features.GetPasswordByGuid;

public static class GetPasswordByGuidEndpoint
{
    public static IEndpointRouteBuilder MapGetPasswordByGuid(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/passwords/{guid}", async Task<Results<Ok<GetPasswordByGuidResponse>, ValidationProblem, ProblemHttpResult>> (
            string guid,
            IGetPasswordByGuidService service,
            PasswordMetrics metrics,
            HttpContext http,
            CancellationToken ct) =>
        {
            http.Response.Headers.CacheControl = "no-store";

            var result = await service.GetAsync(guid, ct);
            metrics.PasswordRetrieved();
            return result switch
            {
                GetPasswordByGuidResult.Found found => TypedResults.Ok(new GetPasswordByGuidResponse(found.Senha)),
                GetPasswordByGuidResult.InvalidGuid => TypedResults.ValidationProblem(
                    new Dictionary<string, string[]> { ["guid"] = ["O identificador não é um GUID válido."] }),
                _ => TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Senha não encontrada.",
                    detail: "Não existe senha para o identificador indicado."),
            };
        });

        return app;
    }
}
