using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EmployeeCrudApp.Models;
using EmployeeCrudApp.Services;

namespace EmployeeCrudApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly INoteRepository _noteRepository;

        public DashboardController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public IActionResult Index()
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var viewModel = new DashboardViewModel
            {
                Notes = _noteRepository.GetAll(userId).OrderByDescending(n => n.CreatedAt).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddNote(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var userId = User.Identity?.Name ?? string.Empty;
                _noteRepository.Add(new Note { Text = text, UserId = userId });
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Note text cannot be empty." });
        }

        [HttpPost]
        public IActionResult EditNote(int id, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var userId = User.Identity?.Name ?? string.Empty;
                var note = new Note { Id = id, Text = text };
                _noteRepository.Update(note, userId);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Note text cannot be empty." });
        }

        [HttpPost]
        public IActionResult DeleteNote(int id)
        {
            var userId = User.Identity?.Name ?? string.Empty;
            _noteRepository.Delete(id, userId);
            return Json(new { success = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
