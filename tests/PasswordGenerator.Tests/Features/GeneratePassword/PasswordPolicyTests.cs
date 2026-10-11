using PasswordGenerator.Api.Features.GeneratePassword;

namespace PasswordGenerator.Tests.Features.GeneratePassword;

public class PasswordPolicyTests
{
    private static string WithLength(int length) => "Aa1!" + new string('x', length - 4);

    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(128)]
    public void Accepts_valid_length(int length) =>
        Assert.True(PasswordPolicy.IsValid(WithLength(length)));

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Rejects_length_below_minimum(int length) =>
        Assert.False(PasswordPolicy.IsValid(new string('a', length)));

    [Fact]
    public void Rejects_length_below_16_even_with_all_classes() =>
        Assert.False(PasswordPolicy.IsValid(WithLength(15)));

    [Fact]
    public void Rejects_length_above_128() =>
        Assert.False(PasswordPolicy.IsValid(WithLength(129)));

    [Theory]
    [InlineData(15, false)]
    [InlineData(16, true)]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public void IsValidLength_respects_limits(int length, bool expected) =>
        Assert.Equal(expected, PasswordPolicy.IsValidLength(length));

    [Fact]
    public void Rejects_password_without_special_character() =>
        Assert.False(PasswordPolicy.IsValid("Abcdefghijklmnop1234"));

    [Theory]
    [InlineData("Aa1!bcdefghijklm nop")]
    [InlineData("Aa1!bcdefghijklm\tnop")]
    [InlineData("Aa1!bcdefghijklm\nnop")]
    [InlineData("Aa1!bcdefghijklm\r\nop")]
    public void Rejects_whitespace(string password) =>
        Assert.False(PasswordPolicy.IsValid(password));

    [Theory]
    [InlineData("Aa1!bcdefghijklm\"op")]
    [InlineData("Aa1!bcdefghijklm\\op")]
    [InlineData("Aa1!bcdefghijklm'op")]
    [InlineData("Aa1!bcdefghijklmçop")]
    [InlineData("Aa1!bcdefghijklm~op")]
    public void Rejects_characters_outside_allowed_sets(string password) =>
        Assert.False(PasswordPolicy.IsValid(password));

    [Theory]
    [InlineData("AAAAAAAAAAAAAAA1!")]
    [InlineData("aaaaaaaaaaaaaaa1!")]
    [InlineData("Aaaaaaaaaaaaaaaa!")]
    public void Rejects_missing_class_required_by_reinforced_composition(string password) =>
        Assert.False(PasswordPolicy.IsValid(password));

    [Fact]
    public void Rejects_null() => Assert.False(PasswordPolicy.IsValid(null));

    [Fact]
    public void Accepts_every_special_character_of_the_documented_set()
    {
        foreach (var special in PasswordPolicy.Special)
            Assert.True(PasswordPolicy.IsValid("Aa1" + special + new string('x', 20)));
    }
}
