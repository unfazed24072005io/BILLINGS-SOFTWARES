using System;

namespace BillingSoftware.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public User()
        {
            Role = "User";
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }
}