using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Shared.Persistence;

namespace PasswordGenerator.Api.Features.GeneratePassword;

public sealed class GeneratePasswordService(
    AppDbContext db,
    IPasswordGenerator passwordGenerator,
    IGuidGenerator guidGenerator,
    TimeProvider timeProvider) : IGeneratePasswordService
{
    public const int MaxAttempts = 3;
    private const int SqlitePrimaryKeyViolation = 1555;

    public async Task<GeneratePasswordResponse> GenerateAsync(CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            var id = guidGenerator.NewGuid();
            var senha = passwordGenerator.Generate();

            if (!PasswordPolicy.IsValid(senha))
                throw new InvalidOperationException("A senha gerada não cumpre a política.");

            db.Passwords.Add(new StoredPassword(id, senha, timeProvider.GetUtcNow().UtcDateTime));

            try
            {
                await db.SaveChangesAsync(ct);
                return new GeneratePasswordResponse(id);
            }
            catch (DbUpdateException ex) when (attempt < MaxAttempts && IsPrimaryKeyViolation(ex))
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    private static bool IsPrimaryKeyViolation(DbUpdateException ex) =>
        ex.InnerException is SqliteException { SqliteExtendedErrorCode: SqlitePrimaryKeyViolation };
}
