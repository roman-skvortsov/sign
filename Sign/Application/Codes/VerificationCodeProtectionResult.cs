namespace Sign.Application.Codes;

/// <summary>
/// Представляет результат защиты кода подтверждения.
/// </summary>
public sealed class VerificationCodeProtectionResult
{
    /// <summary>
    /// Получает или задает хеш кода подтверждения.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает соль кода подтверждения.
    /// </summary>
    public string Salt { get; set; } = string.Empty;
}
