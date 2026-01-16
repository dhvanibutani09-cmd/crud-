using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class ForgotPasswordViewModel
{
    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
    public string Email { get; set; } = string.Empty;
}
