using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wt_lab7_7postgres_efcore_kobzeva.Migrations
{
    /// <inheritdoc />
    public partial class SeedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Properties",
                columns: new[] { "Id", "Address", "Area", "CreatedAt", "IsAvailable", "Price", "Rooms", "Type" },
                values: new object[,]
                {
                    { 1, "ул. Ленина, 10", 54.5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 75000m, 2, "Квартира" },
                    { 2, "пр. Независимости, 25", 78.200000000000003, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), true, 120000m, 3, "Квартира" },
                    { 3, "ул. Садовая, 7", 145.0, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), false, 210000m, 5, "Дом" },
                    { 4, "ул. Лесная, 3", 96.400000000000006, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), true, 98000m, 4, "Дом" },
                    { 5, "ул. Центральная, 1", 32.0, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, 45000m, 1, "Офис" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
