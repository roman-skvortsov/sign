using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sign.Abstractions.Codes;
using Sign.Abstractions.Messaging;
using Sign.Abstractions.Services;
using Sign.Configuration;
using Sign.Application.Services;
using Sign.Abstractions.Persistence;
using Sign.Infrastructure.Messaging;
using Sign.Infrastructure.Persistence;
using Sign.Infrastructure.Persistence.Repositories;
using Sign.Infrastructure.Security;

namespace Sign.DependencyInjection;

/// <summary>
/// Методы для регистрации библиотеки в DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует библиотеку подписания и контекст PostgreSQL.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <param name="connectionString">Строка подключения к PostgreSQL.</param>
    /// <param name="configureOptions">Настройка параметров библиотеки.</param>
    /// <returns>Коллекция сервисов.</returns>
    public static IServiceCollection AddSignLibrary(
        this IServiceCollection services,
        string connectionString,
        Action<SignOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddOptions<SignOptions>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddDbContext<SignDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ISignRequestRepository, SignRequestRepository>();
        services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();
        services.AddScoped<ISignService, SignService>();
        services.AddSingleton<IVerificationCodeProtector, HmacVerificationCodeProtector>();
        services.AddSingleton<ICodeGenerator, NumericCodeGenerator>();
        services.AddSingleton<ITemplateValueProvider, ReflectionTemplateValueProvider>();
        services.AddSingleton<IMessageTemplateRenderer, DefaultMessageTemplateRenderer>();

        // TODO: Пользователь библиотеки должен зарегистрировать собственные реализации ISignChannelSender
        // для нужных каналов, например EmailSignChannelSender и SmsSignChannelSender.
        // TODO: На этом уровне библиотека не привязана к конкретным пакетам отправки сообщений.

        return services;
    }
}
