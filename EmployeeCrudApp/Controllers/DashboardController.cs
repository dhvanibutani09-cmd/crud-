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
        private readonly IHabitRepository _habitRepository;

        public DashboardController(INoteRepository noteRepository, IHabitRepository habitRepository)
        {
            _noteRepository = noteRepository;
            _habitRepository = habitRepository;
        }

        public IActionResult Index()
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var viewModel = new DashboardViewModel
            {
                Notes = _noteRepository.GetAll(userId).OrderByDescending(n => n.CreatedAt).ToList(),
                Habits = _habitRepository.GetAll(userId).OrderByDescending(h => h.CreatedAt).ToList()
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

        [HttpPost]
        public IActionResult AddHabit(string name, string description, string frequency, string customDays, DateTime? startDate)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var userId = User.Identity?.Name ?? string.Empty;
                var days = new List<DayOfWeek>();
                if (frequency == "Custom" && !string.IsNullOrEmpty(customDays))
                {
                    try
                    {
                        days = customDays.Split(',')
                                         .Select(d => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), d))
                                         .ToList();
                    }
                    catch { /* Ignore invalid days */ }
                }

                _habitRepository.Add(new Habit 
                { 
                    Name = name, 
                    Description = description, 
                    UserId = userId,
                    Frequency = frequency,
                    CustomDays = days,
                    StartDate = startDate ?? DateTime.Today
                });
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Habit name cannot be empty." });
        }

        [HttpPost]
        public IActionResult ToggleHabit(int id)
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var habit = _habitRepository.GetById(id);
            
            if (habit != null && habit.UserId == userId)
            {
                var today = DateTime.Today;
                if (habit.CompletedDates.Any(d => d.Date == today))
                {
                    habit.CompletedDates.RemoveAll(d => d.Date == today);
                }
                else
                {
                    habit.CompletedDates.Add(today);
                }
                _habitRepository.Update(habit);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Habit not found." });
        }

        [HttpPost]
        public IActionResult DeleteHabit(int id)
        {
             var userId = User.Identity?.Name ?? string.Empty;
             // Ideally check ownership before delete but current repo structure might assume id is enough or handle it.
             // For safety let's assume we should check, but simple delete for now based on logic.
             // Actually repo.Delete(id, userId) would be safer but I only defined Delete(id).
             // Let's stick to simple delete for this MVP or improved if I changed repo.
             // I'll check existence first.
             var habit = _habitRepository.GetById(id);
             if (habit != null && habit.UserId == userId)
             {
                 _habitRepository.Delete(id);
                 return Json(new { success = true });
             }
             return Json(new { success = false, message = "Habit not found." });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
