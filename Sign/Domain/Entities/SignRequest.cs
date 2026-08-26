using Sign.Domain.Enums;

namespace Sign.Domain.Entities;

/// <summary>
/// Запрос на подписание документа.
/// </summary>
public sealed class SignRequest
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Внешний идентификатор подписания документа.
    /// </summary>
    public Guid DocumentSignId { get; set; }

    /// <summary>
    /// Канал подписания.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Адрес получателя кода.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Текущее состояние запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Время создания запроса.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Время окончания срока действия запроса.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Время успешного подписания.
    /// </summary>
    public DateTimeOffset? SignedAtUtc { get; set; }

    /// <summary>
    /// Значение для проверки одновременных изменений записи.
    /// </summary>
    public uint Version { get; set; }

    /// <summary>
    /// Сколько попыток проверки уже использовано.
    /// </summary>
    public int VerifyAttemptsUsed { get; set; }

    /// <summary>
    /// Сколько раз код уже отправлялся.
    /// </summary>
    public int SendAttemptsUsed { get; set; }

    /// <summary>
    /// Текущий код подтверждения.
    /// </summary>
    public SignCode? SignCode { get; set; }

    /// <summary>
    /// История действий по запросу.
    /// </summary>
    public ICollection<SignAttempt> Attempts { get; set; } = new List<SignAttempt>();
}
