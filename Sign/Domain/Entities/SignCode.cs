namespace Sign.Domain.Entities;

/// <summary>
/// Код подтверждения для запроса на подписание.
/// </summary>
public sealed class SignCode
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
    /// Хеш кода.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Соль кода.
    /// </summary>
    public string CodeSalt { get; set; } = string.Empty;

    /// <summary>
    /// Время создания кода.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Время окончания срока действия кода.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Показывает, был ли код использован.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Связанный запрос на подписание.
    /// </summary>
    public SignRequest SignRequest { get; set; } = null!;
}
