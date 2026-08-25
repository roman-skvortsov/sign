using Sign.Enums;

namespace Sign.Messaging;

/// <summary>
/// Определяет контракт отправки сообщений по каналу подтверждения.
/// </summary>
public interface ISignChannelSender
{
    /// <summary>
    /// Получает канал, поддерживаемый реализацией отправителя.
    /// </summary>
    SignChannel Channel { get; }

    /// <summary>
    /// Отправляет сформированное сообщение получателю.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Асинхронная задача отправки сообщения.</returns>
    Task SendAsync(SignMessage message, CancellationToken cancellationToken = default);
}
