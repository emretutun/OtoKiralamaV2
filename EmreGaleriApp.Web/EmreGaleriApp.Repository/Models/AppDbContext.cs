using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EmreGaleriApp.Repository.Models.EntityConfigurations;
using EmreGaleriApp.Repository.Configurations;
using EmreGaleriApp.Repository.SeedData;

namespace EmreGaleriApp.Repository.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LicenseType> LicenseTypes { get; set; }
        public DbSet<AppUserLicense> AppUserLicenses { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Firm> Firms { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<CarReview> CarReviews { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<PersonelDetail> PersonelDetails { get; set; }
        public DbSet<CarLicenseType> CarLicenseTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Order>(entity =>
            {
                // ✅ Postgres column type = date
                entity.Property(x => x.StartDate).HasColumnType("date");
                entity.Property(x => x.EndDate).HasColumnType("date");
            });
            builder.Entity<PersonelDetail>(entity =>
            {
                entity.Property(x => x.StartDate).HasColumnType("date");
            });

            builder.ApplyConfiguration(new CarConfiguration());
            builder.ApplyConfiguration(new AppUserLicenseConfiguration());
            builder.ApplyConfiguration(new PersonelDetailConfiguration());
            builder.ApplyConfiguration(new CarLicenseTypeConfiguration());
            builder.ApplyConfiguration(new CarReviewConfiguration());
            builder.ApplyConfiguration(new OrderConfiguration());
            builder.ApplyConfiguration(new OrderItemConfiguration());
            builder.ApplyConfiguration(new StockItemConfiguration());
            builder.Entity<AppUser>()
                .Property(x => x.BirthDate)
                .HasColumnType("date");

            builder.SeedLicenseTypes();
            builder.SeedCars();
        }

    }
}
