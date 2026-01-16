using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;

    [Display(Name = "New Password")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm New Password")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
