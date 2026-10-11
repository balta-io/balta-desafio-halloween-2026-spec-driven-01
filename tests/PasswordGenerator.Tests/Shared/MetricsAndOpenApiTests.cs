using Microsoft.AspNetCore.Hosting;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Api.Shared.Observability;

namespace PasswordGenerator.Tests.Shared;

public class MetricsAndOpenApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Counters_increment_on_both_endpoints()
    {
        var meter = factory.Services.GetRequiredService<PasswordMetrics>().Meter;
        var totals = new Dictionary<string, long>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (ReferenceEquals(instrument.Meter, meter))
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
        {
            lock (totals)
                totals[instrument.Name] = totals.GetValueOrDefault(instrument.Name) + value;
        });
        listener.Start();
        var client = factory.CreateClient();

        var id = await client.CreatePasswordAsync();
        await client.GetAsync($"/api/v1/passwords/{id}");
        await client.GetAsync($"/api/v1/passwords/{id}");

        lock (totals)
        {
            Assert.Equal(1, totals["passwords.generated"]);
            Assert.Equal(2, totals["passwords.retrieved"]);
        }
    }

    [Fact]
    public async Task OpenApi_document_describes_both_endpoints_in_development()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/v1/passwords").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/api/v1/passwords/{guid}").TryGetProperty("get", out _));
    }

    [Fact]
    public async Task Scalar_documentation_is_served_in_development()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_is_not_exposed_outside_development()
    {
        using var production = factory.WithWebHostBuilder(b => b.UseEnvironment("Production"));
        var client = production.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
