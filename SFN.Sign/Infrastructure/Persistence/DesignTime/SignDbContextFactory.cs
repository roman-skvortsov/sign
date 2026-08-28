using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration.UserSecrets;
using SFN.Sign.Configuration;

namespace SFN.Sign.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Создает контекст базы данных для команд миграций.
/// </summary>
public sealed class SignDbContextFactory : IDesignTimeDbContextFactory<SignDbContext>
{
    /// <summary>
    /// Создает контекст базы данных для команд миграций.
    /// </summary>
    /// <param name="args">Аргументы команды.</param>
    /// <returns>Контекст базы данных библиотеки.</returns>
    public SignDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables();

        AddUserSecrets(configurationBuilder);

        var configuration = configurationBuilder.Build();

        var signOptions = new SignOptions();
        configuration.GetSection("Sign").Bind(signOptions);
        signOptions.ConnectionString = configuration["Sign:ConnectionString"]
            ?? configuration["ConnectionStrings:Sign"]
            ?? signOptions.ConnectionString;
        signOptions.Schema = configuration["Sign:Schema"] ?? signOptions.Schema;
        var connectionString = SignConnectionStringResolver.Resolve(signOptions);

        var dbContextOptions = new DbContextOptionsBuilder<SignDbContext>()
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__SignMigrationsHistory", signOptions.Schema))
            .Options;

        return new SignDbContext(dbContextOptions, Options.Create(signOptions));
    }

    /// <summary>
    /// Добавляет секреты пользователя из доступных сборок.
    /// </summary>
    /// <param name="configurationBuilder">Построитель конфигурации.</param>
    private static void AddUserSecrets(IConfigurationBuilder configurationBuilder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Concat(LoadAssembliesFromBaseDirectory())
            .Where(x => !x.IsDynamic)
            .GroupBy(x => x.FullName, StringComparer.Ordinal)
            .Select(x => x.First());

        foreach (var assembly in assemblies)
        {
            if (assembly.GetCustomAttribute<UserSecretsIdAttribute>() is null)
            {
                continue;
            }

            configurationBuilder.AddUserSecrets(assembly, optional: true);
        }
    }

    /// <summary>
    /// Загружает сборки из папки запуска команды.
    /// </summary>
    /// <returns>Набор найденных сборок.</returns>
    private static IEnumerable<Assembly> LoadAssembliesFromBaseDirectory()
    {
        foreach (var assemblyPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            Assembly? assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (BadImageFormatException)
            {
            }
            catch (FileLoadException)
            {
            }

            if (assembly is not null)
            {
                yield return assembly;
            }
        }
    }
}
