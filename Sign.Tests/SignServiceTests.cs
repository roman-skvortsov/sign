using Sign.Application.Contracts;
using Sign.Domain.Enums;
using Sign.Tests.Infrastructure;

namespace Sign.Tests;

/// <summary>
/// Содержит интеграционные unit-тесты сервиса подписания на in-memory БД.
/// </summary>
public sealed class SignServiceTests
{
    /// <summary>
    /// Проверяет, что при запуске подписания для SMS создается запрос, сохраняется хеш кода и отправляется сообщение по шаблону.
    /// </summary>
    [Fact]
    public async Task StartSigningAsync_ShouldCreateRequestAndSendSmsMessage()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var result = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000001"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(SignRequestStatus.CodeSent, result.Status);

        var lastMessage = scope.GetLastMessage(SignChannel.Sms);
        var sentCode = scope.GetLastCode(SignChannel.Sms);
        var request = await scope.GetRequestAsync(result.RequestId);

        Assert.Equal("+79990000001", lastMessage.Recipient);
        Assert.Contains("11111111-1111-1111-1111-111111111111", lastMessage.Body);
        Assert.Equal(4, sentCode.Length);
        Assert.NotEqual(sentCode, request.SignCode!.CodeHash);
        Assert.False(request.SignCode.IsUsed);
        Assert.Equal(2, request.SignAttempts.Count);
        Assert.Contains(request.SignAttempts, x => x.Type == SignAttemptType.Created);
        Assert.Contains(request.SignAttempts, x => x.Type == SignAttemptType.Sent);
    }

    /// <summary>
    /// Проверяет, что для email используется шестизначный код и тема письма формируется из шаблона.
    /// </summary>
    [Fact]
    public async Task StartSigningAsync_ShouldSendEmailMessageWithSubjectAndSixDigitCode()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var result = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Channel = SignChannel.Email,
            Recipient = "user@example.com"
        });

        var lastMessage = scope.GetLastMessage(SignChannel.Email);
        var sentCode = scope.GetLastCode(SignChannel.Email);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, sentCode.Length);
        Assert.Equal("user@example.com", lastMessage.Recipient);
        Assert.Contains("22222222-2222-2222-2222-222222222222", lastMessage.Subject);
        Assert.Contains(result.RequestId.ToString(), lastMessage.Body);
    }

    /// <summary>
    /// Проверяет, что повторный запуск подписания для активного запроса возвращает бизнес-ошибку и не создает вторую запись.
    /// </summary>
    [Fact]
    public async Task StartSigningAsync_ShouldReturnError_WhenActiveRequestAlreadyExists()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000002"
        });

        var result = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000002"
        });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Single(scope.SmsSender.SentMessages);
        Assert.Equal(1, await scope.CountRequestsAsync());
    }

    /// <summary>
    /// Проверяет, что повторная отправка кода запрещена до истечения минимального интервала ожидания.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_ShouldReturnCooldownError_WhenCooldownHasNotElapsed()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000003"
        });

        var resendResult = await scope.ResendCodeAsync(new ResendCodeRequest
        {
            RequestId = startResult.RequestId
        });

        Assert.False(resendResult.IsSuccess);
        Assert.NotNull(resendResult.ErrorMessage);
        Assert.NotNull(resendResult.NextAvailableAtUtc);
        Assert.Single(scope.SmsSender.SentMessages);
    }

    /// <summary>
    /// Проверяет, что после истечения интервала ожидания код можно отправить повторно, а в журнал добавляется событие повторной отправки.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_ShouldSendCodeAgain_WhenCooldownElapsed()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Channel = SignChannel.Email,
            Recipient = "resend@example.com"
        });

        var requestBeforeResend = await scope.GetRequestAsync(startResult.RequestId);

        var oldSalt = requestBeforeResend.SignCode!.CodeSalt;

        await scope.MoveSendAttemptsToPastAsync(startResult.RequestId, scope.Options.ResendCooldown.Add(TimeSpan.FromSeconds(5)));

        var resendResult = await scope.ResendCodeAsync(new ResendCodeRequest
        {
            RequestId = startResult.RequestId
        });

        var requestAfterResend = await scope.GetRequestAsync(startResult.RequestId);

        Assert.True(resendResult.IsSuccess);
        Assert.Equal(2, scope.EmailSender.SentMessages.Count);
        Assert.Equal(2, requestAfterResend.SendAttemptsUsed);
        Assert.NotEqual(oldSalt, requestAfterResend.SignCode!.CodeSalt);
        Assert.Contains(requestAfterResend.SignAttempts, x => x.Type == SignAttemptType.Resent);
    }

    /// <summary>
    /// Проверяет, что повторная отправка кода запрещается после достижения максимального количества отправок.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_ShouldReturnError_WhenMaxSendAttemptsReached()
    {
        await using var scope = await TestSignServiceScope.CreateAsync(options =>
        {
            options.MaxSendAttempts = 1;
        });

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000004"
        });

        await scope.MoveSendAttemptsToPastAsync(startResult.RequestId, TimeSpan.FromMinutes(2));

        var resendResult = await scope.ResendCodeAsync(new ResendCodeRequest
        {
            RequestId = startResult.RequestId
        });

        Assert.False(resendResult.IsSuccess);
        Assert.Equal("Достигнуто максимальное количество отправок кода подтверждения.", resendResult.ErrorMessage);
        Assert.Single(scope.SmsSender.SentMessages);
    }

    /// <summary>
    /// Проверяет, что корректный код успешно подтверждает запрос, переводит его в состояние подписанного и помечает код использованным.
    /// </summary>
    [Fact]
    public async Task VerifyCodeAsync_ShouldMarkRequestAsSigned_WhenCodeIsValid()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000005"
        });

        var code = scope.GetLastCode(SignChannel.Sms);

        var verificationResult = await scope.VerifyCodeAsync(new VerifyCodeRequest
        {
            RequestId = startResult.RequestId,
            Code = code
        });

        var request = await scope.GetRequestAsync(startResult.RequestId);

        Assert.True(verificationResult.IsSuccess);
        Assert.Equal(SignRequestStatus.Signed, verificationResult.Status);
        Assert.Equal(SignRequestStatus.Signed, request.Status);
        Assert.True(request.SignCode!.IsUsed);
        Assert.NotNull(request.SignedAtUtc);
        Assert.Contains(request.SignAttempts, x => x.Type == SignAttemptType.VerifySucceeded);
    }

    /// <summary>
    /// Проверяет, что неверные коды уменьшают остаток попыток, а после достижения лимита запрос блокируется.
    /// </summary>
    [Fact]
    public async Task VerifyCodeAsync_ShouldBlockRequest_WhenInvalidCodeLimitReached()
    {
        await using var scope = await TestSignServiceScope.CreateAsync(options =>
        {
            options.MaxVerifyAttempts = 2;
        });

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Channel = SignChannel.Email,
            Recipient = "blocked@example.com"
        });

        var firstAttempt = await scope.VerifyCodeAsync(new VerifyCodeRequest
        {
            RequestId = startResult.RequestId,
            Code = "000000"
        });

        var secondAttempt = await scope.VerifyCodeAsync(new VerifyCodeRequest
        {
            RequestId = startResult.RequestId,
            Code = "111111"
        });

        var request = await scope.GetRequestAsync(startResult.RequestId);

        Assert.False(firstAttempt.IsSuccess);
        Assert.Equal(1, firstAttempt.RemainingAttempts);
        Assert.False(secondAttempt.IsSuccess);
        Assert.Equal(SignRequestStatus.Blocked, secondAttempt.Status);
        Assert.Equal(0, secondAttempt.RemainingAttempts);
        Assert.Equal(SignRequestStatus.Blocked, request.Status);
        Assert.Equal(2, request.VerifyAttemptsUsed);
        Assert.Equal(2, request.SignAttempts.Count(x => x.Type == SignAttemptType.VerifyFailed));
        Assert.Contains(request.SignAttempts, x => x.Type == SignAttemptType.Blocked);
    }

    /// <summary>
    /// Проверяет, что повторная проверка уже подписанного запроса запрещена бизнес-правилом.
    /// </summary>
    [Fact]
    public async Task VerifyCodeAsync_ShouldThrow_WhenRequestIsAlreadySigned()
    {
        await using var scope = await TestSignServiceScope.CreateAsync();

        var startResult = await scope.StartSigningAsync(new StartSigningRequest
        {
            DocumentSignId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Channel = SignChannel.Sms,
            Recipient = "+79990000006"
        });

        var code = scope.GetLastCode(SignChannel.Sms);

        await scope.VerifyCodeAsync(new VerifyCodeRequest
        {
            RequestId = startResult.RequestId,
            Code = code
        });

        var action = async () => await scope.VerifyCodeAsync(new VerifyCodeRequest
        {
            RequestId = startResult.RequestId,
            Code = code
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal("Запрос уже завершен и не может быть повторно подтвержден.", exception.Message);
    }
}
