using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
