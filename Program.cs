using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_kobzeva.Data;
using wt_lab7_7postgres_efcore_kobzeva.Models;

var builder = WebApplication.CreateBuilder(args);

// Строка подключения: локально — ConnectionStrings:DefaultConnection,
// на Render — переменная окружения DATABASE_URL.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var dbPort = uri.Port > 0 ? uri.Port : 5432;
    // Render: внутреннее подключение (host без домена) не использует TLS,
    // внешнее подключение (host с доменом) требует SSL.
    var sslMode = uri.Host.Contains('.') ? "Require" : "Disable";

    connectionString =
        $"Host={uri.Host};" +
        $"Port={dbPort};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={Uri.UnescapeDataString(userInfo[0])};" +
        $"Password={Uri.UnescapeDataString(userInfo[1])};" +
        $"SSL Mode={sslMode}";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Автоматически применяем все существующие миграции при запуске.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => "Minimal API работает!");

app.MapGet("/api/info", () => new
{
    Application = "Web Technologies",
    Technology = "ASP.NET Core Minimal API + Entity Framework Core",
    Database = "PostgreSQL",
    Version = "2.0"
});

// 13.1. Получение списка
app.MapGet("/api/properties", async (AppDbContext db) =>
{
    var properties = await db.Properties.ToListAsync();
    return Results.Ok(properties);
});

// 15. Поиск по адресу/типу с использованием LINQ
app.MapGet("/api/properties/search", async (string? search, AppDbContext db) =>
{
    var query = db.Properties.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var value = search.ToLower();
        query = query.Where(x =>
            x.Address.ToLower().Contains(value) ||
            x.Type.ToLower().Contains(value) ||
            x.Description.ToLower().Contains(value));
    }

    var properties = await query.ToListAsync();
    return Results.Ok(properties);
});

// 19. LINQ: фильтрация по цене
app.MapGet("/api/properties/filter", async (decimal? minPrice, decimal? maxPrice, AppDbContext db) =>
{
    var query = db.Properties.AsQueryable();

    if (minPrice.HasValue)
        query = query.Where(x => x.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(x => x.Price <= maxPrice.Value);

    var properties = await query.ToListAsync();
    return Results.Ok(properties);
});

// 19. LINQ: сортировка по цене
app.MapGet("/api/properties/sorted", async (bool? descending, AppDbContext db) =>
{
    var query = db.Properties.AsQueryable();

    query = descending == true
        ? query.OrderByDescending(x => x.Price)
        : query.OrderBy(x => x.Price);

    var properties = await query.ToListAsync();
    return Results.Ok(properties);
});

// 19. LINQ: выборка с условием
app.MapGet("/api/properties/available", async (AppDbContext db) =>
{
    var properties = await db.Properties
        .Where(x => x.IsAvailable)
        .OrderBy(x => x.Address)
        .ToListAsync();

    return Results.Ok(properties);
});

// 19. Собственный LINQ-запрос: статистика по типам недвижимости
app.MapGet("/api/properties/stats", async (AppDbContext db) =>
{
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

    return Results.Ok(stats);
});

// 14. Получение объекта по идентификатору
app.MapGet("/api/properties/{id:int}", async (int id, AppDbContext db) =>
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id);

    if (property == null)
        return Results.NotFound();

    return Results.Ok(property);
});

// 16. Добавление объекта
app.MapPost("/api/properties", async (Property property, AppDbContext db) =>
{
    if (property.CreatedAt == default)
        property.CreatedAt = DateTime.UtcNow;

    db.Properties.Add(property);
    await db.SaveChangesAsync();

    return Results.Created($"/api/properties/{property.Id}", property);
});

// 17. Изменение объекта
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
    property.Floor = updatedProperty.Floor;
    property.IsAvailable = updatedProperty.IsAvailable;
    property.Description = updatedProperty.Description;

    await db.SaveChangesAsync();
    return Results.Ok(property);
});

// 18. Удаление объекта
app.MapDelete("/api/properties/{id:int}", async (int id, AppDbContext db) =>
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id);

    if (property == null)
        return Results.NotFound();

    db.Properties.Remove(property);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");
