using Sign.Enums;

namespace Sign.Entities;

/// <summary>
/// Представляет шаблон сообщения для канала подтверждения, хранящийся в базе данных.
/// </summary>
public sealed class MessageTemplate
{
    /// <summary>
    /// Получает или задает идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Получает или задает канал подтверждения, для которого предназначен шаблон.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Получает или задает уникальный код шаблона.
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает тему сообщения.
    /// </summary>
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// Получает или задает шаблон тела сообщения.
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает признак активности шаблона.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Получает или задает дату и время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Получает или задает дату и время последнего обновления записи.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
