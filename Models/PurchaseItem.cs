namespace BillingSoftware.Models
{
    public class PurchaseItem
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal Rate { get; set; }
        public decimal Amount => Quantity * Rate;
    }
}