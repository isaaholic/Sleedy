using System.ComponentModel.DataAnnotations;

namespace TestMVC.Models
{
    public class AdminDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Key]
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
