namespace SFN.Sign.Application.Contracts;

/// <summary>
/// Данные для повторной отправки кода.
/// </summary>
public sealed class ResendCodeRequest
{
    /// <summary>
    /// Идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }
}
