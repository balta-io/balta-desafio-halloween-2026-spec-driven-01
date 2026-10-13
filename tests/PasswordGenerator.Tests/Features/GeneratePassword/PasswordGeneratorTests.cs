using Microsoft.Extensions.Options;
using PasswordGenerator.Api.Features.GeneratePassword;
using Generator = PasswordGenerator.Api.Features.GeneratePassword.PasswordGenerator;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class PasswordGeneratorTests
{
    private static Generator Create(int length = 24, bool excludeAmbiguous = false) =>
        new(Options.Create(new PasswordGeneratorOptions { Length = length, ExcludeAmbiguous = excludeAmbiguous }));

    [Fact]
    public void Generates_1000_passwords_accepted_by_policy()
    {
        var sut = Create();

        for (var i = 0; i < 1000; i++)
        {
            var password = sut.Generate();
            Assert.True(PasswordPolicy.IsValid(password), password);
            Assert.True(password.Length >= 16);
            Assert.Contains(password, c => PasswordPolicy.Special.Contains(c));
            Assert.DoesNotContain(password, char.IsWhiteSpace);
        }
    }

    [Fact]
    public void Generates_1000_distinct_passwords()
    {
        var sut = Create();

        var passwords = Enumerable.Range(0, 1000).Select(_ => sut.Generate()).ToHashSet();

        Assert.Equal(1000, passwords.Count);
    }

    [Fact]
    public void Successive_calls_produce_different_passwords()
    {
        var sut = Create();

        Assert.NotEqual(sut.Generate(), sut.Generate());
    }

    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(128)]
    public void Respects_configured_length(int length)
    {
        var sut = Create(length);

        Assert.Equal(length, sut.Generate().Length);
        Assert.True(PasswordPolicy.IsValid(sut.Generate()));
    }

    [Fact]
    public void ExcludeAmbiguous_never_produces_ambiguous_characters()
    {
        var sut = Create(excludeAmbiguous: true);

        for (var i = 0; i < 1000; i++)
        {
            var password = sut.Generate();
            Assert.True(PasswordPolicy.IsValid(password));
            Assert.DoesNotContain(password, c => "Il1O0".Contains(c));
        }
    }
}
