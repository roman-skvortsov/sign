using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Application.Contracts;

/// <summary>
/// Результат проверки кода.
/// </summary>
public sealed class VerifyCodeResult
{
    /// <summary>
    /// Успешна ли проверка кода.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Текущий статус запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Сколько попыток осталось.
    /// </summary>
    public int RemainingAttempts { get; set; }

    /// <summary>
    /// Сколько попыток проверки кода осталось.
    /// </summary>
    public int RemainingVerifyAttempts { get; set; }

    /// <summary>
    /// Сколько попыток отправки осталось.
    /// </summary>
    public int RemainingSendAttempts { get; set; }

    /// <summary>
    /// Текст ошибки, если код не удалось проверить.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
