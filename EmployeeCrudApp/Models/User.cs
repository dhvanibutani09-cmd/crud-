using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public string? Otp { get; set; }
    public DateTime? OtpExpiry { get; set; }
}
