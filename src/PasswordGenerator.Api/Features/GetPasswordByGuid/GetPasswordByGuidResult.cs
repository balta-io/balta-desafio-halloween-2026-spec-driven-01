namespace PasswordGenerator.Api.Features.GetPasswordByGuid;

public abstract record GetPasswordByGuidResult
{
    private GetPasswordByGuidResult() { }

    public sealed record Found(string Senha) : GetPasswordByGuidResult;
    public sealed record InvalidGuid : GetPasswordByGuidResult;
    public sealed record NotFound : GetPasswordByGuidResult;
}
