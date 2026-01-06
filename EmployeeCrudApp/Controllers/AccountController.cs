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

        public AccountController(IEmployeeRepository employeeRepository, IUserRepository userRepository, IEmailService emailService)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
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
                    ModelState.AddModelError(string.Empty, "Incorrect password. You can reset it using the link below.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
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
                if (_userRepository.GetByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var otp = new Random().Next(100000, 999999).ToString();
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

                await _emailService.SendEmailAsync(user.Email, "Verify your email", $"Your OTP code is: {otp}");
                HttpContext.Session.SetString("VerifyEmail", user.Email);

                return RedirectToAction("VerifyOtp");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("VerifyEmail")))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = HttpContext.Session.GetString("VerifyEmail");
                if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

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
                        new Claim("Email", user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    HttpContext.Session.Remove("VerifyEmail");

                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid or expired OTP.");
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
                    
                    TempData["Success"] = "A reset link has been sent to your email.";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, "Email not found.");
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
                    TempData["Success"] = "Password reset successfully. Please login.";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, "Invalid or expired OTP.");
            }
            return View(model);
        }
    }
}
