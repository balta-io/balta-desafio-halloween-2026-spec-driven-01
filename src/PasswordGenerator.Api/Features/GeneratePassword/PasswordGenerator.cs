using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace PasswordGenerator.Api.Features.GeneratePassword;

public interface IPasswordGenerator
{
    string Generate();
}

public sealed class PasswordGenerator : IPasswordGenerator
{
    private readonly int _length;
    private readonly string[] _classes;
    private readonly string _alphabet;

    public PasswordGenerator(IOptions<PasswordGeneratorOptions> options)
    {
        _length = options.Value.Length;
        _classes = BuildClasses(options.Value.ExcludeAmbiguous);
        _alphabet = string.Concat(_classes);
    }

    public string Generate()
    {
        var chars = new char[_length];

        for (var i = 0; i < _classes.Length; i++)
            chars[i] = Pick(_classes[i]);

        for (var i = _classes.Length; i < chars.Length; i++)
            chars[i] = Pick(_alphabet);

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];

    private static string[] BuildClasses(bool excludeAmbiguous)
    {
        string[] classes = [PasswordPolicy.Lowercase, PasswordPolicy.Uppercase, PasswordPolicy.Digits, PasswordPolicy.Special];
        return excludeAmbiguous
            ? [.. classes.Select(c => string.Concat(c.Where(ch => !PasswordPolicy.Ambiguous.Contains(ch))))]
            : classes;
    }
}
