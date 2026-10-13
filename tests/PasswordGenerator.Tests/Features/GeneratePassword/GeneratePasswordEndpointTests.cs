using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Features.GeneratePassword;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class GeneratePasswordEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_returns_201_with_guid_persisted_and_no_password()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/passwords", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(raw);
        Assert.Single(json.RootElement.EnumerateObject());
        var id = json.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal($"/api/v1/passwords/{id}", response.Headers.Location?.OriginalString);

        var stored = await factory.StoredPasswordAsync(id);
        Assert.True(PasswordPolicy.IsValid(stored));
        Assert.DoesNotContain(stored, raw);
        Assert.Equal(1, await factory.WithDbAsync(db => db.Passwords.CountAsync(p => p.Id == id)));
    }

    [Fact]
    public async Task Post_ignores_request_body()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/passwords", new { length = 5 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Each_post_produces_new_guid_and_password()
    {
        var client = factory.CreateClient();

        var first = await client.CreatePasswordAsync();
        var second = await client.CreatePasswordAsync();

        Assert.NotEqual(first, second);
        Assert.NotEqual(await factory.StoredPasswordAsync(first), await factory.StoredPasswordAsync(second));
    }
}
