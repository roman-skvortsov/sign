using Sign.Domain.Enums;

namespace Sign.Domain.Entities;

/// <summary>
/// Запись о действии или попытке.
/// </summary>
public sealed class SignAttempt
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор запроса на подписание.
    /// </summary>
    public Guid SignRequestId { get; set; }

    /// <summary>
    /// Тип действия или попытки.
    /// </summary>
    public SignAttemptType Type { get; set; }

    /// <summary>
    /// Дополнительные сведения.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Связанный запрос на подписание.
    /// </summary>
    public SignRequest SignRequest { get; set; } = null!;
}
