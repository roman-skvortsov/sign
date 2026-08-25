using Sign.Domain.Enums;

namespace Sign.Domain.Entities;

/// <summary>
/// Представляет запись журнала действий и попыток по процессу подписания.
/// </summary>
public sealed class SignAttempt
{
    /// <summary>
    /// Получает или задает идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Получает или задает идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Получает или задает тип события или попытки.
    /// </summary>
    public SignAttemptType Type { get; set; }

    /// <summary>
    /// Получает или задает дополнительные сведения о событии.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Получает или задает дату и время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Получает или задает связанный запрос на подписание.
    /// </summary>
    public SignRequest Request { get; set; } = null!;
}
