# TimescaleApp

## О проекте

WebAPI-приложение для работы с timescale-данными некоторых результатов обработки.

Стек:

- .NET 10;
- ASP.NET Core WebAPI;
- EF Core;
- Swagger;
- PostgreSQL;
- xUnit 4, Testcontainers.

## Быстрый запуск

Запуск базы данных в Docker:

```
docker run --name timescale-app-db -d -p 5432:5432 -e POSTGRES_DB=timescale_app -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres postgres:18.6-alpine
```

> [!NOTE]
> По умолчанию используется PostgreSQL по адресу `127.0.0.1:5432`, база данных `timescale_app`, имя пользователя `postgres`, пароль `postgres`. Для использования других данных передайте строку через переменную окружения `ConnectionStrings__TimescaleAppDatabase`.

Восстановление зависимостей и применение миграций:

```
dotnet tool restore
dotnet restore
dotnet dotnet-ef database update --project Data --startup-project WebApi
```

Запуск WebAPI:

```
dotnet run --project WebApi --launch-profile http
```

Swagger будет доступен по адресу <http://localhost:5075/swagger>.

## API

### Импорт файлов

Производит обработку и сохранение данных файла в БД.

`POST /api/v1/files/import`  
`Content-Type: multipart/form-data`  
Имя поля - `file`

Пример файла:

```
Date;ExecutionTime;Value
2026-01-01T10-30-00.0000Z;0.42;12.5
2026-01-01T10-31-00.0000Z;0.37;15.8
```

Ограничения:

- Дата не может быть позже текущей и раньше 01.01.2000;
- Время выполнения не может быть меньше 0;
- Значение показателя не может быть меньше 0;
- Количество строк не может быть меньше 1 и больше 10 000;
- Значения должны соответствовать своим типам, отсутствие одного из значений в записи недопустимо;
- Если файл с таким именем уже существует, значения в базе перезаписываются.

### Поиск результатов

Получает список записей из таблицы Results, подходящих под фильтры.

`GET /api/v1/results/search`

Поддерживаемые фильтры (query-параметры):

- `filename`;
- `startDateFrom`, `startDateTo`;
- `averageValueFrom`, `averageValueTo`;
- `averageExecutionTimeFrom`, `averageExecutionTimeTo`.

### Последние значения файла

Возвращает последние 10 значений, отсортированных по начальному времени запуска Date по имени заданного файла.

`GET /api/v1/files/latest-values?filename=data.csv`

## Тесты

Для запуска тестов необходим запущенный Docker (для работы PostgreSQL в Testcontainers). Команда:

```
dotnet test --project Tests/Tests.csproj
```

## Архитектура

Ответственность проектов:

- `WebApi` - DI, контроллеры, обработчики HTTP-ошибок;
- `Services.Interfaces` - контракты, DTO;
- `Services.Implementations` - бизнес-логика проекта;
- `Data` - сущности, EF-конфигурации и миграции;
- `Tests` - тесты сервисов.

При добавлении функциональности следует добавить контракт и модели в `Services.Interfaces`, реализацию в `Services.Implementations` и зарегистрировать её в `WebApi/Program.cs`.

Для создания миграции при изменении схемы БД следует использовать команду:

```
dotnet dotnet-ef migrations add MigrationName --project Data --startup-project WebApi
dotnet dotnet-ef database update --project Data --startup-project WebApi
```
