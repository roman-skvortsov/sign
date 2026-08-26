using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sign.Domain.Entities;
using Sign.Configuration;

namespace Sign.Infrastructure.Persistence;

/// <summary>
/// Представляет контекст базы данных библиотеки подписания документов.
/// </summary>
public sealed class SignDbContext : DbContext
{
    private readonly string _schema;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SignDbContext"/>.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста.</param>
    /// <param name="signOptions">Настройки библиотеки подписания.</param>
    public SignDbContext(DbContextOptions<SignDbContext> options, IOptions<SignOptions> signOptions)
        : base(options)
    {
        _schema = signOptions.Value.Schema;
    }

    /// <summary>
    /// Получает набор запросов на подписание.
    /// </summary>
    public DbSet<SignRequest> Requests => Set<SignRequest>();

    /// <summary>
    /// Получает набор кодов подтверждения.
    /// </summary>
    public DbSet<SignCode> Codes => Set<SignCode>();

    /// <summary>
    /// Получает набор записей аудита по процессу подписания.
    /// </summary>
    public DbSet<SignAttempt> Attempts => Set<SignAttempt>();

    /// <summary>
    /// Получает набор шаблонов сообщений по каналам подтверждения.
    /// </summary>
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var isNpgsqlProvider = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        modelBuilder.HasDefaultSchema(_schema);

        modelBuilder.Entity<SignRequest>(builder =>
        {
            builder.ToTable("SignRequests", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentSignId).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Recipient).HasMaxLength(320).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).IsRequired();

            if (isNpgsqlProvider)
            {
                builder.Property(x => x.Version).IsRowVersion();
            }
            else
            {
                builder.Ignore(x => x.Version);
            }

            builder.Property(x => x.VerifyAttemptsUsed).IsRequired();
            builder.Property(x => x.SendAttemptsUsed).IsRequired();
            builder.HasIndex(x => x.DocumentSignId);
            builder.HasIndex(x => x.Status);
            builder.HasOne(x => x.Code)
                .WithOne(x => x.Request)
                .HasForeignKey<SignCode>(x => x.RequestId);
            builder.HasMany(x => x.Attempts)
                .WithOne(x => x.Request)
                .HasForeignKey(x => x.RequestId);
        });

        modelBuilder.Entity<SignCode>(builder =>
        {
            builder.ToTable("SignCodes", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            builder.Property(x => x.CodeSalt).HasMaxLength(128).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).IsRequired();
            builder.Property(x => x.IsUsed).IsRequired();
            builder.HasIndex(x => new { x.RequestId, x.IsUsed });
        });

        modelBuilder.Entity<SignAttempt>(builder =>
        {
            builder.ToTable("SignAttempts", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Details).HasMaxLength(2000);
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.HasIndex(x => x.RequestId);
        });

        modelBuilder.Entity<MessageTemplate>(builder =>
        {
            builder.ToTable("MessageTemplates", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Channel).IsRequired();
            builder.Property(x => x.TemplateCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SubjectTemplate).HasMaxLength(500);
            builder.Property(x => x.BodyTemplate).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc).IsRequired();
            builder.HasIndex(x => x.TemplateCode).IsUnique();
            builder.HasIndex(x => new { x.Channel, x.IsActive });
        });
    }
}
