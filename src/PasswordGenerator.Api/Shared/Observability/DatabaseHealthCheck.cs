using Microsoft.Extensions.Diagnostics.HealthChecks;
using PasswordGenerator.Api.Shared.Persistence;

namespace PasswordGenerator.Api.Shared.Observability;

public sealed class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
