using System.Text;
using SFN.Sign.Configuration;
using SFN.Sign.Infrastructure.Persistence;

namespace SFN.Sign.Tests;

/// <summary>
/// Содержит unit-тесты подготовки строки подключения.
/// </summary>
public sealed class SignConnectionStringResolverTests
{
    /// <summary>
    /// Проверяет, что строка подключения в Base64 декодируется.
    /// </summary>
    [Fact]
    public void Resolve_ShouldDecodeConnectionString()
    {
        const string connectionString = "Host=localhost;Database=sign_db;Username=user;Password=pass";

        var signOptions = new SignOptions
        {
            ConnectionString = Convert.ToBase64String(Encoding.UTF8.GetBytes(connectionString))
        };

        var result = SignConnectionStringResolver.Resolve(signOptions);

        Assert.Equal(connectionString, result);
    }

    /// <summary>
    /// Проверяет, что при неверном формате Base64 возвращается понятная ошибка.
    /// </summary>
    [Fact]
    public void Resolve_ShouldThrowArgumentException_WhenBase64ValueIsInvalid()
    {
        var signOptions = new SignOptions
        {
            ConnectionString = "not-base64-value"
        };

        var exception = Assert.Throws<ArgumentException>(() => SignConnectionStringResolver.Resolve(signOptions));

        Assert.Equal("ConnectionString", exception.ParamName);
        Assert.Contains("Base64", exception.Message);
    }
}
