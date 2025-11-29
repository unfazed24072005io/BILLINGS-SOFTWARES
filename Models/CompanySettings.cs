namespace BillingSoftware.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "My Company";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string GSTNumber { get; set; } = "";
        public string Currency { get; set; } = "₹";

        public CompanySettings()
        {
            CompanyName = "My Company";
            Currency = "₹";
        }
    }
}