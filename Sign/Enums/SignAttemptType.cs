namespace Sign.Enums;

/// <summary>
/// Определяет тип события или попытки в процессе подписания.
/// </summary>
public enum SignAttemptType
{
    /// <summary>
    /// Запрос на подписание создан.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Код подтверждения отправлен получателю.
    /// </summary>
    Sent = 2,

    /// <summary>
    /// При отправке кода произошла ошибка.
    /// </summary>
    SendFailed = 3,

    /// <summary>
    /// Введен неверный код подтверждения.
    /// </summary>
    VerifyFailed = 4,

    /// <summary>
    /// Код подтверждения успешно прошел проверку.
    /// </summary>
    VerifySucceeded = 5,

    /// <summary>
    /// Выполнена повторная отправка кода.
    /// </summary>
    Resent = 6,

    /// <summary>
    /// Срок действия запроса или кода истек.
    /// </summary>
    Expired = 7,

    /// <summary>
    /// Запрос заблокирован по бизнес-правилам.
    /// </summary>
    Blocked = 8
}
