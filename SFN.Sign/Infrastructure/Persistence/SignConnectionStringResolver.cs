using System.Text;
using SFN.Sign.Configuration;

namespace SFN.Sign.Infrastructure.Persistence;

/// <summary>
/// Готовит строку подключения к базе данных.
/// </summary>
internal static class SignConnectionStringResolver
{
    /// <summary>
    /// Возвращает готовую строку подключения.
    /// </summary>
    /// <param name="signOptions">Настройки библиотеки.</param>
    /// <returns>Строка подключения к базе данных.</returns>
    public static string Resolve(SignOptions signOptions)
    {
        ArgumentNullException.ThrowIfNull(signOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(signOptions.ConnectionString);

        try
        {
            var bytes = Convert.FromBase64String(signOptions.ConnectionString);
            var decodedValue = Encoding.UTF8.GetString(bytes);

            ArgumentException.ThrowIfNullOrWhiteSpace(decodedValue);

            return decodedValue;
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Строка подключения в Base64 имеет неверный формат.", nameof(signOptions.ConnectionString), exception);
        }
    }
}
