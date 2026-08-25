using Sign.Domain.Enums;

namespace Sign.Application.Contracts;

/// <summary>
/// Представляет результат запуска процесса подписания.
/// </summary>
public sealed class SigningResult
{
    /// <summary>
    /// Получает или задает признак успешного выполнения отправки кода.
    /// </summary>
    public bool IsSuccess { get; set; }

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

    /// <summary>
    /// Получает или задает текст бизнес-ошибки, если отправка кода недоступна.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Получает или задает дату и время, начиная с которых отправка снова будет доступна.
    /// </summary>
    public DateTimeOffset? NextAvailableAtUtc { get; set; }
}
