using Sign.Application.Contracts;

namespace Sign.Abstractions.Services;

/// <summary>
/// Определяет контракт сервиса управления процессом подписания документов.
/// </summary>
public interface ISignService
{
    /// <summary>
    /// Создает новый запрос на подписание, генерирует код и отправляет сообщение получателю.
    /// </summary>
    /// <param name="request">Параметры запуска подписания.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат запуска процесса подписания.</returns>
    Task<SigningResult> StartSigningAsync(StartSigningRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет код подтверждения для ранее созданного запроса на подписание.
    /// </summary>
    /// <param name="request">Параметры проверки кода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат проверки кода.</returns>
    Task<VerificationResult> VerifyCodeAsync(VerifySigningCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет повторную отправку кода подтверждения для существующего запроса на подписание.
    /// </summary>
    /// <param name="request">Параметры повторной отправки кода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат повторной отправки кода.</returns>
    Task<SigningResult> ResendCodeAsync(ResendSigningCodeRequest request, CancellationToken cancellationToken = default);
}
