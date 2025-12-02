namespace BillingSoftware.Models
{
    public class EstimateItem
    {
        public string ProductName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}