using System;

namespace BillingSoftware.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public decimal Stock { get; set; }
        public string Unit { get; set; }
        public decimal MinStock { get; set; }
        public DateTime CreatedDate { get; set; }

        public Product()
        {
            Unit = "PCS";
            CreatedDate = DateTime.Now;
            MinStock = 10; // Default minimum stock level
        }
    }
}