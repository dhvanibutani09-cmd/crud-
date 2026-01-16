using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeCrudApp.Services;
using EmployeeCrudApp.Models;

namespace EmployeeCrudApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Localization.IStringLocalizer<UserController> _localizer;

        public UserController(IUserRepository userRepository, IEmailService emailService, Microsoft.Extensions.Localization.IStringLocalizer<UserController> localizer)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _userRepository.GetByEmail(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", _localizer["Email already exists."]);
                    return View(user);
                }

                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                
                // Store incomplete user in TempData
                user.IsEmailVerified = false;
                user.Otp = otp;
                user.OtpExpiry = DateTime.Now.AddMinutes(5);

                var userJson = System.Text.Json.JsonSerializer.Serialize(user);
                TempData["PendingUser"] = userJson;

                // Send Email
                await _emailService.SendEmailAsync(user.Email, "Complete User Creation", $"Your verification code is: {otp}");
                
                return RedirectToAction(nameof(VerifyAddUserOtp));
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult VerifyAddUserOtp()
        {
            if (!TempData.ContainsKey("PendingUser"))
            {
                return RedirectToAction(nameof(Create));
            }
            TempData.Keep("PendingUser");
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAddUserOtp(string otp)
        {
            if (TempData.ContainsKey("PendingUser"))
            {
                var userJson = TempData["PendingUser"] as string;
                if (!string.IsNullOrEmpty(userJson))
                {
                    var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
                    
                    if (user != null && user.Otp == otp && user.OtpExpiry > DateTime.Now)
                    {
                        // OTP Verified
                        user.IsEmailVerified = true;
                        user.Otp = null;
                        user.OtpExpiry = null;
                        
                        _userRepository.Add(user);
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData.Keep("PendingUser"); // Keep data for retry
            }
            
            ModelState.AddModelError(string.Empty, _localizer["Invalid or expired OTP provided."]);
            return View();
        }

        public IActionResult Edit(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                _userRepository.Update(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public IActionResult Delete(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _userRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
