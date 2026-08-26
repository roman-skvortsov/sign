using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sign.Abstractions.Codes;
using Sign.Abstractions.Messaging;
using Sign.Abstractions.Persistence;
using Sign.Abstractions.Services;
using Sign.Application.Contracts;
using Sign.Application.Services;
using Sign.Configuration;
using Sign.Domain.Entities;
using Sign.Domain.Enums;
using Sign.Infrastructure.Messaging;
using Sign.Infrastructure.Persistence;
using Sign.Infrastructure.Persistence.Repositories;
using Sign.Infrastructure.Security;
using System.Text.RegularExpressions;

namespace Sign.Tests.Infrastructure;

/// <summary>
/// Представляет тестовое окружение для запуска сервиса подписания на базе in-memory БД.
/// </summary>
public sealed class TestSignServiceScope : IAsyncDisposable
{
    private static readonly Regex CodeRegex = new(@"код(?: подтверждения)?\s*:\s*(?<code>\d+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private readonly DbContextOptions<SignDbContext> _dbContextOptions;
    private readonly SqliteConnection _connection;
    private readonly Microsoft.Extensions.Options.IOptions<SignOptions> _optionsAccessor;

    private TestSignServiceScope(
        SqliteConnection connection,
        DbContextOptions<SignDbContext> dbContextOptions,
        CapturingSignChannelSender smsSender,
        CapturingSignChannelSender emailSender,
        SignOptions options)
    {
        _connection = connection;
        _dbContextOptions = dbContextOptions;
        _optionsAccessor = Microsoft.Extensions.Options.Options.Create(options);
        SmsSender = smsSender;
        EmailSender = emailSender;
        Options = options;
    }

    /// <summary>
    /// Получает тестовый отправитель SMS-сообщений.
    /// </summary>
    public CapturingSignChannelSender SmsSender { get; }

    /// <summary>
    /// Получает тестовый отправитель email-сообщений.
    /// </summary>
    public CapturingSignChannelSender EmailSender { get; }

    /// <summary>
    /// Получает набор настроек, с которыми был создан сервис.
    /// </summary>
    public SignOptions Options { get; }

    /// <summary>
    /// Создает новое тестовое окружение и наполняет БД шаблонами сообщений.
    /// </summary>
    /// <param name="configureOptions">Дополнительная настройка параметров библиотеки.</param>
    /// <returns>Готовое тестовое окружение.</returns>
    public static async Task<TestSignServiceScope> CreateAsync(Action<SignOptions>? configureOptions = null)
    {
        var options = new SignOptions
        {
            Schema = "sign",
            CodeLifetime = TimeSpan.FromMinutes(5),
            RetryCount = 3,
            RetryInterval = TimeSpan.FromSeconds(1),
            ResendCooldown = TimeSpan.FromMinutes(1),
            ExtendedResendCooldownAfterAttemptCount = 3,
            ExtendedResendCooldown = TimeSpan.FromMinutes(5),
            MaxVerifyAttempts = 5,
            MaxSendAttempts = 3,
            SmsCodeLength = 4,
            EmailCodeLength = 6,
            HashPepper = "unit-test-pepper",
            SaltSize = 16
        };

        configureOptions?.Invoke(options);

        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var dbContextOptions = new DbContextOptionsBuilder<SignDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new SignDbContext(dbContextOptions, Microsoft.Extensions.Options.Options.Create(options));
        await dbContext.Database.EnsureCreatedAsync();

        await SeedTemplatesAsync(dbContext);

        var smsSender = new CapturingSignChannelSender(SignChannel.Sms);
        var emailSender = new CapturingSignChannelSender(SignChannel.Email);

        return new TestSignServiceScope(connection, dbContextOptions, smsSender, emailSender, options);
    }

    /// <summary>
    /// Возвращает последний код, отправленный по указанному каналу.
    /// </summary>
    /// <param name="channel">Канал, для которого требуется получить код.</param>
    /// <returns>Последний отправленный код.</returns>
    public string GetLastCode(SignChannel channel)
    {
        var message = GetSender(channel).SentMessages.LastOrDefault()
            ?? throw new InvalidOperationException($"По каналу '{channel}' еще не отправлялись сообщения.");

        var match = CodeRegex.Match(message.Body);

        if (!match.Success)
        {
            throw new InvalidOperationException("В последнем сообщении не найден код подтверждения.");
        }

        return match.Groups["code"].Value;
    }

    /// <summary>
    /// Возвращает последнее сообщение, отправленное по указанному каналу.
    /// </summary>
    /// <param name="channel">Канал, по которому было отправлено сообщение.</param>
    /// <returns>Последнее отправленное сообщение.</returns>
    public SignMessage GetLastMessage(SignChannel channel)
    {
        return GetSender(channel).SentMessages.LastOrDefault()
            ?? throw new InvalidOperationException($"По каналу '{channel}' еще не отправлялись сообщения.");
    }

    /// <summary>
    /// Освобождает ресурсы тестового окружения.
    /// </summary>
    /// <returns>Задача освобождения ресурсов.</returns>
    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    /// <summary>
    /// Запускает процесс подписания в отдельном тестовом scope.
    /// </summary>
    /// <param name="request">Параметры запуска подписания.</param>
    /// <returns>Результат запуска процесса подписания.</returns>
    public async Task<StartSigningResult> StartSigningAsync(StartSigningRequest request)
    {
        await using var session = CreateSession();
        return await session.SignService.StartSigningAsync(request);
    }

    /// <summary>
    /// Выполняет повторную отправку кода в отдельном тестовом scope.
    /// </summary>
    /// <param name="request">Параметры повторной отправки кода.</param>
    /// <returns>Результат повторной отправки.</returns>
    public async Task<ResendCodeResult> ResendCodeAsync(ResendCodeRequest request)
    {
        await using var session = CreateSession();
        return await session.SignService.ResendCodeAsync(request);
    }

    /// <summary>
    /// Проверяет код подтверждения в отдельном тестовом scope.
    /// </summary>
    /// <param name="request">Параметры проверки кода.</param>
    /// <returns>Результат проверки кода.</returns>
    public async Task<VerifyCodeResult> VerifyCodeAsync(VerifyCodeRequest request)
    {
        await using var session = CreateSession();
        return await session.SignService.VerifyCodeAsync(request);
    }

    /// <summary>
    /// Возвращает запрос на подписание вместе с кодом и журналом попыток.
    /// </summary>
    /// <param name="requestId">Идентификатор запроса на подписание.</param>
    /// <returns>Запрос на подписание из базы данных.</returns>
    public async Task<SignRequest> GetRequestAsync(Guid requestId)
    {
        await using var dbContext = CreateDbContext();

        return await dbContext.SignRequests
            .Include(x => x.SignCode)
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == requestId);
    }

    /// <summary>
    /// Возвращает количество запросов на подписание в тестовой базе данных.
    /// </summary>
    /// <returns>Количество запросов на подписание.</returns>
    public async Task<int> CountRequestsAsync()
    {
        await using var dbContext = CreateDbContext();
        return await dbContext.SignRequests.CountAsync();
    }

    /// <summary>
    /// Смещает время предыдущих попыток отправки на указанный интервал в прошлое.
    /// </summary>
    /// <param name="requestId">Идентификатор запроса на подписание.</param>
    /// <param name="offset">Интервал смещения в прошлое.</param>
    /// <returns>Асинхронная задача сохранения изменений.</returns>
    public async Task MoveSendAttemptsToPastAsync(Guid requestId, TimeSpan offset)
    {
        await using var dbContext = CreateDbContext();

        var attempts = await dbContext.SignAttempts
            .Where(x => x.SignRequestId == requestId && (x.Type == SignAttemptType.Sent || x.Type == SignAttemptType.Resent || x.Type == SignAttemptType.SendFailed))
            .ToListAsync();

        foreach (var attempt in attempts)
        {
            attempt.CreatedAtUtc = attempt.CreatedAtUtc.Subtract(offset);
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Возвращает отправитель сообщений по заданному каналу.
    /// </summary>
    /// <param name="channel">Канал отправки.</param>
    /// <returns>Тестовый отправитель выбранного канала.</returns>
    private CapturingSignChannelSender GetSender(SignChannel channel)
    {
        return channel == SignChannel.Sms ? SmsSender : EmailSender;
    }

    /// <summary>
    /// Создает новый экземпляр контекста базы данных поверх общей in-memory базы теста.
    /// </summary>
    /// <returns>Новый экземпляр контекста.</returns>
    private SignDbContext CreateDbContext()
    {
        return new SignDbContext(_dbContextOptions, _optionsAccessor);
    }

    /// <summary>
    /// Создает новый тестовый session для выполнения одной бизнес-операции.
    /// </summary>
    /// <returns>Тестовый session с отдельным контекстом базы данных.</returns>
    private TestSignServiceSession CreateSession()
    {
        var dbContext = CreateDbContext();
        ISignRequestRepository signRequestRepository = new SignRequestRepository(dbContext);
        IMessageTemplateRepository messageTemplateRepository = new MessageTemplateRepository(dbContext);
        ITemplateValueProvider templateValueProvider = new ReflectionTemplateValueProvider();
        IMessageTemplateRenderer messageTemplateRenderer = new DefaultMessageTemplateRenderer(messageTemplateRepository, templateValueProvider);
        IVerificationCodeProtector verificationCodeProtector = new HmacVerificationCodeProtector(_optionsAccessor);
        ICodeGenerator codeGenerator = new NumericCodeGenerator(verificationCodeProtector, _optionsAccessor);

        ISignService signService = new SignService(
            dbContext,
            signRequestRepository,
            codeGenerator,
            verificationCodeProtector,
            messageTemplateRenderer,
            new ISignChannelSender[] { SmsSender, EmailSender },
            _optionsAccessor);

        return new TestSignServiceSession(dbContext, signService);
    }

    /// <summary>
    /// Наполняет in-memory БД обязательными шаблонами сообщений для тестов.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных теста.</param>
    /// <returns>Асинхронная задача заполнения данных.</returns>
    private static async Task SeedTemplatesAsync(SignDbContext dbContext)
    {
        var utcNow = DateTimeOffset.UtcNow;

        dbContext.MessageTemplates.AddRange(
            new MessageTemplate
            {
                Id = Guid.NewGuid(),
                Channel = SignChannel.Sms,
                TemplateCode = "sms-sign-code",
                SubjectTemplate = null,
                BodyTemplate = "Документ %%DocumentSignId%%. Код подтверждения: %%SignCode%%.",
                IsActive = true,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            },
            new MessageTemplate
            {
                Id = Guid.NewGuid(),
                Channel = SignChannel.Email,
                TemplateCode = "email-sign-code",
                SubjectTemplate = "Подписание документа %%DocumentSignId%%",
                BodyTemplate = "Ваш код подтверждения: %%SignCode%%. Идентификатор запроса: %%RequestId%%.",
                IsActive = true,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            });

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Представляет короткоживущий session для одной операции сервиса подписания.
    /// </summary>
    /// <param name="DbContext">Контекст базы данных операции.</param>
    /// <param name="SignService">Сервис подписания для операции.</param>
    private sealed record TestSignServiceSession(SignDbContext DbContext, ISignService SignService) : IAsyncDisposable
    {
        /// <summary>
        /// Освобождает ресурсы session после завершения операции.
        /// </summary>
        /// <returns>Задача освобождения ресурсов.</returns>
        public ValueTask DisposeAsync()
        {
            return DbContext.DisposeAsync();
        }
    }
}
