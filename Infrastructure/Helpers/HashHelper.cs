using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Helpers;

public static class HashHelper
{
    public static string Sha256(string value)
    {
        using var hash = SHA256.Create();
        var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        return string.Concat(bytes.Select(b => b.ToString("x2")));
    }
    
    public static string GenerateTransactionId()
    {
        var milliseconds = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        return milliseconds.ToString();
    }
}
