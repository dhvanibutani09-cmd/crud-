using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models
{
    public class VerifyOtpViewModel
    {
        [Display(Name = "OTP Code")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string Otp { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
