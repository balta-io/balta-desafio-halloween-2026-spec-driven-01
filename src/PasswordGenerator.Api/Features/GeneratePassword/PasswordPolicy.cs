namespace PasswordGenerator.Api.Features.GeneratePassword;

public static class PasswordPolicy
{
    public const int MinLength = 16;
    public const int MaxLength = 128;

    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Digits = "0123456789";
    public const string Special = "!@#$%^&*()-_=+[]{};:,.<>?/";
    public const string Ambiguous = "Il1O0";

    public static bool IsValidLength(int length) => length is >= MinLength and <= MaxLength;

    public static bool IsValid(string? password)
    {
        if (password is null || !IsValidLength(password.Length))
            return false;

        bool hasLower = false, hasUpper = false, hasDigit = false, hasSpecial = false;
        foreach (var c in password)
        {
            if (Lowercase.Contains(c)) hasLower = true;
            else if (Uppercase.Contains(c)) hasUpper = true;
            else if (Digits.Contains(c)) hasDigit = true;
            else if (Special.Contains(c)) hasSpecial = true;
            else return false;
        }

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }
}
