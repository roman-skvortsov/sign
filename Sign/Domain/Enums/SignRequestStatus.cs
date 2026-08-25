namespace Sign.Domain.Enums;

/// <summary>
/// Определяет текущее состояние запроса на подписание.
/// </summary>
public enum SignRequestStatus
{
    /// <summary>
    /// Запрос создан, но код еще не отправлен.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Код подтверждения успешно отправлен получателю.
    /// </summary>
    CodeSent = 2,

    /// <summary>
    /// Документ успешно подписан.
    /// </summary>
    Signed = 3,

    /// <summary>
    /// Срок действия запроса истек.
    /// </summary>
    Expired = 4,

    /// <summary>
    /// Запрос заблокирован из-за превышения числа попыток.
    /// </summary>
    Blocked = 5,

    /// <summary>
    /// Запрос отменен внешней системой.
    /// </summary>
    Cancelled = 6
}
