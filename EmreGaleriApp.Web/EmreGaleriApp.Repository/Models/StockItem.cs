using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Repository.Models
{
    public class StockItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = null!;

        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }

        public int Quantity { get; set; }

        // FK (zorunluluk bununla sağlanır)
        [Required]
        public int FirmId { get; set; }

        // Navigation: Create/Edit formundan gelmez, query'de Include ile dolar
        public Firm? Firm { get; set; }
    }
}
