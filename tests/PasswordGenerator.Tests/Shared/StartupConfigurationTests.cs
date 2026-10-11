using Microsoft.Extensions.Options;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Shared;

public class StartupConfigurationTests
{
    [Theory]
    [InlineData("15")]
    [InlineData("129")]
    [InlineData("0")]
    public void Application_does_not_start_with_length_out_of_range(string length)
    {
        using var parent = new ApiFactory();
        using var factory = parent.WithWebHostBuilder(b => b.UseSetting("PasswordGenerator:Length", length));

        Assert.ThrowsAny<OptionsValidationException>(() => factory.CreateClient());
    }

    [Theory]
    [InlineData("16")]
    [InlineData("128")]
    public void Application_starts_with_length_at_the_limits(string length)
    {
        using var parent = new ApiFactory();
        using var factory = parent.WithWebHostBuilder(b => b.UseSetting("PasswordGenerator:Length", length));

        using var client = factory.CreateClient();

        Assert.NotNull(client);
    }

    [Fact]
    public void Application_starts_with_default_values()
    {
        using var factory = new ApiFactory();

        using var client = factory.CreateClient();

        Assert.NotNull(client);
    }
}
