using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sign.Domain.Entities;
using Sign.Configuration;

namespace Sign.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных библиотеки подписания.
/// </summary>
public sealed class SignDbContext : DbContext
{
    private readonly string _schema;

    /// <summary>
    /// Создает контекст базы данных.
    /// </summary>
    /// <param name="options">Настройки контекста.</param>
    /// <param name="signOptions">Настройки библиотеки.</param>
    public SignDbContext(DbContextOptions<SignDbContext> options, IOptions<SignOptions> signOptions)
        : base(options)
    {
        _schema = signOptions.Value.Schema;
    }

    /// <summary>
    /// Запросы на подписание.
    /// </summary>
    public DbSet<SignRequest> SignRequests => Set<SignRequest>();

    /// <summary>
    /// Коды подтверждения.
    /// </summary>
    public DbSet<SignCode> SignCodes => Set<SignCode>();

    /// <summary>
    /// Записи о действиях и попытках.
    /// </summary>
    public DbSet<SignAttempt> SignAttempts => Set<SignAttempt>();

    /// <summary>
    /// Шаблоны сообщений.
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
            builder.Property(x => x.DocumentSignId).IsRequired();
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
            builder.HasOne(x => x.SignCode)
                .WithOne(x => x.SignRequest)
                .HasForeignKey<SignCode>(x => x.SignRequestId);
            builder.HasMany(x => x.SignAttempts)
                .WithOne(x => x.SignRequest)
                .HasForeignKey(x => x.SignRequestId);
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
            builder.HasIndex(x => new { x.SignRequestId, x.IsUsed });
        });

        modelBuilder.Entity<SignAttempt>(builder =>
        {
            builder.ToTable("SignAttempts", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Details).HasMaxLength(2000);
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.HasIndex(x => x.SignRequestId);
        });

        modelBuilder.Entity<MessageTemplate>(builder =>
        {
            builder.ToTable("MessageTemplates", _schema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Channel).IsRequired();
            builder.Property(x => x.TemplateType).IsRequired();
            builder.Property(x => x.SubjectTemplate).HasMaxLength(500);
            builder.Property(x => x.BodyTemplate).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc).IsRequired();
            builder.HasIndex(x => x.TemplateType);
            builder.HasIndex(x => new { x.Channel, x.IsActive });
        });
    }
}
