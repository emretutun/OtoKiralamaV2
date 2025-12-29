using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmreGaleriApp.Repository.SeedData
{
    public static class CarSeedData
    {
        public static void SeedCars(this ModelBuilder builder)
        {
            builder.Entity<Car>().HasData(
                new Car
                {
                    Id = 1,
                    Brand = "Audi",
                    Model = "A4",
                    Description = "Audi A4 Siyah, konforlu ve şık sedan",
                    ImageUrl = "/images/audia4siyah.jpg",
                    DailyPrice = 2500m,
                    IsAvailable = true,
                    Mileage = 85000,
                    FuelType = "Dizel",
                    Color = "Siyah",
                    ModelYear = 2020,
                    GearType = GearType.Otomatik
                },
                new Car
                {
                    Id = 2,
                    Brand = "BMW",
                    Model = "320i",
                    Description = "BMW 320i Gri, sportif sürüş deneyimi",
                    ImageUrl = "/images/bmw320igri.jpg",
                    DailyPrice = 2700m,
                    IsAvailable = true,
                    Mileage = 72000,
                    FuelType = "Benzin",
                    Color = "Gri",
                    ModelYear = 2019,
                    GearType = GearType.Otomatik
                },
                new Car
                {
                    Id = 3,
                    Brand = "Fiat",
                    Model = "Egea",
                    Description = "Fiat Egea Gri, ekonomik ve aile dostu",
                    ImageUrl = "/images/fiategeagri.jpg",
                    DailyPrice = 1500m,
                    IsAvailable = true,
                    Mileage = 95000,
                    FuelType = "Dizel",
                    Color = "Gri",
                    ModelYear = 2021,
                    GearType = GearType.Manuel
                },
                new Car
                {
                    Id = 4,
                    Brand = "Mercedes",
                    Model = "C180",
                    Description = "Mercedes C180 Beyaz, premium sedan",
                    ImageUrl = "/images/mercedesc180beyaz.jpg",
                    DailyPrice = 3000m,
                    IsAvailable = true,
                    Mileage = 68000,
                    FuelType = "Benzin",
                    Color = "Beyaz",
                    ModelYear = 2020,
                    GearType = GearType.Otomatik
                },
                new Car
                {
                    Id = 5,
                    Brand = "Toyota",
                    Model = "Corolla",
                    Description = "Toyota Corolla Beyaz, sorunsuz ve dayanıklı",
                    ImageUrl = "/images/toyotacorollabeyaz.jpg",
                    DailyPrice = 1800m,
                    IsAvailable = true,
                    Mileage = 88000,
                    FuelType = "Benzin",
                    Color = "Beyaz",
                    ModelYear = 2022,
                    GearType = GearType.Otomatik
                }
            );
        }
    }
}
