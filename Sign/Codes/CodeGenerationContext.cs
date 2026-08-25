using Sign.Enums;

namespace Sign.Codes;

/// <summary>
/// Содержит данные, необходимые для генерации кода подтверждения.
/// </summary>
public sealed class CodeGenerationContext
{
    /// <summary>
    /// Получает или задает канал подтверждения.
    /// </summary>
    public SignChannel Channel { get; set; }
}
