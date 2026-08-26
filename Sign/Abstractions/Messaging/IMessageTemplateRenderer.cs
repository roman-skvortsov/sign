using Sign.Infrastructure.Messaging;

namespace Sign.Abstractions.Messaging;

/// <summary>
/// Определяет контракт построения сообщений по шаблонам.
/// </summary>
public interface IMessageTemplateRenderer
{
    /// <summary>
    /// Собирает сообщение для отправки по каналу подписания.
    /// </summary>
    /// <param name="context">Контекст шаблона сообщения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Готовое сообщение для отправки.</returns>
    Task<SignMessage> RenderAsync(MessageTemplateData context, CancellationToken cancellationToken = default);
}
