using System.Net;
using System.Net.Http.Json;
using PasswordGenerator.Api.Features.GetPasswordByGuid;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Features.GetPasswordByGuid;

public class GetPasswordByGuidEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<(HttpResponseMessage Response, string Senha)> GetAsync(HttpClient client, string id)
    {
        var response = await client.GetAsync($"/api/v1/passwords/{id}");
        var body = response.StatusCode == HttpStatusCode.OK
            ? (await response.Content.ReadFromJsonAsync<GetPasswordByGuidResponse>())!.Senha
            : string.Empty;
        return (response, body);
    }

    [Fact]
    public async Task Get_returns_the_generated_password_with_no_store()
    {
        var client = factory.CreateClient();
        var id = await client.CreatePasswordAsync();

        var (response, senha) = await GetAsync(client, id.ToString());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(await factory.StoredPasswordAsync(id), senha);
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task Same_guid_always_returns_same_password()
    {
        var client = factory.CreateClient();
        var id = await client.CreatePasswordAsync();

        var (_, first) = await GetAsync(client, id.ToString());
        var (_, second) = await GetAsync(client, id.ToString());
        var (_, third) = await GetAsync(client, id.ToString());

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public async Task Get_accepts_uppercase_guid()
    {
        var client = factory.CreateClient();
        var id = await client.CreatePasswordAsync();

        var (response, senha) = await GetAsync(client, id.ToString().ToUpperInvariant());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(await factory.StoredPasswordAsync(id), senha);
    }

    [Fact]
    public async Task Get_accepts_guid_without_hyphens()
    {
        var client = factory.CreateClient();
        var id = await client.CreatePasswordAsync();

        var (response, senha) = await GetAsync(client, id.ToString("N"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(await factory.StoredPasswordAsync(id), senha);
    }

    [Fact]
    public async Task Unknown_guid_returns_404_problem_without_echoing_guid()
    {
        var client = factory.CreateClient();
        var unknown = Guid.NewGuid();

        var response = await client.GetAsync($"/api/v1/passwords/{unknown}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(unknown.ToString(), await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("texto-livre")]
    [InlineData("12345")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Invalid_or_empty_guid_returns_400_problem_without_echoing_input(string input)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/passwords/{input}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(input, await response.Content.ReadAsStringAsync());
    }
}
