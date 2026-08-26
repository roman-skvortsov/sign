using SFN.Sign.Application.Codes;

namespace SFN.Sign.Abstractions.Codes;

/// <summary>
/// Интерфейс защиты кода.
/// </summary>
public interface IVerificationCodeProtector
{
    /// <summary>
    /// Создает защищенный вид кода.
    /// </summary>
    /// <param name="code">Исходный код подтверждения.</param>
    /// <returns>Хеш и соль кода.</returns>
    VerificationCodeProtectionResult Protect(string code);

    /// <summary>
    /// Проверяет, совпадает ли код с сохраненным значением.
    /// </summary>
    /// <param name="code">Код подтверждения, введенный пользователем.</param>
    /// <param name="hash">Сохраненный хеш кода.</param>
    /// <param name="salt">Сохраненная соль кода.</param>
    /// <returns><see langword="true"/>, если код корректен; иначе <see langword="false"/>.</returns>
    bool Verify(string code, string hash, string salt);
}
