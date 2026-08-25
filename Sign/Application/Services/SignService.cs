using Microsoft.Extensions.Options;
using Sign.Abstractions.Codes;
using Sign.Abstractions.Messaging;
using Sign.Abstractions.Services;
using Sign.Application.Codes;
using Sign.Application.Contracts;
using Sign.Configuration;
using Sign.Domain.Entities;
using Sign.Domain.Enums;
using Sign.Infrastructure.Messaging;
using Sign.Infrastructure.Persistence;
using Sign.Abstractions.Persistence;

namespace Sign.Application.Services;

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
        var activeRequest = await _signRequestRepository.GetActiveByDocumentSignIdAsync(
            request.DocumentSignId,
            request.Channel,
            request.Recipient,
            cancellationToken);

        if (activeRequest is not null)
        {
            return new SigningResult
            {
                IsSuccess = false,
                RequestId = activeRequest.Id,
                DocumentSignId = activeRequest.DocumentSignId,
                Status = activeRequest.Status,
                ExpiresAtUtc = activeRequest.ExpiresAtUtc,
                ErrorMessage = "Для документа уже существует активный запрос на подписание. Для повторной отправки используйте существующий RequestId."
            };
        }

        var generatedCode = _codeGenerator.Generate(new CodeGenerationData
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
            SendAttemptsUsed = 0,
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
        return await SendCodeAsync(signRequest, generatedCode.Value, isResend: false, utcNow, cancellationToken);
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

    /// <inheritdoc />
    public async Task<SigningResult> ResendCodeAsync(ResendSigningCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var signRequest = await _signRequestRepository.GetByIdWithCodeAndAttemptsAsync(request.RequestId, cancellationToken);

        if (signRequest is null)
        {
            return new SigningResult
            {
                IsSuccess = false,
                ErrorMessage = "Запрос на подписание не найден."
            };
        }

        var utcNow = DateTimeOffset.UtcNow;
        var sendAvailability = GetSendAvailability(signRequest, utcNow);

        if (!sendAvailability.CanSend)
        {
            return new SigningResult
            {
                IsSuccess = false,
                RequestId = signRequest.Id,
                DocumentSignId = signRequest.DocumentSignId,
                Status = signRequest.Status,
                ExpiresAtUtc = signRequest.ExpiresAtUtc,
                ErrorMessage = sendAvailability.ErrorMessage,
                NextAvailableAtUtc = sendAvailability.NextAvailableAtUtc
            };
        }

        var generatedCode = _codeGenerator.Generate(new CodeGenerationData
        {
            Channel = signRequest.Channel
        });

        signRequest.ExpiresAtUtc = utcNow.Add(_options.CodeLifetime);
        signRequest.Code!.CodeHash = generatedCode.Hash;
        signRequest.Code.CodeSalt = generatedCode.Salt;
        signRequest.Code.CreatedAtUtc = utcNow;
        signRequest.Code.ExpiresAtUtc = signRequest.ExpiresAtUtc;
        signRequest.Code.IsUsed = false;

        return await SendCodeAsync(signRequest, generatedCode.Value, isResend: true, utcNow, cancellationToken);
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
    /// Отправляет код подтверждения по указанному запросу на подписание.
    /// </summary>
    /// <param name="signRequest">Запрос на подписание.</param>
    /// <param name="plainCode">Исходный код подтверждения.</param>
    /// <param name="isResend">Признак повторной отправки.</param>
    /// <param name="utcNow">Текущее время в формате UTC.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки кода.</returns>
    private async Task<SigningResult> SendCodeAsync(
        SignRequest signRequest,
        string plainCode,
        bool isResend,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        signRequest.SendAttemptsUsed++;

        var message = await _messageTemplateRenderer.RenderAsync(new MessageTemplateData
        {
            DocumentSignId = signRequest.DocumentSignId,
            RequestId = signRequest.Id,
            Channel = signRequest.Channel,
            Recipient = signRequest.Recipient,
            Code = plainCode,
            ExpiresAtUtc = signRequest.ExpiresAtUtc
        }, cancellationToken);

        try
        {
            var sender = ResolveSender(signRequest.Channel);
            await sender.SendAsync(message, cancellationToken);

            signRequest.Status = SignRequestStatus.CodeSent;
            signRequest.Attempts.Add(CreateAttempt(
                signRequest.Id,
                isResend ? SignAttemptType.Resent : SignAttemptType.Sent,
                isResend ? "Код подтверждения успешно отправлен повторно." : "Код подтверждения успешно отправлен.",
                utcNow));
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
            IsSuccess = true,
            RequestId = signRequest.Id,
            DocumentSignId = signRequest.DocumentSignId,
            Status = signRequest.Status,
            ExpiresAtUtc = signRequest.ExpiresAtUtc
        };
    }

    /// <summary>
    /// Проверяет, доступна ли отправка кода для указанного запроса на подписание.
    /// </summary>
    /// <param name="signRequest">Проверяемый запрос на подписание.</param>
    /// <param name="utcNow">Текущее время в формате UTC.</param>
    /// <returns>Результат проверки доступности отправки кода.</returns>
    private SendAvailabilityResult GetSendAvailability(SignRequest signRequest, DateTimeOffset utcNow)
    {
        if (signRequest.Status is SignRequestStatus.Signed or SignRequestStatus.Cancelled)
        {
            return new SendAvailabilityResult
            {
                CanSend = false,
                ErrorMessage = "Запрос уже завершен, повторная отправка кода недоступна."
            };
        }

        if (signRequest.Status == SignRequestStatus.Blocked)
        {
            return new SendAvailabilityResult
            {
                CanSend = false,
                ErrorMessage = "Запрос заблокирован, повторная отправка кода недоступна."
            };
        }

        if (signRequest.SendAttemptsUsed >= _options.MaxSendAttempts)
        {
            return new SendAvailabilityResult
            {
                CanSend = false,
                ErrorMessage = "Достигнуто максимальное количество отправок кода подтверждения."
            };
        }

        var lastSendAttemptAtUtc = GetLastSendAttemptAtUtc(signRequest);

        if (lastSendAttemptAtUtc is null)
        {
            return new SendAvailabilityResult
            {
                CanSend = true
            };
        }

        var resendCooldown = GetResendCooldown(signRequest.SendAttemptsUsed);
        var nextSendAvailableAtUtc = lastSendAttemptAtUtc.Value.Add(resendCooldown);

        if (utcNow < nextSendAvailableAtUtc)
        {
            var remaining = nextSendAvailableAtUtc - utcNow;
            return new SendAvailabilityResult
            {
                CanSend = false,
                ErrorMessage = $"Повторная отправка кода будет доступна через {FormatRemainingTime(remaining)}.",
                NextAvailableAtUtc = nextSendAvailableAtUtc
            };
        }

        return new SendAvailabilityResult
        {
            CanSend = true,
            NextAvailableAtUtc = utcNow
        };
    }

    /// <summary>
    /// Возвращает интервал повторной отправки в зависимости от количества уже выполненных отправок.
    /// </summary>
    /// <param name="sendAttemptsUsed">Количество уже выполненных отправок.</param>
    /// <returns>Интервал, который должен пройти до следующей отправки.</returns>
    private TimeSpan GetResendCooldown(int sendAttemptsUsed)
    {
        if (_options.ExtendedResendCooldownAfterAttemptCount > 0
            && sendAttemptsUsed >= _options.ExtendedResendCooldownAfterAttemptCount)
        {
            return _options.ExtendedResendCooldown;
        }

        return _options.ResendCooldown;
    }

    /// <summary>
    /// Возвращает дату и время последней попытки отправки кода.
    /// </summary>
    /// <param name="signRequest">Запрос на подписание.</param>
    /// <returns>Дата и время последней попытки отправки кода или <see langword="null"/>.</returns>
    private static DateTimeOffset? GetLastSendAttemptAtUtc(SignRequest signRequest)
    {
        return signRequest.Attempts
            .Where(x => x.Type is SignAttemptType.Sent or SignAttemptType.Resent or SignAttemptType.SendFailed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => (DateTimeOffset?)x.CreatedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Форматирует интервал ожидания до следующей повторной отправки кода.
    /// </summary>
    /// <param name="remaining">Оставшийся интервал ожидания.</param>
    /// <returns>Человекочитаемое представление интервала ожидания.</returns>
    private static string FormatRemainingTime(TimeSpan remaining)
    {
        var totalSeconds = Math.Max((int)Math.Ceiling(remaining.TotalSeconds), 1);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        if (minutes > 0 && seconds > 0)
        {
            return $"{minutes} мин. {seconds} сек.";
        }

        if (minutes > 0)
        {
            return $"{minutes} мин.";
        }

        return $"{seconds} сек.";
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
