using SFN.Sign.Abstractions.Messaging;
using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Отправитель сообщений для электронной почты.
/// </summary>
public sealed class EmailSignChannelSender : ISignChannelSender
{
    /// <summary>
    /// Канал, с которым работает отправитель.
    /// </summary>
    public SignChannel Channel => SignChannel.Email;

    /// <summary>
    /// Отправляет сообщение по электронной почте.
    /// </summary>
    /// <param name="message">Готовое сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача отправки.</returns>
    public Task SendAsync(SignMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Channel != SignChannel.Email)
        {
            throw new InvalidOperationException("Email-отправитель может отправлять только email-сообщения.");
        }

        // TODO: Добавить реальную отправку email через SMTP или выбранный email-провайдер.
        return Task.CompletedTask;
    }
}
