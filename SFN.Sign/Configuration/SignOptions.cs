namespace SFN.Sign.Configuration;

/// <summary>
/// Настройки библиотеки подписания.
/// </summary>
public sealed class SignOptions
{
    /// <summary>
    /// Имя схемы базы данных.
    /// </summary>
    public string Schema { get; set; } = "sign";

    /// <summary>
    /// Срок действия кода.
    /// </summary>
    public TimeSpan CodeLifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Число повторных попыток отправки сообщения.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Интервал между повторными попытками отправки сообщения.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Минимальный интервал между повторными отправками кода.
    /// </summary>
    public TimeSpan ResendCooldown { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Число отправок, после которого увеличивается интервал повторной отправки.
    /// </summary>
    public int ExtendedResendCooldownAfterAttemptCount { get; set; } = 3;

    /// <summary>
    /// Увеличенный интервал между повторными отправками кода.
    /// </summary>
    public TimeSpan ExtendedResendCooldown { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Максимальное число попыток проверки кода.
    /// </summary>
    public int MaxVerifyAttempts { get; set; } = 5;

    /// <summary>
    /// Максимальное число отправок кода.
    /// </summary>
    public int MaxSendAttempts { get; set; } = 3;

    /// <summary>
    /// Длина SMS-кода.
    /// </summary>
    public int SmsCodeLength { get; set; } = 4;

    /// <summary>
    /// Длина кода для email.
    /// </summary>
    public int EmailCodeLength { get; set; } = 6;

    /// <summary>
    /// Секретное значение для хеширования кода.
    /// </summary>
    public string HashPepper { get; set; } = string.Empty;

    /// <summary>
    /// Размер соли в байтах.
    /// </summary>
    public int SaltSize { get; set; } = 16;

}
