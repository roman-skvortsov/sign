using SFN.Sign.Abstractions.Messaging;
using SFN.ApiClients.DPV;
using SFN.Sign.Domain.Enums;

namespace SFN.Sign.Infrastructure.Messaging;

/// <summary>
/// Отправитель сообщений для электронной почты.
/// </summary>
public sealed class EmailSignChannelSender : ISignChannelSender
{
    private readonly IDpvApiClient _dpvApiClient;

    /// <summary>
    /// Создает отправитель email-сообщений.
    /// </summary>
    /// <param name="dpvApiClient">Клиент DPV-сервиса.</param>
    public EmailSignChannelSender(IDpvApiClient dpvApiClient)
    {
        _dpvApiClient = dpvApiClient;
    }

    /// <summary>
    /// Канал, с которым работает отправитель.
    /// </summary>
    public SignChannel Channel => SignChannel.Email;

    /// <summary>
    /// Отправляет сообщение по электронной почте.
    /// </summary>
    /// <param name="message">Готовое сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки.</returns>
    public async Task<SendMessageResult> SendAsync(SignMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Channel != SignChannel.Email)
        {
            throw new InvalidOperationException("Email-отправитель может отправлять только email-сообщения.");
        }

        if (!Guid.TryParse(message.Recipient, out var dpvUserId))
        {
            return new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = "InvalidDpvUserId",
                ErrorMessage = "Для отправки email в Recipient должен быть передан DpvUserId в формате Guid."
            };
        }

        var response = await _dpvApiClient.SendEmailAsync(new SendEmailRequest
        {
            DpvUserId = dpvUserId,
            EmailHeader = message.Subject ?? string.Empty,
            EmailBody = message.Body
        }, cancellationToken);

        if (!response.IsSuccessful)
        {
            return new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = "DpvApiRequestFailed",
                ErrorMessage = response.Error?.Content ?? response.Error?.Message ?? "Сервис email вернул ошибку."
            };
        }

        if (response.Content is null)
        {
            return new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = "EmailEmptyResponse",
                ErrorMessage = "Сервис email вернул пустой ответ."
            };
        }

        return response.Content.Status == SendEmailStatus.Success
            ? new SendMessageResult
            {
                IsSuccess = true,
                ProviderMessageId = response.Content.DpvUserId.ToString()
            }
            : new SendMessageResult
            {
                IsSuccess = false,
                ErrorCode = response.Content.Status.ToString(),
                ErrorMessage = response.Content.Error ?? "Сервис email вернул ошибку отправки."
            };
    }
}
