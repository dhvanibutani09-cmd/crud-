using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class Employee
{
    public int Id { get; set; }

    [Display(Name = "Name")]
    [Required(ErrorMessage = "The {0} field is required.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Age")]
    [Required(ErrorMessage = "The {0} field is required.")]
    public int Age { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
