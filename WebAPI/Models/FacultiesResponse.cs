using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models;

public class FacultiesResponse
{
    [Key]
    public string FacultyName { get; set; }
    public string ImageUrl { get; set; }
}
