using Sign.Application.Codes;

namespace Sign.Abstractions.Codes;

/// <summary>
/// Определяет контракт защиты кода подтверждения с использованием соли и секретного значения.
/// </summary>
public interface IVerificationCodeProtector
{
    /// <summary>
    /// Создает защищенное представление кода подтверждения.
    /// </summary>
    /// <param name="code">Исходный код подтверждения.</param>
    /// <returns>Результат защиты кода подтверждения.</returns>
    VerificationCodeProtectionResult Protect(string code);

    /// <summary>
    /// Проверяет соответствие исходного кода сохраненному защищенному значению.
    /// </summary>
    /// <param name="code">Код подтверждения, введенный пользователем.</param>
    /// <param name="hash">Сохраненный хеш кода подтверждения.</param>
    /// <param name="salt">Сохраненная соль кода подтверждения.</param>
    /// <returns><see langword="true"/>, если код корректен; иначе <see langword="false"/>.</returns>
    bool Verify(string code, string hash, string salt);
}
