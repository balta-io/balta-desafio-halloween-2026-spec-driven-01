using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PasswordGenerator.Tests.Shared;

public sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
{
    public const string FailureMessage = "falha-simulada-na-gravacao";

    public bool Enabled { get; set; }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThrowIfEnabled();
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThrowIfEnabled();
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ThrowIfEnabled()
    {
        if (Enabled)
            throw new InvalidOperationException(FailureMessage);
    }
}
