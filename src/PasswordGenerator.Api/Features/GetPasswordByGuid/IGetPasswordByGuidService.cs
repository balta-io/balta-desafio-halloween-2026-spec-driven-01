namespace PasswordGenerator.Api.Features.GetPasswordByGuid;

public interface IGetPasswordByGuidService
{
    Task<GetPasswordByGuidResult> GetAsync(string id, CancellationToken ct);
}
