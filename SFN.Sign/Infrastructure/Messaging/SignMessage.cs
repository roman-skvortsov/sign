using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Готовое сообщение для отправки.
/// </summary>
public sealed class SignMessage
{
    /// <summary>
    /// Канал сообщения.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Адрес получателя.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Тема сообщения.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
