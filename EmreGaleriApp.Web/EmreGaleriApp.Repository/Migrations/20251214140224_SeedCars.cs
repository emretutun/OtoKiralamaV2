using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmreGaleriApp.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "Color", "DailyPrice", "Description", "FuelType", "GearType", "ImageUrl", "IsAvailable", "Mileage", "Model", "ModelYear" },
                values: new object[,]
                {
                    { 1, "Audi", "Siyah", 2500m, "Audi A4 Siyah, konforlu ve şık sedan", "Dizel", 1, "/images/audia4siyah.jpg", true, 85000, "A4", 2020 },
                    { 2, "BMW", "Gri", 2700m, "BMW 320i Gri, sportif sürüş deneyimi", "Benzin", 1, "/images/bmw320igri.jpg", true, 72000, "320i", 2019 },
                    { 3, "Fiat", "Gri", 1500m, "Fiat Egea Gri, ekonomik ve aile dostu", "Dizel", 3, "/images/fiategeagri.jpg", true, 95000, "Egea", 2021 },
                    { 4, "Mercedes", "Beyaz", 3000m, "Mercedes C180 Beyaz, premium sedan", "Benzin", 1, "/images/mercedesc180beyaz.jpg", true, 68000, "C180", 2020 },
                    { 5, "Toyota", "Beyaz", 1800m, "Toyota Corolla Beyaz, sorunsuz ve dayanıklı", "Benzin", 1, "/images/toyotacorollabeyaz.jpg", true, 88000, "Corolla", 2022 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
