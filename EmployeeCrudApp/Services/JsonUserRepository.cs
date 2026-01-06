using EmployeeCrudApp.Models;
using System.Text.Json;

namespace EmployeeCrudApp.Services;

public class JsonUserRepository : IUserRepository
{
    private readonly string _filePath;

    public JsonUserRepository(IWebHostEnvironment webHostEnvironment)
    {
        _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "user.json");
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private List<User> ReadData()
    {
        if (!File.Exists(_filePath)) return new List<User>();
        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return new List<User>();
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }

    private void WriteData(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public IEnumerable<User> GetAll() => ReadData();

    public User? GetByEmail(string email) => ReadData().FirstOrDefault(u => u.Email == email);

    public User? GetById(int id) => ReadData().FirstOrDefault(u => u.Id == id);

    public void Add(User user)
    {
        var users = ReadData();
        user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        WriteData(users);
    }

    public void Update(User user)
    {
        var users = ReadData();
        var index = users.FindIndex(u => u.Id == user.Id);
        if (index != -1)
        {
            users[index] = user;
            WriteData(users);
        }
    }

    public void Delete(int id)
    {
        var users = ReadData();
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            users.Remove(user);
            WriteData(users);
        }
    }
}
