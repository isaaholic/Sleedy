using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers.Dtos
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
