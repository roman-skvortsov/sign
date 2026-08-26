using Sign.Domain.Enums;

namespace Sign.Infrastructure.Messaging;

/// <summary>
/// Содержит данные для построения сообщения по шаблону.
/// </summary>
public sealed class MessageTemplateData
{
    /// <summary>
    /// Получает или задает идентификатор операции подписания документа.
    /// </summary>
    [TemplatePlaceholder("DocumentSignId")]
    public Guid DocumentSignId { get; set; }

    /// <summary>
    /// Получает или задает идентификатор запроса на подписание.
    /// </summary>
    [TemplatePlaceholder("RequestId")]
    public Guid RequestId { get; set; }

    /// <summary>
    /// Получает или задает канал подтверждения.
    /// </summary>
    [TemplatePlaceholder("Channel")]
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Получает или задает адрес получателя.
    /// </summary>
    [TemplatePlaceholder("Recipient")]
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает исходный код подтверждения.
    /// </summary>
    [TemplatePlaceholder("SignCode")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает дату и время истечения кода.
    /// </summary>
    [TemplatePlaceholder("ExpiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Получает словарь дополнительных значений для подстановки в шаблон.
    /// </summary>
    public IDictionary<string, string?> PlaceholderValues { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}
