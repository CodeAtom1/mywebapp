using System;

namespace MyWebApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}