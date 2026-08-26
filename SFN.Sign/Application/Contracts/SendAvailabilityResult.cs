namespace SFN.Sign.Application.Contracts;

/// <summary>
/// Представляет результат проверки доступности отправки кода подтверждения.
/// </summary>
public sealed class SendAvailabilityResult
{
    /// <summary>
    /// Получает или задает признак доступности отправки кода.
    /// </summary>
    public bool CanSend { get; set; }

    /// <summary>
    /// Получает или задает текст причины недоступности отправки кода.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Получает или задает дату и время, начиная с которых отправка снова доступна.
    /// </summary>
    public DateTimeOffset? NextAvailableAtUtc { get; set; }
}
