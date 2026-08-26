using Sign.Infrastructure.Messaging;

namespace Sign.Abstractions.Messaging;

/// <summary>
/// Интерфейс получения значений заменяемых полей из данных шаблона.
/// </summary>
public interface ITemplateValueProvider
{
    /// <summary>
    /// Получает значения заменяемых полей из переданных данных.
    /// </summary>
    /// <param name="context">Контекст шаблона сообщения.</param>
    /// <returns>Словарь заменяемых полей и их значений.</returns>
    IReadOnlyDictionary<string, string?> GetValues(MessageTemplateData context);
}
