using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeeCrudApp.Models;
using EmployeeCrudApp.Services;

namespace EmployeeCrudApp.Controllers;

public class HomeController : Controller
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;

    public HomeController(IEmployeeRepository employeeRepository, IUserRepository userRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
    }

    public IActionResult Index()
    {
        var model = new DashboardViewModel
        {
            TotalEmployees = _employeeRepository.GetAll().Count(),
            TotalUsers = _userRepository.GetAll().Count(),
            RecentEmployees = _employeeRepository.GetAll().OrderByDescending(e => e.Id).Take(5).ToList()
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
