using System.Security.Cryptography;

namespace Infrastructure.Helpers;

public static class PasswordGenerator
{
    public static string Generate()
    {
        var letter = (char)RandomNumberGenerator.GetInt32('A', 'Z' + 1);
        var numbers = string.Concat(Enumerable.Range(0, 6).Select(_ => RandomNumberGenerator.GetInt32(0, 10)));
        return $"{letter}{numbers}";
    }
}
