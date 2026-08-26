namespace SFN.Sign.Application.Contracts;

/// <summary>
/// Данные для проверки кода.
/// </summary>
public sealed class VerifyCodeRequest
{
    /// <summary>
    /// Идентификатор запроса на подписание.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Код, который ввел пользователь.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
