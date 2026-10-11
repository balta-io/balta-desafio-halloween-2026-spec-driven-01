namespace PasswordGenerator.Api.Features.GeneratePassword;

public interface IGeneratePasswordService
{
    Task<GeneratePasswordResponse> GenerateAsync(CancellationToken ct);
}
