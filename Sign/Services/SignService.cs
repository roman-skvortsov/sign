using Microsoft.Extensions.Options;
using Sign.Codes;
using Sign.Contracts;
using Sign.Data;
using Sign.Data.Repositories;
using Sign.Entities;
using Sign.Enums;
using Sign.Messaging;
using Sign.Options;

namespace Sign.Services;

/// <summary>
/// Представляет основную реализацию сервиса управления процессом подписания документов.
/// </summary>
public sealed class SignService : ISignService
{
    private readonly SignDbContext _dbContext;
    private readonly ISignRequestRepository _signRequestRepository;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IVerificationCodeProtector _verificationCodeProtector;
    private readonly IMessageTemplateRenderer _messageTemplateRenderer;
    private readonly IReadOnlyDictionary<SignChannel, ISignChannelSender> _senders;
    private readonly SignOptions _options;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SignService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных библиотеки подписания.</param>
    /// <param name="signRequestRepository">Репозиторий запросов на подписание.</param>
    /// <param name="codeGenerator">Сервис генерации кода подтверждения.</param>
    /// <param name="verificationCodeProtector">Сервис защиты и проверки кода подтверждения.</param>
    /// <param name="messageTemplateRenderer">Сервис построения текста сообщения по шаблону.</param>
    /// <param name="senders">Набор отправителей по каналам подтверждения.</param>
    /// <param name="options">Настройки библиотеки подписания.</param>
    public SignService(
        SignDbContext dbContext,
        ISignRequestRepository signRequestRepository,
        ICodeGenerator codeGenerator,
        IVerificationCodeProtector verificationCodeProtector,
        IMessageTemplateRenderer messageTemplateRenderer,
        IEnumerable<ISignChannelSender> senders,
        IOptions<SignOptions> options)
    {
        _dbContext = dbContext;
        _signRequestRepository = signRequestRepository;
        _codeGenerator = codeGenerator;
        _verificationCodeProtector = verificationCodeProtector;
        _messageTemplateRenderer = messageTemplateRenderer;
        _senders = senders.ToDictionary(x => x.Channel);
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<SigningResult> StartSigningAsync(StartSigningRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DocumentSignId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Recipient);

        var utcNow = DateTimeOffset.UtcNow;
        var generatedCode = _codeGenerator.Generate(new CodeGenerationContext
        {
            Channel = request.Channel
        });

        var signRequest = new SignRequest
        {
            Id = Guid.NewGuid(),
            DocumentSignId = request.DocumentSignId,
            Channel = request.Channel,
            Recipient = request.Recipient,
            Status = SignRequestStatus.Pending,
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow.Add(_options.CodeLifetime),
            VerifyAttemptsUsed = 0,
            SendAttemptsUsed = 1,
            Code = new SignCode
            {
                Id = Guid.NewGuid(),
                CodeHash = generatedCode.Hash,
                CodeSalt = generatedCode.Salt,
                CreatedAtUtc = utcNow,
                ExpiresAtUtc = utcNow.Add(_options.CodeLifetime),
                IsUsed = false
            }
        };

        signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.Created, "Запрос на подписание создан.", utcNow));

        _dbContext.Requests.Add(signRequest);

        var message = await _messageTemplateRenderer.RenderAsync(new MessageTemplateContext
        {
            DocumentSignId = signRequest.DocumentSignId,
            RequestId = signRequest.Id,
            Channel = signRequest.Channel,
            Recipient = signRequest.Recipient,
            Code = generatedCode.Value,
            ExpiresAtUtc = signRequest.ExpiresAtUtc
        }, cancellationToken);

        try
        {
            var sender = ResolveSender(signRequest.Channel);
            await sender.SendAsync(message, cancellationToken);

            signRequest.Status = SignRequestStatus.CodeSent;
            signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.Sent, "Код подтверждения успешно отправлен.", utcNow));
        }
        catch (Exception exception)
        {
            signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.SendFailed, exception.Message, utcNow));
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SigningResult
        {
            RequestId = signRequest.Id,
            DocumentSignId = signRequest.DocumentSignId,
            Status = signRequest.Status,
            ExpiresAtUtc = signRequest.ExpiresAtUtc
        };
    }

    /// <inheritdoc />
    public async Task<VerificationResult> VerifyCodeAsync(VerifySigningCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);

        var signRequest = await _signRequestRepository.GetByIdWithCodeAndAttemptsAsync(request.RequestId, cancellationToken);

        if (signRequest is null)
        {
            throw new InvalidOperationException("Запрос на подписание не найден.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        EnsureRequestCanBeVerified(signRequest, utcNow);

        signRequest.VerifyAttemptsUsed++;

        var isValid = _verificationCodeProtector.Verify(
            request.Code,
            signRequest.Code!.CodeHash,
            signRequest.Code.CodeSalt);

        if (isValid)
        {
            signRequest.Status = SignRequestStatus.Signed;
            signRequest.SignedAtUtc = utcNow;
            signRequest.Code.IsUsed = true;
            signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.VerifySucceeded, "Код подтверждения успешно проверен.", utcNow));
        }
        else
        {
            signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.VerifyFailed, "Введен неверный код подтверждения.", utcNow));

            if (signRequest.VerifyAttemptsUsed >= _options.MaxVerifyAttempts)
            {
                signRequest.Status = SignRequestStatus.Blocked;
                signRequest.Attempts.Add(CreateAttempt(signRequest.Id, SignAttemptType.Blocked, "Запрос заблокирован по лимиту попыток проверки.", utcNow));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerificationResult
        {
            IsSuccess = isValid,
            Status = signRequest.Status,
            RemainingAttempts = Math.Max(_options.MaxVerifyAttempts - signRequest.VerifyAttemptsUsed, 0)
        };
    }

    /// <summary>
    /// Возвращает отправителя для указанного канала подтверждения.
    /// </summary>
    /// <param name="channel">Канал подтверждения.</param>
    /// <returns>Экземпляр отправителя по выбранному каналу.</returns>
    private ISignChannelSender ResolveSender(SignChannel channel)
    {
        if (_senders.TryGetValue(channel, out var sender))
        {
            return sender;
        }

        throw new InvalidOperationException($"Для канала '{channel}' не зарегистрирован отправитель.");
    }

    /// <summary>
    /// Проверяет, что запрос на подписание доступен для валидации кода.
    /// </summary>
    /// <param name="signRequest">Проверяемый запрос на подписание.</param>
    /// <param name="utcNow">Текущее время в формате UTC.</param>
    private static void EnsureRequestCanBeVerified(SignRequest signRequest, DateTimeOffset utcNow)
    {
        if (signRequest.Code is null)
        {
            throw new InvalidOperationException("Для запроса отсутствует активный код подтверждения.");
        }

        if (signRequest.Status is SignRequestStatus.Signed or SignRequestStatus.Cancelled)
        {
            throw new InvalidOperationException("Запрос уже завершен и не может быть повторно подтвержден.");
        }

        if (signRequest.Status == SignRequestStatus.Blocked)
        {
            throw new InvalidOperationException("Запрос заблокирован по лимиту попыток.");
        }

        if (signRequest.Code.IsUsed)
        {
            throw new InvalidOperationException("Код подтверждения уже использован.");
        }

        if (signRequest.ExpiresAtUtc <= utcNow || signRequest.Code.ExpiresAtUtc <= utcNow)
        {
            signRequest.Status = SignRequestStatus.Expired;
            throw new InvalidOperationException("Срок действия кода подтверждения истек.");
        }
    }

    /// <summary>
    /// Создает запись аудита по процессу подписания.
    /// </summary>
    /// <param name="requestId">Идентификатор запроса на подписание.</param>
    /// <param name="type">Тип события или попытки.</param>
    /// <param name="details">Дополнительные сведения о событии.</param>
    /// <param name="utcNow">Текущее время в формате UTC.</param>
    /// <returns>Новая запись аудита.</returns>
    private static SignAttempt CreateAttempt(Guid requestId, SignAttemptType type, string? details, DateTimeOffset utcNow)
    {
        return new SignAttempt
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Type = type,
            Details = details,
            CreatedAtUtc = utcNow
        };
    }
}
