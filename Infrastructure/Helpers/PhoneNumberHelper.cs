namespace Infrastructure.Helpers;

public static class PhoneNumberHelper
{
    public static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        // Удаляем все кроме цифр и плюса
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // Если плюс есть не в начале, удаляем его
        if (cleaned.Contains('+') && !cleaned.StartsWith("+"))
        {
            cleaned = new string(cleaned.Where(char.IsDigit).ToArray());
        }

        if (cleaned.StartsWith("+992"))
            return cleaned;

        if (cleaned.StartsWith("992"))
            return "+" + cleaned;

        if (cleaned.StartsWith("00992"))
            return "+" + cleaned.Substring(2);

        // Если введено 9 цифр (например 937001122), добавляем код страны
        if (cleaned.Length == 9 && char.IsDigit(cleaned[0]))
            return "+992" + cleaned;

        return cleaned;
    }

    public static bool IsValidTajikPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var normalized = NormalizePhoneNumber(phoneNumber);

        // Формат Таджикистана: +992 и 9 цифр номера
        if (!normalized.StartsWith("+992") || normalized.Length != 13)
            return false;

        var numberPart = normalized.Substring(4);
        return numberPart.All(char.IsDigit);
    }
}
