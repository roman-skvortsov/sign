using Sign.Domain.Entities;
using Sign.Domain.Enums;

namespace Sign.Abstractions.Persistence;

/// <summary>
/// Определяет контракт репозитория для повторно используемых запросов по шаблонам сообщений.
/// </summary>
public interface IMessageTemplateRepository
{
    /// <summary>
    /// Возвращает активный шаблон сообщения для указанного канала.
    /// </summary>
    /// <param name="channel">Канал подтверждения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Активный шаблон сообщения или <see langword="null"/>.</returns>
    Task<MessageTemplate?> GetActiveByChannelAsync(SignChannel channel, CancellationToken cancellationToken = default);
}
