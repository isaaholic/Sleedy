using System.ComponentModel.DataAnnotations;

namespace TestMVC.Models
{
    public class Admin
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Key]
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
