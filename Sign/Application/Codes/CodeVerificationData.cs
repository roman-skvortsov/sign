namespace Sign.Application.Codes;

/// <summary>
/// Содержит данные, необходимые для проверки кода подтверждения.
/// </summary>
public sealed class CodeVerificationData
{
    /// <summary>
    /// Получает или задает код, введенный пользователем.
    /// </summary>
    public string InputCode { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает сохраненный хеш кода подтверждения.
    /// </summary>
    public string StoredCodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает сохраненную соль кода подтверждения.
    /// </summary>
    public string StoredCodeSalt { get; set; } = string.Empty;
}
