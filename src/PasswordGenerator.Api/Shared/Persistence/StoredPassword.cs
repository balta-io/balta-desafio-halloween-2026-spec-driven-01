namespace PasswordGenerator.Api.Shared.Persistence;

public sealed class StoredPassword
{
    private StoredPassword()
    {
        Senha = string.Empty;
    }

    public StoredPassword(Guid id, string senha, DateTime createdAtUtc)
    {
        Id = id;
        Senha = senha;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public string Senha { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
