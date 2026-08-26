using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFN.Sign.Abstractions.Codes;
using SFN.Sign.Abstractions.Messaging;
using SFN.Sign.Abstractions.Services;
using SFN.Sign.Configuration;
using SFN.Sign.Application.Services;
using SFN.Sign.Abstractions.Persistence;
using SFN.Sign.Infrastructure.Messaging;
using SFN.Sign.Infrastructure.Persistence;
using SFN.Sign.Infrastructure.Persistence.Repositories;
using SFN.Sign.Infrastructure.Security;

namespace SFN.Sign.DependencyInjection;

/// <summary>
/// Методы для регистрации библиотеки в DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует библиотеку подписания по настройкам из конфигурации.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="sectionName">Имя раздела с настройками библиотеки.</param>
    /// <returns>Коллекция сервисов.</returns>
    public static IServiceCollection AddSignLibrary(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Sign")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        return services.AddSignLibrary(options =>
        {
            configuration.GetSection(sectionName).Bind(options);

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                options.ConnectionString = configuration.GetConnectionString("Sign") ?? string.Empty;
            }
        });
    }

    /// <summary>
    /// Регистрирует библиотеку подписания и контекст PostgreSQL.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <param name="configureOptions">Настройка параметров библиотеки.</param>
    /// <returns>Коллекция сервисов.</returns>
    public static IServiceCollection AddSignLibrary(
        this IServiceCollection services,
        Action<SignOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<SignOptions>().Configure(configureOptions);
        services.AddDbContext<SignDbContext>((serviceProvider, options) =>
        {
            var signOptions = serviceProvider.GetRequiredService<IOptions<SignOptions>>().Value;
            ArgumentException.ThrowIfNullOrWhiteSpace(signOptions.ConnectionString);

            options.UseNpgsql(
                signOptions.ConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__SignMigrationsHistory", signOptions.Schema));
        });

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
