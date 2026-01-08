namespace Infrastructure.Helpers;

public static class PasswordGenerator
{
    public static string Generate()
    {
        var random = new Random();
        var letter = (char)random.Next('A', 'Z' + 1);
        var numbers = string.Concat(Enumerable.Range(0, 6).Select(_ => random.Next(0, 10)));
        return $"{letter}{numbers}";
    }
}
