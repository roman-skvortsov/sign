namespace SFN.Sign.Application.Codes;

/// <summary>
/// Представляет результат генерации кода подтверждения.
/// </summary>
public sealed class GeneratedCode
{
    /// <summary>
    /// Получает или задает исходный код подтверждения.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает хеш кода подтверждения.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает соль кода подтверждения.
    /// </summary>
    public string Salt { get; set; } = string.Empty;

}
