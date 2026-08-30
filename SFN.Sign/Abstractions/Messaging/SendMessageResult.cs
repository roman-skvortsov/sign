namespace SFN.Sign.Abstractions.Messaging;

/// <summary>
/// Результат отправки сообщения.
/// </summary>
public sealed class SendMessageResult
{
    /// <summary>
    /// Успешно ли отправлено сообщение.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Внешний идентификатор сообщения у провайдера.
    /// </summary>
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Код ошибки провайдера.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Текст ошибки отправки.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
