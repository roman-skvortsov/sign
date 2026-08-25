using Sign.Enums;

namespace Sign.Contracts;

/// <summary>
/// Представляет результат проверки кода подтверждения.
/// </summary>
public sealed class VerificationResult
{
    /// <summary>
    /// Получает или задает признак успешной проверки кода.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Получает или задает итоговый статус запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Получает или задает количество оставшихся попыток проверки.
    /// </summary>
    public int RemainingAttempts { get; set; }
}
