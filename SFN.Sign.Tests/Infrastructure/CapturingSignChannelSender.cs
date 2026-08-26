using SFN.Sign.Abstractions.Messaging;
using SFN.Sign.Domain.Enums;
using SFN.Sign.Infrastructure.Messaging;

namespace SFN.Sign.Tests.Infrastructure;

/// <summary>
/// Представляет тестовый отправитель сообщений с сохранением всех отправленных сообщений в памяти.
/// </summary>
public sealed class CapturingSignChannelSender : ISignChannelSender
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CapturingSignChannelSender"/>.
    /// </summary>
    /// <param name="channel">Канал, поддерживаемый тестовым отправителем.</param>
    public CapturingSignChannelSender(SignChannel channel)
    {
        Channel = channel;
    }

    /// <summary>
    /// Получает канал, поддерживаемый тестовым отправителем.
    /// </summary>
    public SignChannel Channel { get; }

    /// <summary>
    /// Получает коллекцию сообщений, которые были отправлены в рамках теста.
    /// </summary>
    public IList<SignMessage> SentMessages { get; } = new List<SignMessage>();

    /// <summary>
    /// Сохраняет отправленное сообщение в тестовую коллекцию.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Завершенная задача.</returns>
    public Task SendAsync(SignMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        SentMessages.Add(new SignMessage
        {
            Channel = message.Channel,
            Recipient = message.Recipient,
            Subject = message.Subject,
            Body = message.Body
        });

        return Task.CompletedTask;
    }
}
