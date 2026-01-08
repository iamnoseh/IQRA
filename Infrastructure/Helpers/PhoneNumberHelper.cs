namespace Infrastructure.Helpers;

public static class PhoneNumberHelper
{
    public static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        var cleaned = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

        if (cleaned.StartsWith("+992"))
            return cleaned;

        if (cleaned.StartsWith("992"))
            return "+" + cleaned;

        if (cleaned.StartsWith("00992"))
            return "+" + cleaned.Substring(2);

        if (cleaned.Length == 9 && cleaned.StartsWith("9"))
            return "+992" + cleaned;

        return phoneNumber;
    }

    public static bool IsValidTajikPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var normalized = NormalizePhoneNumber(phoneNumber);

        if (!normalized.StartsWith("+992"))
            return false;

        if (normalized.Length != 13)
            return false;

        var number = normalized.Substring(4);
        return number.All(char.IsDigit) && (number.StartsWith("9") || number.StartsWith("5"));
    }
}
