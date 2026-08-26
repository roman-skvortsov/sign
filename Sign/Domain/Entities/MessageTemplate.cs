using Sign.Domain.Enums;

namespace Sign.Domain.Entities;

/// <summary>
/// Шаблон сообщения из базы данных.
/// </summary>
public sealed class MessageTemplate
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Канал, для которого нужен шаблон.
    /// </summary>
    public SignChannel Channel { get; set; }

    /// <summary>
    /// Уникальный код шаблона.
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// Тема сообщения.
    /// </summary>
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// Шаблон текста сообщения.
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Показывает, активен ли шаблон.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Время последнего обновления записи.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
