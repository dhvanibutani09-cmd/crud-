using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models
{
    public class SignupViewModel
    {
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

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
