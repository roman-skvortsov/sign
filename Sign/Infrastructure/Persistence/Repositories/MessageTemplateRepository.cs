using Microsoft.EntityFrameworkCore;
using Sign.Abstractions.Persistence;
using Sign.Domain.Entities;
using Sign.Domain.Enums;
using Sign.Infrastructure.Persistence;

namespace Sign.Infrastructure.Persistence.Repositories;

/// <summary>
/// Представляет репозиторий для повторно используемых запросов по шаблонам сообщений.
/// </summary>
public sealed class MessageTemplateRepository : IMessageTemplateRepository
{
    private readonly SignDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MessageTemplateRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных библиотеки подписания.</param>
    public MessageTemplateRepository(SignDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<MessageTemplate?> GetActiveByChannelAsync(SignChannel channel, CancellationToken cancellationToken = default)
    {
        return _dbContext.MessageTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Channel == channel && x.IsActive, cancellationToken);
    }
}
