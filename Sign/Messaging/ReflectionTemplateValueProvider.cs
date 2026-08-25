using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Sign.Messaging;

/// <summary>
/// Представляет реализацию извлечения значений плейсхолдеров из контекста сообщения через reflection с кэшированием.
/// </summary>
public sealed class ReflectionTemplateValueProvider : ITemplateValueProvider
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PlaceholderDescriptor>> Cache = new();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string?> GetValues(MessageTemplateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var descriptors = Cache.GetOrAdd(context.GetType(), BuildDescriptors);
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in descriptors)
        {
            var rawValue = descriptor.Property.GetValue(context);
            values[descriptor.PlaceholderName] = ConvertToString(rawValue);
        }

        foreach (var additionalValue in context.PlaceholderValues)
        {
            values[additionalValue.Key] = additionalValue.Value;
        }

        return values;
    }

    /// <summary>
    /// Формирует набор дескрипторов плейсхолдеров для указанного типа контекста.
    /// </summary>
    /// <param name="contextType">Тип контекста шаблона.</param>
    /// <returns>Набор дескрипторов плейсхолдеров.</returns>
    private static IReadOnlyCollection<PlaceholderDescriptor> BuildDescriptors(Type contextType)
    {
        return contextType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => new
            {
                Property = property,
                Attribute = property.GetCustomAttribute<TemplatePlaceholderAttribute>()
            })
            .Where(x => x.Attribute is not null)
            .Select(x => new PlaceholderDescriptor(x.Attribute!.PlaceholderName, x.Property))
            .ToArray();
    }

    /// <summary>
    /// Преобразует значение свойства в строковое представление для шаблона.
    /// </summary>
    /// <param name="value">Исходное значение свойства.</param>
    /// <returns>Строковое представление значения.</returns>
    private static string? ConvertToString(object? value)
    {
        return value switch
        {
            null => null,
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Представляет описание плейсхолдера и связанного свойства контекста.
    /// </summary>
    /// <param name="PlaceholderName">Имя плейсхолдера.</param>
    /// <param name="Property">Свойство контекста.</param>
    private sealed record PlaceholderDescriptor(string PlaceholderName, PropertyInfo Property);
}
