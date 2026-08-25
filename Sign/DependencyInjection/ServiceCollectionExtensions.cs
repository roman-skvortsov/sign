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
/// Содержит методы расширения для регистрации библиотеки подписания в контейнере зависимостей.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует сервисы библиотеки подписания документов и контекст PostgreSQL.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="connectionString">Строка подключения к PostgreSQL.</param>
    /// <param name="configureOptions">Делегат конфигурации настроек библиотеки.</param>
    /// <returns>Исходная коллекция сервисов.</returns>
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

        return services;
    }
}
