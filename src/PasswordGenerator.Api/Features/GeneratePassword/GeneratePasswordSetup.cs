namespace PasswordGenerator.Api.Features.GeneratePassword;

public static class GeneratePasswordSetup
{
    public static IServiceCollection AddGeneratePassword(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PasswordGeneratorOptions>()
            .Bind(configuration.GetSection(PasswordGeneratorOptions.SectionName))
            .Validate(
                o => PasswordPolicy.IsValidLength(o.Length),
                $"PasswordGenerator:Length deve estar entre {PasswordPolicy.MinLength} e {PasswordPolicy.MaxLength}.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        services.AddScoped<IGeneratePasswordService, GeneratePasswordService>();
        return services;
    }
}
