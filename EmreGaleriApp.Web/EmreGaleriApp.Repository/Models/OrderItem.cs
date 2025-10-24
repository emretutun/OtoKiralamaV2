﻿namespace EmreGaleriApp.Repository.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public decimal DailyPrice { get; set; }
    }
}
