# SFN.Sign

Библиотека для подписания документов по SMS и email на `.NET 10`.

Библиотека хранит данные в PostgreSQL, использует свою схему `sign`, ведет журнал отправок и проверок кода, хранит шаблоны сообщений в базе данных и позволяет подключать свои отправители для разных каналов подписания.

## Что умеет

- запускать подписание документа;
- отправлять код подтверждения по SMS или email;
- повторно отправлять код с учетом ограничений по попыткам и интервалам;
- проверять код подтверждения;
- хранить коды в виде хеша с солью и перцем;
- хранить шаблоны сообщений в базе данных;
- применять миграции своей схемы в общей базе данных.

## Подключение

Установите ссылку на проект или пакет `SFN.Sign`, затем зарегистрируйте библиотеку в `Program.cs`.

Важно: для метода `AddSignLibrary(...)` нужен `using SFN.Sign.DependencyInjection;`.

```csharp
using SFN.Sign.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignLibrary(builder.Configuration);

// Пользователь библиотеки должен сам зарегистрировать отправители сообщений.
// Примеры:
// builder.Services.AddScoped<ISignChannelSender, SmsSignChannelSender>();
// builder.Services.AddScoped<ISignChannelSender, EmailSignChannelSender>();
```

## Пример настроек

Пример `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Sign": "Host=localhost;Port=5432;Database=app_db;Username=postgres;Password=postgres"
  },
  "Sign": {
    "ConnectionString": "",
    "Schema": "sign",
    "CodeLifetime": "00:05:00",
    "RetryCount": 3,
    "RetryInterval": "00:00:10",
    "ResendCooldown": "00:01:00",
    "ExtendedResendCooldownAfterAttemptCount": 3,
    "ExtendedResendCooldown": "00:05:00",
    "MaxVerifyAttempts": 5,
    "MaxSendAttempts": 3,
    "SmsCodeLength": 4,
    "EmailCodeLength": 6,
    "HashPepper": "your-secret-pepper",
    "SaltSize": 16
  }
}
```

Примечания:

- если `Sign:ConnectionString` пустая, библиотека возьмет строку подключения из `ConnectionStrings:Sign`;
- `HashPepper` лучше хранить не в файле, а во внешнем хранилище секретов или в переменной окружения.
- файл с пояснениями по всем настройкам: [appsettings.sign.example.jsonc](/Users/macbook/Documents/Projects/sign/appsettings.sign.example.jsonc)

### Что означает каждая настройка

- `ConnectionStrings:Sign` - запасная строка подключения к базе данных.
- `Sign:ConnectionString` - основная строка подключения библиотеки.
- `Sign:Schema` - схема базы данных для таблиц библиотеки.
- `Sign:CodeLifetime` - время жизни кода подтверждения.
- `Sign:RetryCount` - сколько раз повторять отправку сообщения при ошибке отправителя.
- `Sign:RetryInterval` - пауза между повторными попытками отправки сообщения.
- `Sign:ResendCooldown` - минимальная пауза между отправками нового кода.
- `Sign:ExtendedResendCooldownAfterAttemptCount` - после какого числа отправок включать увеличенную паузу.
- `Sign:ExtendedResendCooldown` - увеличенная пауза между отправками после достижения порога.
- `Sign:MaxVerifyAttempts` - максимальное число попыток ввода кода.
- `Sign:MaxSendAttempts` - максимальное число отправок кода для одного запроса.
- `Sign:SmsCodeLength` - длина кода для SMS.
- `Sign:EmailCodeLength` - длина кода для email.
- `Sign:HashPepper` - секретное значение для хеширования кода.
- `Sign:SaltSize` - размер соли в байтах.

## Секрет для хеширования

Настройка `Sign:HashPepper` является секретом.

Ее лучше не хранить в `appsettings.json` и не коммитить в репозиторий. Рекомендуется передавать это значение через безопасное хранилище секретов или через переменные окружения.

Рекомендуемая длина секрета:

- минимум `32` байта случайных данных;
- лучше `48` или `64` байта;
- удобно хранить значение в виде `Base64`.

Примеры длины в `Base64`:

- `32` байта - около `44` символов;
- `48` байт - около `64` символов;
- `64` байта - около `88` символов.

Пример через переменную окружения:

```bash
Sign__HashPepper=your-secret-value
```

Если используется `appsettings.json`, лучше оставить поле пустым:

```json
{
  "Sign": {
    "HashPepper": ""
  }
}
```

Подходящие варианты хранения:

- Vault;
- Kubernetes Secret;
- Secret Manager;
- переменные окружения CI/CD;
- секреты хостинга или контейнера.

## Миграции

Миграции лежат внутри проекта `SFN.Sign`.

Таблицы библиотеки создаются в схеме `sign`, а история миграций хранится в таблице `sign.__SignMigrationsHistory`.

Пример команд:

```bash
dotnet ef database update --project SFN.Sign/SFN.Sign.csproj --startup-project SFN.Sign/SFN.Sign.csproj
```

```bash
dotnet ef migrations add MigrationName --project SFN.Sign/SFN.Sign.csproj --startup-project SFN.Sign/SFN.Sign.csproj --output-dir Infrastructure/Persistence/Migrations
```

## Структура данных

Библиотека использует таблицы:

- `sign.SignRequests`
- `sign.SignCodes`
- `sign.SignAttempts`
- `sign.MessageTemplates`

## Важно

- Библиотека не содержит HTTP API.
- Библиотека не отправляет SMS и email сама по себе без ваших реализаций `ISignChannelSender`.
- Для каждого нового способа отправки можно добавить свою реализацию `ISignChannelSender`.
