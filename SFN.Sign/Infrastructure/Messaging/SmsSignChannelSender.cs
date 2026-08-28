using SFN.Sign.Abstractions.Messaging;
using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Отправитель сообщений для SMS.
/// </summary>
public sealed class SmsSignChannelSender : ISignChannelSender
{
    /// <summary>
    /// Канал, с которым работает отправитель.
    /// </summary>
    public SignChannel Channel => SignChannel.Sms;

    /// <summary>
    /// Отправляет SMS-сообщение.
    /// </summary>
    /// <param name="message">Готовое сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача отправки.</returns>
    public Task SendAsync(SignMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Channel != SignChannel.Sms)
        {
            throw new InvalidOperationException("SMS-отправитель может отправлять только SMS-сообщения.");
        }

        // TODO: Добавить реальную отправку SMS через выбранный HTTP API или SDK провайдера.
        return Task.CompletedTask;
    }
}
