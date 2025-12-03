namespace BillingSoftware.Models
{
    public class Voucher
    {
        // Core Voucher Information
        public int Id { get; set; }
        public string Type { get; set; } = ""; // Sales, Purchase, Receipt, Payment, Journal, Estimate
        public string Number { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        
        // Party Information
        public string Party { get; set; } = ""; // Customer/Supplier Name
        public string PartyCode { get; set; } = ""; // Customer/Supplier Code
        public string PartyGSTIN { get; set; } = "";
        
        // Financial Information
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        
        // Description & Status
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Active"; // Active, Cancelled, Draft, Posted
        public string ReferenceVoucher { get; set; } = ""; // For Receipt against Sales, Payment against Purchase
        
        // Payment Information (for Receipt/Payment vouchers)
        public string PaymentMode { get; set; } = "Cash"; // Cash, Cheque, Bank Transfer
        public string ChequeNo { get; set; } = "";
        public DateTime? ChequeDate { get; set; }
        public string BankName { get; set; } = "";
        
        // Audit Information
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = "";
        public DateTime? ModifiedDate { get; set; }
        
        // Items
        public List<VoucherItem> Items { get; set; } = new List<VoucherItem>();

        public Voucher()
        {
            Date = DateTime.Now;
            CreatedDate = DateTime.Now;
            Status = "Active";
            Items = new List<VoucherItem>();
            CreatedBy = Program.CurrentUser ?? "System";
        }
    }

    public class VoucherItem
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;
        public decimal NetAmount => TotalAmount * (1 - DiscountRate / 100) * (1 + TaxRate / 100);
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}