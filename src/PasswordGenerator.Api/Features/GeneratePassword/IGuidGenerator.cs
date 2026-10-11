namespace PasswordGenerator.Api.Features.GeneratePassword;

public interface IGuidGenerator
{
    Guid NewGuid();
}

public sealed class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}
