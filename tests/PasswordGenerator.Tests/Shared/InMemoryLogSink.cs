using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace PasswordGenerator.Tests.Shared;

public sealed class InMemoryLogSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public IReadOnlyCollection<LogEvent> Events => _events;

    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    public IEnumerable<string> RenderedOutput() =>
        _events.Select(e =>
            e.RenderMessage() + " " +
            string.Join(' ', e.Properties.Select(p => $"{p.Key}={p.Value}")) + " " +
            e.Exception);
}
