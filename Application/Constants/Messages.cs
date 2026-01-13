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
        public const string LoginSuccess = "Даромад бомуваффақият анҷом ёфт";
        public const string InvalidCredentials = "Номи корбар ё парол нодуруст аст";
        public const string UserNotFound = "Корбар ёфт нашуд";
        public const string PasswordChanged = "Парол бомуваффақият иваз карда шуд";
        public const string PasswordChangeError = "Хатогӣ ҳангоми иваз кардани парол: {0}";
        public const string OtpSent = "Рамзи тасдиқ ба рақам/email равон карда шуд";
        public const string OtpSendError = "Хатогӣ ҳангоми ирсоли OTP: {0}";
        public const string OtpVerified = "Рамз тасдиқ карда шуд";
        public const string OtpInvalid = "Рамзи тасдиқ нодуруст аст";
        public const string OtpExpired = "Рамзи тасдиқ expired шудааст";
        public const string OtpVerifyError = "Хатогӣ ҳангоми тасдиқи OTP: {0}";
        public const string PasswordReset = "Парол бомуваффақият иваз карда шуд";
        public const string PasswordResetError = "Хатогӣ ҳангоми иваз кардани парол: {0}";
        public const string TokenInvalid = "Token нодуруст аст";
        public const string TokenExpired = "Token expired шудааст";
        public const string TokenUsedOrInvalid = "Token аллакай истифода шудааст ё нодуруст аст";
        public const string PasswordsNotMatch = "Паролҳо мувофиқат намекунанд";
        public const string UsernameRequired = "Номи корбар ҳатмист";
        public const string UserNotFoundByUsername = "Корбар бо ин номи корбар ёфт нашуд";
        public const string OtpCreationError = "Хатогӣ ҳангоми эҷоди рамзи тасдиқ";
        public const string UsernameAndOtpRequired = "Номи корбар ва рамзи тасдиқ ҳатмист";
        public const string TokenAndPasswordRequired = "Token ва парол ҳатмист";
        public const string UserNotAuthenticated = "Корбар аутентификатсия нашудааст";

        public const string InvalidPhoneFormat = "Рақами телефон нодуруст аст. Формат: +992XXXXXXXXX ё 9XXXXXXXX";
        public const string UserAlreadyExists = "Корбар бо ин рақам аллакай вуҷуд дорад";
        public const string RegistrationError = "Хатогӣ ҳангоми бақайдгирӣ";
        public const string RegistrationSuccessSms = "Салом! Шумо дар IQRA ба қайд гирифта шудед.\n\n РАМЗ: {0}\n\nИН РАМЗРО МАХФӢ НИГОҲ ДОРЕД!\nБарои даромад рақами телефони худро истифода баред.\n\nIQRA.tj";
        public const string PasswordResetSms = "IQRA - Барқароркунии парол\n\nРАМЗИ ТАСДИҚ: {0}\n\nМуҳлат: 3 дақиқа\nАгар шумо дархост накардед, ин паёмро рад кунед.\n\nIQRA.tj";
    }
}