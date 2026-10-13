using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PasswordGenerator.Api.Features.GeneratePassword;
using PasswordGenerator.Api.Features.GetPasswordByGuid;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Shared;

public class ExtraEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("GET", "/api/v1/passwords")]
    [InlineData("PUT", "/api/v1/passwords")]
    [InlineData("DELETE", "/api/v1/passwords/3f2504e0-4f89-41d3-9a0c-0305e82c3301")]
    [InlineData("POST", "/api/v1/passwords/3f2504e0-4f89-41d3-9a0c-0305e82c3301")]
    public async Task Unsupported_method_returns_405(string method, string path)
    {
        var client = factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Health_returns_200_when_database_is_reachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Fifty_parallel_posts_produce_distinct_guids_and_passwords()
    {
        var client = factory.CreateClient();

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 50).Select(_ => client.PostAsync("/api/v1/passwords", content: null)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        var ids = new List<Guid>();
        foreach (var response in responses)
            ids.Add((await response.Content.ReadFromJsonAsync<GeneratePasswordResponse>())!.Id);

        Assert.Equal(50, ids.Distinct().Count());
        var passwords = await factory.WithDbAsync(db =>
            db.Passwords.AsNoTracking().Where(p => ids.Contains(p.Id)).Select(p => p.Senha).ToListAsync());
        Assert.Equal(50, passwords.Distinct().Count());
    }

    [Fact]
    public async Task Password_with_json_escaped_characters_round_trips_after_deserializing()
    {
        const string senha = "Aa1!<>&+/{}[]-_=;:,.?";
        var generator = Substitute.For<IPasswordGenerator>();
        generator.Generate().Returns(senha);
        var client = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton(generator)))
            .CreateClient();

        var id = await client.CreatePasswordAsync();
        var response = await client.GetAsync($"/api/v1/passwords/{id}");

        var body = await response.Content.ReadFromJsonAsync<GetPasswordByGuidResponse>();
        Assert.Equal(senha, body!.Senha);
    }
}
