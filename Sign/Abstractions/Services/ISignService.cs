using Sign.Application.Contracts;

namespace Sign.Abstractions.Services;

/// <summary>
/// Интерфейс сервиса подписания.
/// </summary>
public interface ISignService
{
    /// <summary>
    /// Запускает подписание, создает код и отправляет сообщение.
    /// </summary>
    /// <param name="request">Данные для запуска подписания.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат запуска подписания.</returns>
    Task<StartSigningResult> StartSigningAsync(StartSigningRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет код для запроса на подписание.
    /// </summary>
    /// <param name="request">Данные для проверки кода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат проверки кода.</returns>
    Task<VerifyCodeResult> VerifyCodeAsync(VerifyCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Повторно отправляет код для запроса на подписание.
    /// </summary>
    /// <param name="request">Данные для повторной отправки кода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат повторной отправки кода.</returns>
    Task<ResendCodeResult> ResendCodeAsync(ResendCodeRequest request, CancellationToken cancellationToken = default);
}
