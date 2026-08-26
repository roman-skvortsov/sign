namespace Sign.Infrastructure.Messaging;

/// <summary>
/// Задает имя заменяемого поля шаблона, связанного со свойством данных сообщения.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TemplatePlaceholderAttribute : Attribute
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TemplatePlaceholderAttribute"/>.
    /// </summary>
    /// <param name="placeholderName">Имя заменяемого поля без обрамляющих символов.</param>
    public TemplatePlaceholderAttribute(string placeholderName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(placeholderName);
        PlaceholderName = placeholderName;
    }

    /// <summary>
    /// Имя заменяемого поля без обрамляющих символов.
    /// </summary>
    public string PlaceholderName { get; }
}
