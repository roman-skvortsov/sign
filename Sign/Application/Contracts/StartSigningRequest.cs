using Sign.Domain.Enums;

namespace Sign.Application.Contracts;

/// <summary>
/// Данные для запуска подписания.
/// </summary>
public sealed class StartSigningRequest
{
    /// <summary>
    /// Идентификатор подписания документа во внешней системе.
    /// </summary>
    public Guid DocumentSignId { get; set; }

    /// <summary>
    /// Канал подписания.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Адрес получателя кода.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;
}
