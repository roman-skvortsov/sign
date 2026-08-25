namespace Sign.Messaging;

/// <summary>
/// Определяет контракт построения сообщений по шаблонам.
/// </summary>
public interface IMessageTemplateRenderer
{
    /// <summary>
    /// Формирует сообщение для отправки по каналу подтверждения.
    /// </summary>
    /// <param name="context">Контекст шаблона сообщения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Готовое сообщение для отправки.</returns>
    Task<SignMessage> RenderAsync(MessageTemplateContext context, CancellationToken cancellationToken = default);
}
