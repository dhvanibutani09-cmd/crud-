using Microsoft.AspNetCore.Mvc;
using EmployeeCrudApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EmployeeCrudApp.Services;

namespace EmployeeCrudApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Localization.IStringLocalizer<AccountController> _localizer;

        public AccountController(IEmployeeRepository employeeRepository, IUserRepository userRepository, IEmailService emailService, Microsoft.Extensions.Localization.IStringLocalizer<AccountController> localizer)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _localizer = localizer;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetByEmail(model.Email);

                if (user != null && user.Password == model.Password)
                {
                    // Existing sign-in logic
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.GivenName, user.Name), // Added for display
                        new Claim("Email", user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Dashboard");
                }
                
                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, _localizer["Incorrect password. You can reset it using the link below."]);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["Invalid email or password."]);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _userRepository.GetByEmail(model.Email);
                if (existingUser != null && existingUser.IsEmailVerified)
                {
                    ModelState.AddModelError("Email", _localizer["Email already exists."]);
                    return View(model);
                }

                var otp = new Random().Next(100000, 999999).ToString();
                
                if (existingUser != null)
                {
                    // Update existing unverified user
                    existingUser.Name = model.Name;
                    existingUser.Password = model.Password;
                    existingUser.Otp = otp;
                    existingUser.OtpExpiry = DateTime.Now.AddMinutes(5);
                    _userRepository.Update(existingUser);
                }
                else
                {
                    // Create new user
                    var user = new User
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Password = model.Password,
                        IsEmailVerified = false,
                        Otp = otp,
                        OtpExpiry = DateTime.Now.AddMinutes(5)
                    };
                    _userRepository.Add(user);
                }

                await _emailService.SendEmailAsync(model.Email, "Verify your email", $"Your OTP code is: {otp}");
                HttpContext.Session.SetString("VerifyEmail", model.Email);
                HttpContext.Session.SetString("VerifyName", model.Name); // Added for display

                return RedirectToAction("VerifyOtp");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp()
        {
            var email = HttpContext.Session.GetString("VerifyEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            ViewBag.Name = HttpContext.Session.GetString("VerifyName");
            
            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = model.Email;
                // session check removed/secondary because we have email in model

                var user = _userRepository.GetByEmail(email);

                if (user != null && user.Otp == model.Otp && user.OtpExpiry > DateTime.Now)
                {
                    user.IsEmailVerified = true;
                    user.Otp = null;
                    user.OtpExpiry = null;
                    _userRepository.Update(user);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.GivenName, user.Name), // Added for display
                        new Claim("Email", user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Sync with Employee Repository
                    var employees = _employeeRepository.GetAll();
                    if (!employees.Any(e => e.Email == user.Email))
                    {
                        var newEmployee = new Employee
                        {
                            Name = user.Name,
                            Email = user.Email,
                            Password = user.Password, // Sync password for consistency
                            Age = 0 // Default age, can be updated later
                        };
                        _employeeRepository.Add(newEmployee);
                    }

                    HttpContext.Session.Remove("VerifyEmail");

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, _localizer["Invalid or expired OTP."]);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetByEmail(model.Email);
                if (user != null)
                {
                    var token = Guid.NewGuid().ToString();
                    user.Otp = token;
                    user.OtpExpiry = DateTime.Now.AddMinutes(30);
                    _userRepository.Update(user);

                    var resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);
                    
                    await _emailService.SendEmailAsync(user.Email, "Reset your password", 
                        $"<p>Click the link below to reset your password:</p><p><a href='{resetLink}'>{resetLink}</a></p>");
                    
                    TempData["Success"] = _localizer["A reset link has been sent to your email."];
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, _localizer["Email not found."]);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel { Email = email, Otp = token });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetByEmail(model.Email);
                if (user != null && user.Otp == model.Otp && user.OtpExpiry > DateTime.Now)
                {
                    user.Password = model.NewPassword;
                    user.Otp = null;
                    user.OtpExpiry = null;
                    _userRepository.Update(user);
                    TempData["Success"] = _localizer["Password reset successfully. Please login."];
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, _localizer["Invalid or expired OTP."]);
            }
            return View(model);
        }
    }
}
