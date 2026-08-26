using Sign.Domain.Enums;

namespace Sign.Application.Contracts;

/// <summary>
/// Результат повторной отправки кода.
/// </summary>
public sealed class ResendCodeResult
{
    /// <summary>
    /// Успешна ли повторная отправка кода.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Идентификатор подписания документа.
    /// </summary>
    public Guid DocumentSignId { get; set; }

    /// <summary>
    /// Текущий статус запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Время окончания срока действия кода.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Текст ошибки, если код нельзя отправить повторно.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Время, когда повторная отправка снова станет доступна.
    /// </summary>
    public DateTimeOffset? NextAvailableAtUtc { get; set; }
}
