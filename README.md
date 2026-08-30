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

builder.Services.AddSmsApiClient(builder.Configuration);
builder.Services.AddDpvApiClient(builder.Configuration);
builder.Services.AddSignLibrary(builder.Configuration);
```

Важно:

- `AddSmsApiClient(builder.Configuration)` регистрирует `ISmsApiClient`;
- `AddDpvApiClient(builder.Configuration)` регистрирует `IDpvApiClient`;
- `AddSignLibrary(builder.Configuration)` регистрирует сервис подписания и отправители, которые используют эти клиенты.

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
- библиотека всегда ожидает строку подключения в `Base64` и декодирует ее перед подключением;
- `HashPepper` лучше хранить не в файле, а во внешнем хранилище секретов или в переменной окружения.
- файл с пояснениями по всем настройкам: [appsettings.sign.example.jsonc](/Users/macbook/Documents/Projects/sign/appsettings.sign.example.jsonc)
- строку подключения рекомендуется хранить в `Vault`, а для локальной разработки в `Secrets`;
- в библиотеке ожидается, что строка подключения хранится в `Base64`.

### Что означает каждая настройка

- `ConnectionStrings:Sign` - запасная строка подключения к базе данных.
- `Sign:ConnectionString` - строка подключения библиотеки в виде `Base64`.
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
- `Sign:HashPepper` - секретное значение для хеширования кода в виде строки `Base64`.
- `Sign:SaltSize` - размер соли в байтах.

## Секрет для хеширования

Настройка `Sign:HashPepper` является секретом.

Ее лучше не хранить в `appsettings.json` и не коммитить в репозиторий. Рекомендуется передавать это значение через безопасное хранилище секретов или через переменные окружения.

В этой настройке должна быть строка с секретом в `Base64`.

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

## Строка подключения

Строку подключения нужно хранить не в файле, а во внешнем секрете. Для рабочего окружения рекомендуется `Vault`, для локальной разработки `Secrets`.

В библиотеке ожидается, что строка подключения хранится в виде `Base64`:

```json
{
  "Sign": {
    "ConnectionString": "SE9zdD1sb2NhbGhvc3Q7RGF0YWJhc2U9YXBwX2RiO1VzZXJuYW1lPXBvc3RncmVzO1Bhc3N3b3JkPXBvc3RncmVz"
  }
}
```

Библиотека сама декодирует это значение перед подключением к PostgreSQL.

## Миграции

Миграции лежат внутри проекта `SFN.Sign`.

Таблицы библиотеки создаются в схеме `sign`, а история миграций хранится в таблице `sign.__SignMigrationsHistory`.

Для design-time команд EF `SignDbContextFactory` читает настройки только из:

- `User Secrets`;
- переменных окружения.

Файлы `appsettings.json` и `appsettings.Development.json` в design-time фабрике не используются.

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
- Для работы отправки нужно заранее зарегистрировать `AddSmsApiClient(builder.Configuration)` и `AddDpvApiClient(builder.Configuration)`.
- Для каждого нового способа отправки можно добавить свою реализацию `ISignChannelSender`.
