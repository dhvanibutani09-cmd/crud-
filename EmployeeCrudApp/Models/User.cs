using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public string? Otp { get; set; }
    public DateTime? OtpExpiry { get; set; }

    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    [System.Text.Json.Serialization.JsonIgnore]
    public string ConfirmPassword { get; set; } = string.Empty;
}
