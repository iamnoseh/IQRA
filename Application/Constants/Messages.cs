namespace Application.Constants;

public static class Messages
{
    public static class OsonSms
    {
        public const string SendSuccess = "SMS успешно отправлено";
        public const string SendError = "Ошибка при отправке SMS";
        public const string StatusSuccess = "Статус SMS успешно получен";
        public const string StatusError = "Ошибка при получении статуса SMS";
        public const string BalanceSuccess = "Баланс успешно получен";
        public const string BalanceError = "Ошибка при получении баланса";
        public const string Error = "Ошибка в сервисе SMS: {0}";
    }
    
    public static class Auth
    {
        public const string LoginSuccess = "Вход успешно выполнен";
        public const string InvalidCredentials = "Неверное имя пользователя или пароль";
        public const string UserNotFound = "Пользователь не найден";
        public const string PasswordChanged = "Пароль успешно изменен";
        public const string PasswordChangeError = "Ошибка при изменении пароля: {0}";
        public const string OtpSent = "Код подтверждения отправлен на номер/email";
        public const string OtpSendError = "Ошибка при отправке OTP: {0}";
        public const string OtpVerified = "Код подтвержден";
        public const string OtpInvalid = "Неверный код подтверждения";
        public const string OtpExpired = "Срок действия кода подтверждения истек";
        public const string OtpVerifyError = "Ошибка при проверке OTP: {0}";
        public const string PasswordReset = "Пароль успешно сброшен";
        public const string PasswordResetError = "Ошибка при сбросе пароля: {0}";
        public const string TokenInvalid = "Неверный токен";
        public const string TokenExpired = "Срок действия токена истек";
        public const string TokenUsedOrInvalid = "Токен уже использован или недействителен";
        public const string PasswordsNotMatch = "Пароли не совпадают";
        public const string UsernameRequired = "Имя пользователя обязательно";
        public const string UserNotFoundByUsername = "Пользователь с таким именем не найден";
        public const string OtpCreationError = "Ошибка при создании кода подтверждения";
        public const string UsernameAndOtpRequired = "Имя пользователя и код подтверждения обязательны";
        public const string TokenAndPasswordRequired = "Токен и пароль обязательны";
        public const string UserNotAuthenticated = "Пользователь не авторизован";

        public const string InvalidPhoneFormat = "Неверный формат номера телефона. Формат: +992XXXXXXXXX или 9XXXXXXXX";
        public const string UserAlreadyExists = "Пользователь с таким номером уже существует. Пожалуйста, используйте другой номер или войдите в систему.";
        public const string RegistrationError = "Ошибка при регистрации";
        public const string RegistrationSuccessSms = "Привет! Вы зарегистрированы в IQRA.\n\n ПАРОЛЬ: {0}\n\nХРАНИТЕ ЭТОТ ПАРОЛЬ В СЕКРЕТЕ!\nДля входа используйте свой номер телефона.\n\nIQRA.tj";
        public const string PasswordResetSms = "IQRA - Сброс пароля\n\nКОД ПОДТВЕРЖДЕНИЯ: {0}\n\nСрок: 3 минуты\nЕсли вы не запрашивали этот код, проигнорируйте сообщение.\n\nIQRA.tj";
    }
}
