namespace PasswordGenerator.Api.Shared.Errors;

public static class ProblemDetailsSetup
{
    public static IServiceCollection AddGlobalProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
                {
                    context.ProblemDetails.Title = "Erro interno.";
                    context.ProblemDetails.Detail = "Ocorreu um erro ao processar o pedido.";
                }
            });
        return services;
    }
}
