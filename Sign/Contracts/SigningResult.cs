using Sign.Enums;

namespace Sign.Contracts;

/// <summary>
/// Представляет результат запуска процесса подписания.
/// </summary>
public sealed class SigningResult
{
    /// <summary>
    /// Получает или задает идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Получает или задает идентификатор операции подписания документа.
    /// </summary>
    public string DocumentSignId { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает итоговый статус запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Получает или задает дату и время истечения кода.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
