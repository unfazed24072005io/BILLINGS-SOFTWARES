namespace BillingSoftware.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User"; // Admin, User, Manager
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}