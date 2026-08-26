namespace SFN.Sign.Domain.Enums;

/// <summary>
/// Определяет канал доставки кода подтверждения.
/// </summary>
public enum SignChannel
{
    /// <summary>
    /// Подписание через SMS.
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Подписание через электронную почту.
    /// </summary>
    Email = 2
}
