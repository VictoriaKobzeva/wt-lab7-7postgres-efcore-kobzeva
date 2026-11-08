# Лабораторная работа № 7

## PostgreSQL и Entity Framework Core: ORM, Code First, миграции и публикация на Render

**Дисциплина:** Web-технологии  
**Проект:** `wt-lab7-7postgres-efcore-kobzeva`  
**Выполнила:** Кобзева В.  
**Дата:** 03.11.2026

---

## 1. Тема и цель работы

**Тема:** подключение PostgreSQL к ASP.NET Core Minimal API через Entity Framework Core,
использование подхода Code First, миграций и публикация приложения на Render.

**Цель:** освоить работу с ORM Entity Framework Core и СУБД PostgreSQL, заменить
хранилище данных в оперативной памяти на базу данных, автоматизировать применение
миграций при запуске приложения.

---

## 2. Название проекта и предметная область

Проект создан на основе лабораторной работы № 6: `wt-lab6-6minimalapi-kobzeva`.

**Предметная область:** недвижимость (объекты аренды/продажи).

Основная сущность — `Property`:

| Свойство | Тип | Назначение |
|---|---|---|
| `Id` | `int` | Идентификатор |
| `Address` | `string` | Адрес |
| `Type` | `string` | Тип объекта |
| `Price` | `decimal` | Цена |
| `Area` | `double` | Площадь |
| `Rooms` | `int` | Количество комнат |
| `IsAvailable` | `bool` | Доступность |
| `CreatedAt` | `DateTime` | Дата добавления |
| `Description` | `string` | Описание |

---

## 3. Исходная модель из WT-6

```csharp
public class Property
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Price { get; set; }
    public double Area { get; set; }
    public int Rooms { get; set; }
    public bool IsAvailable { get; set; }
}
```

---

## 4. Подключение Entity Framework Core

Установлены пакеты:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet tool install --global dotnet-ef --version 8.0.11
```

Провайдер `Npgsql.EntityFrameworkCore.PostgreSQL` обеспечивает работу EF Core
с PostgreSQL.

---

## 5. AppDbContext

```csharp
using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_kobzeva.Models;

namespace wt_lab7_7postgres_efcore_kobzeva.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>().HasData(
            new Property { Id = 1, Address = "ул. Ленина, 10", Type = "Квартира", Price = 75000, Area = 54.5, Rooms = 2, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Property { Id = 2, Address = "пр. Независимости, 25", Type = "Квартира", Price = 120000, Area = 78.2, Rooms = 3, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new Property { Id = 3, Address = "ул. Садовая, 7", Type = "Дом", Price = 210000, Area = 145.0, Rooms = 5, IsAvailable = false, CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
            new Property { Id = 4, Address = "ул. Лесная, 3", Type = "Дом", Price = 98000, Area = 96.4, Rooms = 4, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Property { Id = 5, Address = "ул. Центральная, 1", Type = "Офис", Price = 45000, Area = 32.0, Rooms = 1, IsAvailable = true, CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
```

---

## 6. Строка подключения

Локально используется `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wt_lab7;Username=postgres;Password=your_password"
  }
}
```

Файл добавлен в `.gitignore`, так как содержит локальный пароль.

На Render используется переменная окружения `DATABASE_URL`. В `Program.cs`
выполняется её преобразование в строку подключения Npgsql:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var dbPort = uri.Port > 0 ? uri.Port : 5432;
    var sslMode = uri.Host.Contains('.') ? "Require" : "Disable";

    connectionString =
        $"Host={uri.Host};" +
        $"Port={dbPort};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={Uri.UnescapeDataString(userInfo[0])};" +
        $"Password={Uri.UnescapeDataString(userInfo[1])};" +
        $"SSL Mode={sslMode}";
}
```

---

## 7. Первая миграция

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Создана таблица `Properties` и таблица истории `__EFMigrationsHistory`.

`[СКРИНШОТ: структура таблицы Properties в pgAdmin/DBeaver]`

`[СКРИНШОТ: таблица __EFMigrationsHistory]`

---

## 8. Автоматическое применение миграций при запуске

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

Последовательность запуска:

```
Запуск приложения
      ↓
Подключение к PostgreSQL
      ↓
Проверка миграций
      ↓
Применение отсутствующих миграций
      ↓
Запуск HTTP API
```

`EnsureCreated()` не используется, так как он обходит систему миграций.

---

## 9. CRUD через Entity Framework Core

### Получение списка

```csharp
app.MapGet("/api/properties", async (AppDbContext db) =>
{
    var properties = await db.Properties.ToListAsync();
    return Results.Ok(properties);
});
```

### Получение по идентификатору

```csharp
app.MapGet("/api/properties/{id:int}", async (int id, AppDbContext db) =>
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id);
    if (property == null)
        return Results.NotFound();
    return Results.Ok(property);
});
```

### Создание

```csharp
app.MapPost("/api/properties", async (Property property, AppDbContext db) =>
{
    if (property.CreatedAt == default)
        property.CreatedAt = DateTime.UtcNow;

    db.Properties.Add(property);
    await db.SaveChangesAsync();

    return Results.Created($"/api/properties/{property.Id}", property);
});
```

### Изменение

```csharp
app.MapPut("/api/properties/{id:int}", async (int id, Property updatedProperty, AppDbContext db) =>
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id);
    if (property == null)
        return Results.NotFound();

    property.Address = updatedProperty.Address;
    property.Type = updatedProperty.Type;
    property.Price = updatedProperty.Price;
    property.Area = updatedProperty.Area;
    property.Rooms = updatedProperty.Rooms;
    property.IsAvailable = updatedProperty.IsAvailable;
    property.Description = updatedProperty.Description;

    await db.SaveChangesAsync();
    return Results.Ok(property);
});
```

### Удаление

```csharp
app.MapDelete("/api/properties/{id:int}", async (int id, AppDbContext db) =>
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id);
    if (property == null)
        return Results.NotFound();

    db.Properties.Remove(property);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
```

### Результаты проверки CRUD

| Операция | Запрос | Результат |
|---|---|---|
| Create | `POST /api/properties` | `201 Created`, объект получил `Id = 6` |
| Read | `GET /api/properties/6` | `200 OK`, данные объекта |
| Update | `PUT /api/properties/6` | `200 OK`, цена изменена на `99999` |
| Delete | `DELETE /api/properties/6` | `204 No Content` |
| Read after delete | `GET /api/properties/6` | `404 Not Found` |

`[СКРИНШОТ: выполнение CRUD в Postman/curl]`

---

## 10. LINQ-запросы

### Фильтрация

```csharp
var properties = await db.Properties
    .Where(x => x.Price >= minPrice)
    .ToListAsync();
```

### Сортировка

```csharp
var properties = await db.Properties
    .OrderBy(x => x.Price)
    .ToListAsync();
```

### Выборка с условием

```csharp
var properties = await db.Properties
    .Where(x => x.IsAvailable)
    .OrderBy(x => x.Address)
    .ToListAsync();
```

### Собственный запрос: статистика по типам

```csharp
var stats = await db.Properties
    .GroupBy(x => x.Type)
    .Select(g => new
    {
        Type = g.Key,
        Count = g.Count(),
        AveragePrice = g.Average(x => x.Price),
        MinPrice = g.Min(x => x.Price),
        MaxPrice = g.Max(x => x.Price)
    })
    .OrderByDescending(x => x.Count)
    .ToListAsync();
```

Пример SQL, сформированного EF Core:

```sql
SELECT p."Type", count(*)::int AS "Count", avg(p."Price") AS "AveragePrice",
       min(p."Price") AS "MinPrice", max(p."Price") AS "MaxPrice"
FROM "Properties" AS p
GROUP BY p."Type"
ORDER BY count(*)::int DESC;
```

`[СКРИНШОТ: ответ /api/properties/stats]`

---

## 11. Изменение модели и вторая миграция

Добавлено свойство:

```csharp
public DateTime CreatedAt { get; set; }
```

Создана и применена миграция:

```bash
dotnet ef migrations add AddCreatedAt
dotnet ef database update
```

Затем добавлены начальные данные (`HasData`) и создана миграция `SeedProperties`.

Позже добавлено свойство `Description` и создана миграция `AddDescription`:

```bash
dotnet ef migrations add AddDescription
dotnet ef database update
```

Список миграций:

```
20261103072107_InitialCreate
20261103072113_AddCreatedAt
20261103072125_SeedProperties
20261103072139_AddDescription
```

`[СКРИНШОТ: dotnet ef migrations list]`

---

## 12. Структура PostgreSQL

Таблицы:

```
Properties
__EFMigrationsHistory
```

Столбцы `Properties`:

```
Id          integer
Address     text
Type        text
Price       numeric
Area        double precision
Rooms       integer
IsAvailable boolean
CreatedAt   timestamp with time zone
Description text
```

SQL-проверка:

```sql
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'Properties'
ORDER BY ordinal_position;
```

`[СКРИНШОТ: результат SQL-запроса]`

---

## 13. Начальные данные

Добавлены пять объектов недвижимости через `HasData`. Проверка:

```sql
SELECT * FROM "Properties";
```

`[СКРИНШОТ: содержимое таблицы Properties]`

---

## 14. Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["dotnet", "wt-lab7-7postgres-efcore-kobzeva.dll"]
```

Приложение читает порт из переменной `PORT`:

```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");
```

---

## 15. Настройка DATABASE_URL и PostgreSQL на Render

1. Создана база PostgreSQL в Render: `wt-lab7-db-kobzeva`.
2. Создан Web Service с Runtime **Docker**.
3. В разделе **Environment** добавлена переменная `DATABASE_URL`
   со значением **Internal Database URL**.
4. Выполнен Deploy.

`[СКРИНШОТ: настройки Environment Web Service без значения секрета]`

`[СКРИНШОТ: страница PostgreSQL в Render]`

`[СКРИНШОТ: успешный Deploy]`

---

## 16. Логи успешного запуска

В логах Render видно:

```
Application starting
      ↓
Connection to PostgreSQL
      ↓
EF Core
      ↓
Migrations
      ↓
Application started
```

`[СКРИНШОТ: логи Render]`

---

## 17. Проверка REST API после публикации

Публичный URL: `[https://<имя-сервиса>.onrender.com]`

| Метод | Endpoint | Результат |
|---|---|---|
| GET | `/` | `Minimal API работает!` |
| GET | `/api/info` | JSON с информацией о приложении |
| GET | `/api/properties` | список объектов из PostgreSQL |
| GET | `/api/properties/1` | объект с `Id = 1` |
| POST | `/api/properties` | `201 Created` |
| PUT | `/api/properties/1` | `200 OK` |
| DELETE | `/api/properties/1` | `204 No Content` |

`[СКРИНШОТ: ответы API после публикации]`

---

## 18. Повторная публикация после изменения модели

Добавлено свойство `Description`, создана миграция `AddDescription`,
изменения отправлены в GitHub:

```bash
git add .
git commit -m "Add property description"
git push
```

После нового Deploy на Render в таблице `Properties` появился столбец
`Description`. Это подтверждает полный цикл:

```
Изменение C#-модели
      ↓
Создание миграции
      ↓
Git commit
      ↓
Git push
      ↓
Render Deploy
      ↓
Migrate()
      ↓
Обновление PostgreSQL
      ↓
Обновлённый REST API
```

`[СКРИНШОТ: структура Properties после повторного Deploy]`

---

## 19. Ссылки

- GitHub-репозиторий: `[URL репозитория]`
- Публичный URL Render: `[https://<имя-сервиса>.onrender.com]`

---

## 20. Вывод

В ходе работы изучены Entity Framework Core, подход Code First, механизм миграций
и работа с PostgreSQL. Хранилище в оперативной памяти заменено на базу данных,
реализованы CRUD-операции и LINQ-запросы через `AppDbContext`. Настроено
автоматическое применение миграций при запуске приложения, подготовлен Dockerfile
и конфигурация для публикации на Render с использованием переменной `DATABASE_URL`.
