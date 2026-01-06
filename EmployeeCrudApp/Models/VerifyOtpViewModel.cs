using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "OTP is required.")]
        [Display(Name = "One Time Password (OTP)")]
        public string Otp { get; set; }
    }
}
