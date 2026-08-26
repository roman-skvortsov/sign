using Microsoft.Extensions.Options;
using SFN.Sign.Abstractions.Codes;
using SFN.Sign.Application.Codes;
using SFN.Sign.Domain.Enums;
using SFN.Sign.Configuration;
using System.Security.Cryptography;

namespace SFN.Sign.Infrastructure.Security;

/// <summary>
/// Представляет реализацию генерации цифровых кодов подтверждения для разных каналов.
/// </summary>
public sealed class NumericCodeGenerator : ICodeGenerator
{
    private readonly IVerificationCodeProtector _verificationCodeProtector;
    private readonly SignOptions _options;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="NumericCodeGenerator"/>.
    /// </summary>
    /// <param name="verificationCodeProtector">Сервис защиты кода подтверждения.</param>
    /// <param name="options">Настройки библиотеки подписания.</param>
    public NumericCodeGenerator(IVerificationCodeProtector verificationCodeProtector, IOptions<SignOptions> options)
    {
        _verificationCodeProtector = verificationCodeProtector;
        _options = options.Value;
    }

    /// <inheritdoc />
    public GeneratedCode Generate(CodeGenerationData context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var length = context.Channel == SignChannel.Sms
            ? _options.SmsCodeLength
            : _options.EmailCodeLength;

        var value = GenerateNumericCode(length);
        var protectionResult = _verificationCodeProtector.Protect(value);

        return new GeneratedCode
        {
            Value = value,
            Hash = protectionResult.Hash,
            Salt = protectionResult.Salt
        };
    }

    /// <summary>
    /// Генерирует цифровой код фиксированной длины.
    /// </summary>
    /// <param name="length">Длина кода.</param>
    /// <returns>Строковое представление сгенерированного кода.</returns>
    private static string GenerateNumericCode(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(length, 0);

        Span<char> chars = stackalloc char[length];

        for (var index = 0; index < chars.Length; index++)
        {
            chars[index] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(chars);
    }
}
