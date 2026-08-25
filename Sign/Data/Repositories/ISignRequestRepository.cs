using Sign.Entities;

namespace Sign.Data.Repositories;

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
}
