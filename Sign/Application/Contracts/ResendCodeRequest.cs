namespace Sign.Application.Contracts;

/// <summary>
/// Представляет входные данные для повторной отправки кода подтверждения.
/// </summary>
public sealed class ResendCodeRequest
{
    /// <summary>
    /// Получает или задает идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }
}
