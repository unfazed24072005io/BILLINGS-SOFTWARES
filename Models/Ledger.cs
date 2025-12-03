using System;
using System.Collections.Generic;

namespace BillingSoftware.Models
{
    public class Ledger
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Type { get; set; } = ""; // Customer, Supplier, Bank, Cash, Expense, Income
        public string ContactPerson { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string GSTIN { get; set; } = "";
        public decimal OpeningBalance { get; set; }
        public string BalanceType { get; set; } = "Dr"; // Dr or Cr
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "";
    }

    public class LedgerTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string VoucherType { get; set; } = ""; // Receipt, Payment, Journal, Sales, Purchase
        public string VoucherNumber { get; set; } = "";
        public string LedgerName { get; set; } = "";
        public string Particulars { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string Reference { get; set; } = "";
        public string Narration { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class ReceiptVoucher
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string ReceivedFrom { get; set; } = "";
        public string AmountInWords { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string ChequeNo { get; set; } = "";
        public DateTime? ChequeDate { get; set; }
        public string BankName { get; set; } = "";
        public string Narration { get; set; } = "";
        public bool IsPosted { get; set; } = false;
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public List<ReceiptDetail> Details { get; set; } = new List<ReceiptDetail>();
    }

    public class ReceiptDetail
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; } = "";
        public string LedgerName { get; set; } = "";
        public string Particulars { get; set; } = "";
        public decimal Amount { get; set; }
        public string VoucherReference { get; set; } = ""; // Reference to Sales/Purchase voucher
    }

    public class PaymentVoucher
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string PaidTo { get; set; } = "";
        public string AmountInWords { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string ChequeNo { get; set; } = "";
        public DateTime? ChequeDate { get; set; }
        public string BankName { get; set; } = "";
        public string Narration { get; set; } = "";
        public bool IsPosted { get; set; } = false;
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public List<PaymentDetail> Details { get; set; } = new List<PaymentDetail>();
    }

    public class PaymentDetail
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = "";
        public string LedgerName { get; set; } = "";
        public string Particulars { get; set; } = "";
        public decimal Amount { get; set; }
        public string VoucherReference { get; set; } = ""; // Reference to Sales/Purchase voucher
    }
}