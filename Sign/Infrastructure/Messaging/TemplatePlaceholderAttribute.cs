namespace Sign.Infrastructure.Messaging;

/// <summary>
/// Определяет имя плейсхолдера шаблона, связанного со свойством контекста сообщения.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TemplatePlaceholderAttribute : Attribute
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TemplatePlaceholderAttribute"/>.
    /// </summary>
    /// <param name="placeholderName">Имя плейсхолдера без обрамляющих символов.</param>
    public TemplatePlaceholderAttribute(string placeholderName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(placeholderName);
        PlaceholderName = placeholderName;
    }

    /// <summary>
    /// Получает имя плейсхолдера без обрамляющих символов.
    /// </summary>
    public string PlaceholderName { get; }
}
