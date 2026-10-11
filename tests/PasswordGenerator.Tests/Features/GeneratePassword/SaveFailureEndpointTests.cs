using System.Net;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class SaveFailureEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Save_failure_returns_500_problem_without_guid_and_without_record()
    {
        var client = factory.CreateClient();
        var before = await factory.WithDbAsync(db => db.Passwords.CountAsync());
        factory.FailingInterceptor.Enabled = true;

        try
        {
            var response = await client.PostAsync("/api/v1/passwords", content: null);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            var body = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("\"id\"", body);
            Assert.DoesNotContain(FailingSaveChangesInterceptor.FailureMessage, body);
            Assert.DoesNotContain("   at ", body);
            Assert.DoesNotContain("InvalidOperationException", body);
        }
        finally
        {
            factory.FailingInterceptor.Enabled = false;
        }

        Assert.Equal(before, await factory.WithDbAsync(db => db.Passwords.CountAsync()));
    }
}
