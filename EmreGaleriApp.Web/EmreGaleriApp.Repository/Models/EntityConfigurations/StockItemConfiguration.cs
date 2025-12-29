using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmreGaleriApp.Repository.Models.EntityConfigurations
{
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductName)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(x => x.PurchasePrice)
                   .HasColumnType("numeric(18,2)");

            builder.Property(x => x.SalePrice)
                   .HasColumnType("numeric(18,2)");

            // 🔴 KRİTİK: ilişkiyi TEK YERDEN ve NET tanımlıyoruz
            builder.HasOne(x => x.Firm)
                   .WithMany(f => f.StockItems) // 🔴 KRİTİK SATIR
                   .HasForeignKey(x => x.FirmId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
