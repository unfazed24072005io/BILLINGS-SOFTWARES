using System;

namespace BillingSoftware.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Stock { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal MinStock { get; set; } = 10;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Product()
        {
            Unit = "PCS";
            CreatedDate = DateTime.Now;
            MinStock = 10;
        }
    }
}