using Sign.Application.Codes;

namespace Sign.Abstractions.Codes;

/// <summary>
/// Определяет контракт генерации кода подтверждения.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Генерирует новый код подтверждения и его хеш.
    /// </summary>
    /// <param name="context">Контекст генерации кода.</param>
    /// <returns>Результат генерации кода.</returns>
    GeneratedCode Generate(CodeGenerationData context);
}
