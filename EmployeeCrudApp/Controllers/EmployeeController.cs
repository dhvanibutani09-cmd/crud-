using EmployeeCrudApp.Models;
using EmployeeCrudApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudApp.Controllers;

public class EmployeeController : Controller
{
    private readonly IEmployeeRepository _repository;

    public EmployeeController(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var employees = _repository.GetAll();
        return View(employees);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _repository.Add(employee);
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    public IActionResult Edit(int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost]
    public IActionResult Edit(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _repository.Update(employee);
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    public IActionResult Delete(int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        _repository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null) return NotFound();
        return View(employee);
    }
}
