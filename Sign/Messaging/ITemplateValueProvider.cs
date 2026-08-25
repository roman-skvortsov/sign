namespace Sign.Messaging;

/// <summary>
/// Определяет контракт извлечения значений плейсхолдеров из контекста шаблона.
/// </summary>
public interface ITemplateValueProvider
{
    /// <summary>
    /// Извлекает значения плейсхолдеров из переданного контекста.
    /// </summary>
    /// <param name="context">Контекст шаблона сообщения.</param>
    /// <returns>Словарь плейсхолдеров и их значений.</returns>
    IReadOnlyDictionary<string, string?> GetValues(MessageTemplateContext context);
}
