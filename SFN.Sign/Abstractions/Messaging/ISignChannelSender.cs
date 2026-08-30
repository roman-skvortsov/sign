using SFN.Sign.Domain.Enums;
using SFN.Sign.Infrastructure.Messaging;

namespace SFN.Sign.Abstractions.Messaging;

/// <summary>
/// Интерфейс отправки сообщений.
/// </summary>
public interface ISignChannelSender
{
    /// <summary>
    /// Канал, с которым работает отправитель.
    /// </summary>
    SignChannel Channel { get; }

    /// <summary>
    /// Отправляет готовое сообщение.
    /// TODO: Добавить реальные реализации отправки для Email и Sms через внешние библиотеки или провайдеры.
    /// TODO: Для Email рекомендуется отдельная реализация с SMTP или email-провайдером.
    /// TODO: Для Sms рекомендуется отдельная реализация с HTTP API или SDK выбранного SMS-провайдера.
    /// </summary>
    /// <param name="message">Готовое сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки.</returns>
    Task<SendMessageResult> SendAsync(SignMessage message, CancellationToken cancellationToken = default);
}
