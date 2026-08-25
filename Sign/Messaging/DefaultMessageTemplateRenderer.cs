using Sign.Data.Repositories;
using System.Text.RegularExpressions;

namespace Sign.Messaging;

/// <summary>
/// Представляет реализацию построения сообщений на основе шаблонов, сохраненных в базе данных.
/// </summary>
public sealed class DefaultMessageTemplateRenderer : IMessageTemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new(
        "%%(?<placeholder>[A-Za-z0-9_]+)%%",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IMessageTemplateRepository _messageTemplateRepository;
    private readonly ITemplateValueProvider _templateValueProvider;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DefaultMessageTemplateRenderer"/>.
    /// </summary>
    /// <param name="messageTemplateRepository">Репозиторий шаблонов сообщений.</param>
    /// <param name="templateValueProvider">Сервис извлечения значений плейсхолдеров.</param>
    public DefaultMessageTemplateRenderer(
        IMessageTemplateRepository messageTemplateRepository,
        ITemplateValueProvider templateValueProvider)
    {
        _messageTemplateRepository = messageTemplateRepository;
        _templateValueProvider = templateValueProvider;
    }

    /// <inheritdoc />
    public async Task<SignMessage> RenderAsync(MessageTemplateContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var template = await _messageTemplateRepository.GetActiveByChannelAsync(context.Channel, cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException($"Для канала '{context.Channel}' не найден активный шаблон сообщения.");
        }

        var placeholderValues = _templateValueProvider.GetValues(context);

        return new SignMessage
        {
            Channel = context.Channel,
            Recipient = context.Recipient,
            Subject = RenderTemplate(template.SubjectTemplate, placeholderValues),
            Body = RenderTemplate(template.BodyTemplate, placeholderValues) ?? string.Empty
        };
    }

    /// <summary>
    /// Выполняет подстановку значений в шаблон сообщения.
    /// </summary>
    /// <param name="template">Строковый шаблон.</param>
    /// <param name="placeholderValues">Набор значений плейсхолдеров.</param>
    /// <returns>Строка с подставленными значениями.</returns>
    private static string? RenderTemplate(string? template, IReadOnlyDictionary<string, string?> placeholderValues)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var placeholderName = match.Groups["placeholder"].Value;

            return placeholderValues.TryGetValue(placeholderName, out var value)
                ? value ?? string.Empty
                : match.Value;
        });
    }
}
