using System;

namespace BillingSoftware.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string UserRole { get; set; } = "";
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = ""; // Voucher, Product, User, etc.
        public string EntityId { get; set; } = ""; // Voucher number, Product code, etc.
        public string Details { get; set; } = "";
        public string OldValues { get; set; } = "";
        public string NewValues { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Module { get; set; } = ""; // Sales, Purchase, Reports, etc.
    }
}