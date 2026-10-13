namespace PasswordGenerator.Api.Features.GetPasswordByGuid;

public static class GetPasswordByGuidSetup
{
    public static IServiceCollection AddGetPasswordByGuid(this IServiceCollection services)
    {
        services.AddScoped<IGetPasswordByGuidService, GetPasswordByGuidService>();
        return services;
    }
}
