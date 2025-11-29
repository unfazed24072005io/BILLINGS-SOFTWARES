using System;

namespace BillingSoftware.Models
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Type { get; set; }  // Sales, Receipt, Payment, Journal, Estimate
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Party { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }  // Active, Cancelled
        public DateTime CreatedDate { get; set; }

        public Voucher()
        {
            Date = DateTime.Now;
            CreatedDate = DateTime.Now;
            Status = "Active";
        }
    }
}