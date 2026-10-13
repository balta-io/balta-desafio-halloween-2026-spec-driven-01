using PasswordGenerator.Api.Features.GeneratePassword;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class GuidGeneratorTests
{
    [Fact]
    public void Generates_distinct_non_empty_guids()
    {
        var sut = new GuidGenerator();

        var guids = Enumerable.Range(0, 1000).Select(_ => sut.NewGuid()).ToList();

        Assert.DoesNotContain(Guid.Empty, guids);
        Assert.Equal(1000, guids.Distinct().Count());
    }

    [Fact]
    public void Generates_version_4_guids()
    {
        var guid = new GuidGenerator().NewGuid();

        Assert.Equal('4', guid.ToString()[14]);
    }
}
