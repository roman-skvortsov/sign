namespace Sign.Application.Contracts;

/// <summary>
/// Представляет входные данные для проверки кода подтверждения.
/// </summary>
public sealed class VerifySigningCodeRequest
{
    /// <summary>
    /// Получает или задает идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Получает или задает код, введенный пользователем.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
