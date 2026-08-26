using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Данные для сборки сообщения по шаблону.
/// </summary>
public sealed class MessageTemplateData
{
    /// <summary>
    /// Идентификатор подписания документа.
    /// </summary>
    [TemplatePlaceholder("DocumentSignId")]
    public Guid DocumentSignId { get; set; }

    /// <summary>
    /// Идентификатор запроса на подписание.
    /// </summary>
    [TemplatePlaceholder("RequestId")]
    public Guid RequestId { get; set; }

    /// <summary>
    /// Канал подписания.
    /// </summary>
    [TemplatePlaceholder("Channel")]
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Адрес получателя.
    /// </summary>
    [TemplatePlaceholder("Recipient")]
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Код подтверждения.
    /// </summary>
    [TemplatePlaceholder("SignCode")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Время окончания срока действия кода.
    /// </summary>
    [TemplatePlaceholder("ExpiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>
    /// Дополнительные значения заменяемых полей.
    /// </summary>
    public IDictionary<string, string?> PlaceholderValues { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}
