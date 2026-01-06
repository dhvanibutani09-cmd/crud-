using EmployeeCrudApp.Models;
using System.Text.Json;

namespace EmployeeCrudApp.Services;

public class JsonEmployeeRepository : IEmployeeRepository
{
    private readonly string _filePath;

    public JsonEmployeeRepository(IWebHostEnvironment webHostEnvironment)
    {
        _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "employees.json");
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private List<Employee> ReadData()
    {
        if (!File.Exists(_filePath)) return new List<Employee>();
        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return new List<Employee>();
        return JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();
    }

    private void WriteData(List<Employee> employees)
    {
        var json = JsonSerializer.Serialize(employees, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public IEnumerable<Employee> GetAll() => ReadData();

    public Employee? GetById(int id) => ReadData().FirstOrDefault(e => e.Id == id);

    public void Add(Employee employee)
    {
        var employees = ReadData();
        employee.Id = employees.Count > 0 ? employees.Max(e => e.Id) + 1 : 1;
        employees.Add(employee);
        WriteData(employees);
    }

    public void Update(Employee employee)
    {
        var employees = ReadData();
        var existing = employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existing != null)
        {
            existing.Name = employee.Name;
            
            existing.Age = employee.Age;
            existing.Email = employee.Email;
            existing.Password = employee.Password;
            WriteData(employees);
        }
    }

    public void Delete(int id)
    {
        var employees = ReadData();
        var employee = employees.FirstOrDefault(e => e.Id == id);
        if (employee != null)
        {
            employees.Remove(employee);
            WriteData(employees);
        }
    }
}
