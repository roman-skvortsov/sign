using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Application.Contracts;

/// <summary>
/// Результат запуска подписания.
/// </summary>
public sealed class StartSigningResult
{
    /// <summary>
    /// Успешна ли отправка кода.
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
    /// Текст ошибки, если код не удалось отправить.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Время, когда отправка снова станет доступна.
    /// </summary>
    public DateTimeOffset? NextAvailableAtUtc { get; set; }
}
