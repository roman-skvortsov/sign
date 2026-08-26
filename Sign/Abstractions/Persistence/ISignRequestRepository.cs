using Sign.Domain.Entities;
using Sign.Domain.Enums;

namespace Sign.Abstractions.Persistence;

/// <summary>
/// Определяет контракт репозитория для повторно используемых запросов по операциям подписания.
/// </summary>
public interface ISignRequestRepository
{
    /// <summary>
    /// Возвращает запрос на подписание по идентификатору вместе с кодом и историей попыток.
    /// </summary>
    /// <param name="requestId">Идентификатор запроса на подписание.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Найденный запрос на подписание или <see langword="null"/>.</returns>
    Task<SignRequest?> GetByIdWithCodeAndAttemptsAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает активный запрос на подписание для документа, канала и получателя вместе с кодом и историей попыток.
    /// </summary>
    /// <param name="documentSignId">Идентификатор операции подписания документа.</param>
    /// <param name="channel">Канал подписания.</param>
    /// <param name="recipient">Адрес получателя кода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Активный запрос на подписание или <see langword="null"/>.</returns>
    Task<SignRequest?> GetActiveByDocumentSignIdAsync(
        Guid documentSignId,
        SignChannel channel,
        string recipient,
        CancellationToken cancellationToken = default);
}
