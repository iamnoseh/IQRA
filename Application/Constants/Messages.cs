namespace Application.Constants;

public static class Messages
{
    public static class OsonSms
    {
        public const string SendSuccess = "SMS бомуваффақият ирсол шуд";
        public const string SendError = "Хатогӣ ҳангоми ирсоли SMS";
        public const string StatusSuccess = "Статуси SMS бомуваффақият гирифта шуд";
        public const string StatusError = "Хатогӣ ҳангоми гирифтани статуси SMS";
        public const string BalanceSuccess = "Баланс бомуваффақият гирифта шуд";
        public const string BalanceError = "Хатогӣ ҳангоми гирифтани баланс";
        public const string Error = "Хатогӣ дар хадамоти SMS: {0}";
    }
    
    public static class Auth
    {
        public const string PasswordSent = "Парол ба рақами телефон равон карда шуд";
        public const string UserNotFound = "Корбар ёфт нашуд";
        public const string InvalidPassword = "Парол нодуруст аст";
        public const string UserAlreadyExists = "Корбар бо ин рақам аллакай вуҷуд дорад";
        public const string RegistrationSuccess = "Бақайдгирӣ бомуваффақият анҷом ёфт";
        public const string LoginSuccess = "Даромад бомуваффақият анҷом ёфт";
        public const string InvalidCredentials = "Рақами телефон ё парол нодуруст аст";
        public const string PasswordGenerationError = "Хатогӣ ҳангоми генератсияи парол";
    }
}