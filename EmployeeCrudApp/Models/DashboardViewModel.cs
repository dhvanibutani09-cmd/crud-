namespace EmployeeCrudApp.Models;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int TotalUsers { get; set; }
    public List<Employee> RecentEmployees { get; set; } = new List<Employee>();
}
