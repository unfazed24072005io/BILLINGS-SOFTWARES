namespace BillingSoftware.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Number { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Party { get; set; } = "";
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string ReferenceVoucher { get; set; } = ""; // ADDED: Reference to other voucher
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<VoucherItem> Items { get; set; } = new List<VoucherItem>(); // ADDED: Item list

        public Voucher()
        {
            Date = DateTime.Now;
            CreatedDate = DateTime.Now;
            Status = "Active";
            Items = new List<VoucherItem>();
        }
    }

    // ADD THIS NEW CLASS for voucher items
    public class VoucherItem
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}