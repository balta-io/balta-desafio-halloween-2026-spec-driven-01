namespace PasswordGenerator.Api.Features.GeneratePassword;

public sealed class PasswordGeneratorOptions
{
    public const string SectionName = "PasswordGenerator";

    public int Length { get; set; } = 24;
    public bool ExcludeAmbiguous { get; set; }
}
