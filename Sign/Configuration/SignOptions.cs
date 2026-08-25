namespace Sign.Configuration;

/// <summary>
/// Содержит общие настройки библиотеки подписания документов.
/// </summary>
public sealed class SignOptions
{
    /// <summary>
    /// Получает или задает имя схемы базы данных.
    /// </summary>
    public string Schema { get; set; } = "sign";

    /// <summary>
    /// Получает или задает срок действия кода подтверждения.
    /// </summary>
    public TimeSpan CodeLifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Получает или задает количество повторных попыток отправки сообщения.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Получает или задает интервал между повторными попытками отправки сообщения.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Получает или задает минимальный интервал между повторными отправками кода.
    /// </summary>
    public TimeSpan ResendCooldown { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Получает или задает количество отправок, после которого применяется увеличенный интервал повторной отправки.
    /// </summary>
    public int ExtendedResendCooldownAfterAttemptCount { get; set; } = 3;

    /// <summary>
    /// Получает или задает увеличенный интервал между повторными отправками кода после достижения порога попыток.
    /// </summary>
    public TimeSpan ExtendedResendCooldown { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Получает или задает максимально допустимое число попыток проверки кода.
    /// </summary>
    public int MaxVerifyAttempts { get; set; } = 5;

    /// <summary>
    /// Получает или задает максимально допустимое число отправок кода.
    /// </summary>
    public int MaxSendAttempts { get; set; } = 3;

    /// <summary>
    /// Получает или задает длину SMS-кода.
    /// </summary>
    public int SmsCodeLength { get; set; } = 4;

    /// <summary>
    /// Получает или задает длину email-кода.
    /// </summary>
    public int EmailCodeLength { get; set; } = 6;

    /// <summary>
    /// Получает или задает секретное значение, используемое при хешировании кода подтверждения.
    /// </summary>
    public string HashPepper { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает размер соли в байтах для защиты кода подтверждения.
    /// </summary>
    public int SaltSize { get; set; } = 16;

}
