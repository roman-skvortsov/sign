using Microsoft.EntityFrameworkCore;
using Sign.Entities;

namespace Sign.Data.Repositories;

/// <summary>
/// Представляет репозиторий для повторно используемых запросов по операциям подписания.
/// </summary>
public sealed class SignRequestRepository : ISignRequestRepository
{
    private readonly SignDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SignRequestRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных библиотеки подписания.</param>
    public SignRequestRepository(SignDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<SignRequest?> GetByIdWithCodeAndAttemptsAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Requests
            .Include(x => x.Code)
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);
    }
}
