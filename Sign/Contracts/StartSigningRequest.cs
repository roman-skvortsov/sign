using Sign.Enums;

namespace Sign.Contracts;

/// <summary>
/// Представляет входные данные для запуска процесса подписания.
/// </summary>
public sealed class StartSigningRequest
{
    /// <summary>
    /// Получает или задает идентификатор операции подписания документа во внешней системе.
    /// </summary>
    public string DocumentSignId { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает канал подтверждения.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Получает или задает адрес получателя кода подтверждения.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;
}
