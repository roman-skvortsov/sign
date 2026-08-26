using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Application.Codes;

/// <summary>
/// Содержит данные, необходимые для генерации кода подтверждения.
/// </summary>
public sealed class CodeGenerationData
{
    /// <summary>
    /// Канал подписания.
    /// </summary>
    public SignChannel Channel { get; set; }
}
