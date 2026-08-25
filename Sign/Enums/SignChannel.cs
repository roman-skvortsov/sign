namespace Sign.Enums;

/// <summary>
/// Определяет канал доставки кода подтверждения.
/// </summary>
public enum SignChannel
{
    /// <summary>
    /// Канал подтверждения через SMS.
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Канал подтверждения через электронную почту.
    /// </summary>
    Email = 2
}
