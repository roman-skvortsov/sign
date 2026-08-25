using Microsoft.EntityFrameworkCore;
using Sign.Abstractions.Persistence;
using Sign.Domain.Entities;
using Sign.Domain.Enums;
using Sign.Infrastructure.Persistence;

namespace Sign.Infrastructure.Persistence.Repositories;

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

    /// <inheritdoc />
    public Task<SignRequest?> GetActiveByDocumentSignIdAsync(
        string documentSignId,
        SignChannel channel,
        string recipient,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentSignId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);

        return _dbContext.Requests
            .Include(x => x.Code)
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(
                x => x.DocumentSignId == documentSignId
                    && x.Channel == channel
                    && x.Recipient == recipient
                    && x.Status != SignRequestStatus.Signed
                    && x.Status != SignRequestStatus.Cancelled
                    && x.Status != SignRequestStatus.Expired
                    && x.Status != SignRequestStatus.Blocked,
                cancellationToken);
    }
}
