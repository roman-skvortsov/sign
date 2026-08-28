using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

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
}
