using SFN.Sign.Application.Codes;

namespace SFN.Sign.Abstractions.Codes;

/// <summary>
/// Интерфейс генератора кода.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Создает новый код и его хеш.
    /// </summary>
    /// <param name="context">Данные для создания кода.</param>
    /// <returns>Новый код и данные для его хранения.</returns>
    GeneratedCode Generate(CodeGenerationData context);
}
