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
            new Property
            {
                Id = 1,
                Address = "ул. Ленина, 10",
                Type = "Квартира",
                Price = 75000,
                Area = 54.5,
                Rooms = 2,
                Floor = 4,
                IsAvailable = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Property
            {
                Id = 2,
                Address = "пр. Независимости, 25",
                Type = "Квартира",
                Price = 120000,
                Area = 78.2,
                Rooms = 3,
                Floor = 7,
                IsAvailable = true,
                CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new Property
            {
                Id = 3,
                Address = "ул. Садовая, 7",
                Type = "Дом",
                Price = 210000,
                Area = 145.0,
                Rooms = 5,
                Floor = 2,
                IsAvailable = false,
                CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new Property
            {
                Id = 4,
                Address = "ул. Лесная, 3",
                Type = "Дом",
                Price = 98000,
                Area = 96.4,
                Rooms = 4,
                Floor = 2,
                IsAvailable = true,
                CreatedAt = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
            },
            new Property
            {
                Id = 5,
                Address = "ул. Центральная, 1",
                Type = "Офис",
                Price = 45000,
                Area = 32.0,
                Rooms = 1,
                Floor = 1,
                IsAvailable = true,
                CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
