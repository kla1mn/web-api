using System.ComponentModel.DataAnnotations;

namespace WebApi.MinimalApi.Models;

public class UserEntityToDto
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }
}
