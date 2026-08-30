using SFN.Sign.Abstractions.Messaging;
using SFN.ApiClients.SMS;
using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Отправитель сообщений для SMS.
/// </summary>
public sealed class SmsSignChannelSender : ISignChannelSender
{
    private readonly ISmsApiClient _smsApiClient;

    /// <summary>
    /// Создает отправитель SMS-сообщений.
    /// </summary>
    /// <param name="smsApiClient">Клиент SMS-сервиса.</param>
    public SmsSignChannelSender(ISmsApiClient smsApiClient)
    {
        _smsApiClient = smsApiClient;
    }

    /// <summary>
    /// Канал, с которым работает отправитель.
    /// </summary>
    public SignChannel Channel => SignChannel.Sms;

    /// <summary>
    /// Отправляет SMS-сообщение.
    /// </summary>
    /// <param name="message">Готовое сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки.</returns>
    public async Task<SendMessageResult> SendAsync(SignMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Channel != SignChannel.Sms)
        {
            throw new InvalidOperationException("SMS-отправитель может отправлять только SMS-сообщения.");
        }

        var response = await _smsApiClient.SendSmsAsync(new SendSmsRequest
        {
            SmsText = message.Body,
            MobilePhone = message.Recipient,
            NeedSendSms = true
        }, cancellationToken);

        if (!response.IsSuccessful)
        {
            return new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = "SmsApiRequestFailed",
                ErrorMessage = response.Error?.Content ?? response.Error?.Message ?? "Сервис SMS вернул ошибку."
            };
        }

        if (response.Content is null)
        {
            return new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = "SmsEmptyResponse",
                ErrorMessage = "Сервис SMS вернул пустой ответ."
            };
        }

        return response.Content.SmsStatus == SmsStatus.Sent
            ? new SendMessageResult
            {
                IsSuccess = true,
                ProviderMessageId = response.Content.SmsId.ToString()
            }
            : new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = response.Content.SmsStatus.ToString(),
                ErrorMessage = "Сервис SMS вернул ошибку отправки."
            };
    }
}
