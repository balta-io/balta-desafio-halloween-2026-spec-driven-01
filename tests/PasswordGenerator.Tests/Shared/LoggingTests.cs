namespace PasswordGenerator.Tests.Shared;

public class LoggingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string RouteTemplate = "RouteTemplate";

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline)
            await Task.Delay(25);
    }

    private int RequestEvents(string template) =>
        factory.Logs.Events.Count(e =>
            e.Properties.TryGetValue(RouteTemplate, out var value) && value.ToString() == $"\"{template}\"");

    [Fact]
    public async Task Logs_never_contain_the_generated_password_nor_the_raw_guid()
    {
        var client = factory.CreateClient();
        var id = await client.CreatePasswordAsync();
        var senha = await factory.StoredPasswordAsync(id);

        await client.GetAsync($"/api/v1/passwords/{id}");
        await client.GetAsync($"/api/v1/passwords/{Guid.NewGuid()}");
        await client.GetAsync("/api/v1/passwords/texto-livre");
        await WaitUntilAsync(() => RequestEvents("/api/v1/passwords/{guid}") >= 3);

        var output = factory.Logs.RenderedOutput().ToList();
        Assert.True(RequestEvents("/api/v1/passwords/{guid}") >= 3);
        Assert.DoesNotContain(output, line => line.Contains(senha));
        Assert.DoesNotContain(output, line => line.Contains(id.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(output, line => line.Contains(id.ToString("N"), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Request_logs_use_the_route_template()
    {
        var client = factory.CreateClient();
        await client.GetAsync($"/api/v1/passwords/{Guid.NewGuid()}");

        await WaitUntilAsync(() => RequestEvents("/api/v1/passwords/{guid}") > 0);

        Assert.True(RequestEvents("/api/v1/passwords/{guid}") > 0);
    }
}
