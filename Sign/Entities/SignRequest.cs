using Sign.Enums;

namespace Sign.Entities;

/// <summary>
/// Представляет запрос на подписание документа через выбранный канал подтверждения.
/// </summary>
public sealed class SignRequest
{
    /// <summary>
    /// Получает или задает идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Получает или задает внешний идентификатор операции подписания документа.
    /// </summary>
    public string DocumentSignId { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает канал подтверждения.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Получает или задает адрес получателя кода подтверждения.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает текущее состояние запроса.
    /// </summary>
    public SignRequestStatus Status { get; set; }

    /// <summary>
    /// Получает или задает дату и время создания запроса.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Получает или задает дату и время истечения запроса.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Получает или задает дату и время успешного подписания.
    /// </summary>
    public DateTimeOffset? SignedAtUtc { get; set; }

    /// <summary>
    /// Получает или задает уже использованное число попыток проверки кода.
    /// </summary>
    public int VerifyAttemptsUsed { get; set; }

    /// <summary>
    /// Получает или задает уже использованное число отправок кода.
    /// </summary>
    public int SendAttemptsUsed { get; set; }

    /// <summary>
    /// Получает или задает активный код подтверждения.
    /// </summary>
    public SignCode? Code { get; set; }

    /// <summary>
    /// Получает коллекцию записей аудита по запросу.
    /// </summary>
    public ICollection<SignAttempt> Attempts { get; set; } = new List<SignAttempt>();
}
