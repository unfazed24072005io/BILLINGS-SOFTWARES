using System;

namespace BillingSoftware.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }  // Admin, User
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public User()
        {
            Role = "User";
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }
}