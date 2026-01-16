using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class User
{
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "The {0} field is required.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Email Verified")]
    public bool IsEmailVerified { get; set; } = false;

    public string? Otp { get; set; }
    public DateTime? OtpExpiry { get; set; }

    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "The {0} field is required.")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    [System.Text.Json.Serialization.JsonIgnore]
    public string ConfirmPassword { get; set; } = string.Empty;
}
