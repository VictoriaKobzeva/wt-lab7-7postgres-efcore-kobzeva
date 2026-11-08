# Лабораторная работа № 7. PostgreSQL и Entity Framework Core

Проект: **wt-lab7-7postgres-efcore-kobzeva**

ASP.NET Core Minimal API для учёта недвижимости. Данные хранятся в PostgreSQL,
доступ к ним выполняется через Entity Framework Core (подход Code First).

## Предметная область

Недвижимость (объекты аренды/продажи). Основная сущность — `Property`:

| Свойство | Тип | Назначение |
|---|---|---|
| `Id` | `int` | Идентификатор |
| `Address` | `string` | Адрес объекта |
| `Type` | `string` | Тип: квартира, дом, офис |
| `Price` | `decimal` | Цена |
| `Area` | `double` | Площадь, м² |
| `Rooms` | `int` | Количество комнат |
| `Floor` | `int` | Этаж |
| `IsAvailable` | `bool` | Доступность |
| `CreatedAt` | `DateTime` | Дата добавления |
| `Description` | `string` | Описание |

## Структура проекта

```
wt-lab7-7postgres-efcore-kobzeva/
├── Data/
│   └── AppDbContext.cs
├── Models/
│   └── Property.cs
├── Migrations/
│   ├── 20261103072107_InitialCreate.cs
│   ├── 20261103072113_AddCreatedAt.cs
│   ├── 20261103072125_SeedProperties.cs
│   ├── 20261103072139_AddDescription.cs
│   ├── 20261108192553_AddFloor.cs
│   └── AppDbContextModelSnapshot.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
├── .gitignore
└── wt-lab7-7postgres-efcore-kobzeva.csproj
```

## API

| Метод | Endpoint | Назначение |
|---|---|---|
| GET | `/` | Проверка работы приложения |
| GET | `/api/info` | Информация о приложении |
| GET | `/api/properties` | Получение всех объектов |
| GET | `/api/properties/{id}` | Получение объекта по идентификатору |
| GET | `/api/properties/search?search=...` | Поиск по адресу, типу и описанию |
| GET | `/api/properties/filter?minPrice=...&maxPrice=...` | Фильтрация по цене (LINQ `Where`) |
| GET | `/api/properties/sorted?descending=true` | Сортировка по цене (LINQ `OrderBy`) |
| GET | `/api/properties/available` | Доступные объекты (LINQ `Where` + `OrderBy`) |
| GET | `/api/properties/stats` | Статистика по типам (LINQ `GroupBy`, собственный запрос) |
| POST | `/api/properties` | Создание объекта |
| PUT | `/api/properties/{id}` | Изменение объекта |
| DELETE | `/api/properties/{id}` | Удаление объекта |

Все операции с данными выполняются через `AppDbContext`.

> В шаблоне лабораторной работы используется сущность `Product` и маршруты
> `/api/products`. В данном проекте сохранена предметная область лабораторной
> работы № 6, поэтому сущность называется `Property`, а маршруты — `/api/properties`.

## Локальный запуск

1. Установить PostgreSQL и создать базу данных `wt_lab7`.
2. Указать строку подключения в `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wt_lab7;Username=postgres;Password=your_password"
  }
}
```

3. Восстановить пакеты и применить миграции:

```bash
dotnet restore
dotnet ef database update
```

4. Запустить приложение:

```bash
dotnet run
```

При запуске приложение автоматически применяет все неприменённые миграции
(`db.Database.Migrate()`), поэтому шаг `dotnet ef database update` можно не выполнять
отдельно.

Проверка:

```bash
curl http://localhost:10000/api/properties
curl http://localhost:10000/api/properties/1
curl "http://localhost:10000/api/properties/search?search=дом"
```

## Миграции

```bash
dotnet ef migrations list
dotnet ef migrations add <Name>
dotnet ef database update
dotnet ef migrations script
```

История применённых миграций хранится в таблице `__EFMigrationsHistory`.
`EnsureCreated()` не используется, так как он обходит систему миграций.

## Docker

```bash
docker build -t wt-lab7-7postgres-efcore .
docker run -p 10000:10000 \
  -e DATABASE_URL="postgres://user:password@host:5432/database" \
  wt-lab7-7postgres-efcore
```

Проверка: <http://localhost:10000/api/properties>

## Публикация на Render

1. Создать PostgreSQL: **New → PostgreSQL**, имя `wt-lab7-db-kobzeva`.
2. Создать Web Service: **New → Web Service**, подключить GitHub-репозиторий,
   выбрать Runtime **Docker**.
3. В настройках Web Service → **Environment** добавить переменную `DATABASE_URL`
   со значением **Internal Database URL** созданной базы PostgreSQL.
4. Выполнить Deploy.

`Program.cs` поддерживает обе конфигурации:

- локально — `ConnectionStrings:DefaultConnection`;
- на Render — переменная окружения `DATABASE_URL`, которая преобразуется
  в строку подключения Npgsql.

Порт берётся из переменной `PORT` (по умолчанию `10000`), приложение слушает
`http://0.0.0.0:{port}`.

> Пароли и полные значения connection string не хранятся в репозитории.
> `appsettings.Development.json` добавлен в `.gitignore`.

## Ссылки

- GitHub-репозиторий: `<URL репозитория>`
- Публичный URL Render: `<https://имя-сервиса>.onrender.com>`
