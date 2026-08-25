namespace Sign.Domain.Entities;

/// <summary>
/// Представляет код подтверждения, связанный с запросом на подписание.
/// </summary>
public sealed class SignCode
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
    /// Получает или задает хеш кода подтверждения.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает соль кода подтверждения.
    /// </summary>
    public string CodeSalt { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает дату и время создания кода.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Получает или задает дату и время истечения кода.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Получает или задает признак использования кода.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Получает или задает связанный запрос на подписание.
    /// </summary>
    public SignRequest Request { get; set; } = null!;
}
