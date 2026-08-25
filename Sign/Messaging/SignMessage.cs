using Sign.Enums;

namespace Sign.Messaging;

/// <summary>
/// Представляет готовое сообщение для отправки по каналу подтверждения.
/// </summary>
public sealed class SignMessage
{
    /// <summary>
    /// Получает или задает канал сообщения.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Получает или задает адрес получателя сообщения.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает тему сообщения.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Получает или задает тело сообщения.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
