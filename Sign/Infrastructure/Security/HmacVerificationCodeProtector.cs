using Microsoft.Extensions.Options;
using Sign.Abstractions.Codes;
using Sign.Application.Codes;
using Sign.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Sign.Infrastructure.Security;

/// <summary>
/// Представляет реализацию защиты кода подтверждения на основе HMACSHA256, соли и секретного значения.
/// </summary>
public sealed class HmacVerificationCodeProtector : IVerificationCodeProtector
{
    private readonly SignOptions _options;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="HmacVerificationCodeProtector"/>.
    /// </summary>
    /// <param name="options">Настройки библиотеки подписания.</param>
    public HmacVerificationCodeProtector(IOptions<SignOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public VerificationCodeProtectionResult Protect(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var salt = GenerateSalt();
        var hash = ComputeHash(code, salt);

        return new VerificationCodeProtectionResult
        {
            Hash = hash,
            Salt = salt
        };
    }

    /// <inheritdoc />
    public bool Verify(string code, string hash, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        var computedHash = ComputeHash(code, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHash),
            Convert.FromHexString(hash));
    }

    /// <summary>
    /// Вычисляет хеш кода подтверждения с использованием соли и секретного значения.
    /// </summary>
    /// <param name="code">Исходный код подтверждения.</param>
    /// <param name="salt">Соль кода подтверждения.</param>
    /// <returns>Хеш кода подтверждения.</returns>
    private string ComputeHash(string code, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.HashPepper);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(_options.SaltSize, 0);

        var key = Encoding.UTF8.GetBytes(_options.HashPepper);
        var payload = Encoding.UTF8.GetBytes($"{salt}:{code}");

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(payload);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Генерирует криптографически стойкую соль для кода подтверждения.
    /// </summary>
    /// <returns>Соль в шестнадцатеричном представлении.</returns>
    private string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(_options.SaltSize);
        return Convert.ToHexString(saltBytes);
    }
}
