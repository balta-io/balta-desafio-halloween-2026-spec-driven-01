using System.Diagnostics.Metrics;

namespace PasswordGenerator.Api.Shared.Observability;

public sealed class PasswordMetrics : IDisposable
{
    public const string MeterName = "PasswordGenerator.Api";

    private readonly Counter<long> _generated;
    private readonly Counter<long> _retrieved;

    public PasswordMetrics()
    {
        Meter = new Meter(MeterName);
        _generated = Meter.CreateCounter<long>("passwords.generated");
        _retrieved = Meter.CreateCounter<long>("passwords.retrieved");
    }

    public Meter Meter { get; }

    public void PasswordGenerated() => _generated.Add(1);

    public void PasswordRetrieved() => _retrieved.Add(1);

    public void Dispose() => Meter.Dispose();
}
