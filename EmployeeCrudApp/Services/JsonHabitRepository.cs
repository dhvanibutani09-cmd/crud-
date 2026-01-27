using EmployeeCrudApp.Models;
using System.Text.Json;

namespace EmployeeCrudApp.Services
{
    public class JsonHabitRepository : IHabitRepository
    {
        private readonly string _filePath = "habits.json";

        public JsonHabitRepository()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private List<Habit> ReadFromFile()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Habit>>(json) ?? new List<Habit>();
        }

        private void WriteToFile(List<Habit> habits)
        {
            var json = JsonSerializer.Serialize(habits, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<Habit> GetAll(string userId)
        {
            return ReadFromFile().Where(h => h.UserId == userId).ToList();
        }

        public Habit? GetById(int id)
        {
            return ReadFromFile().FirstOrDefault(h => h.Id == id);
        }

        public void Add(Habit habit)
        {
            var habits = ReadFromFile();
            habit.Id = habits.Any() ? habits.Max(h => h.Id) + 1 : 1;
            habit.CreatedAt = DateTime.Now;
            habits.Add(habit);
            WriteToFile(habits);
        }

        public void Update(Habit habit)
        {
            var habits = ReadFromFile();
            var existing = habits.FirstOrDefault(h => h.Id == habit.Id);
            if (existing != null)
            {
                existing.Name = habit.Name;
                existing.Description = habit.Description;
                existing.CompletedDates = habit.CompletedDates;
                // UserId and CreatedAt should generally not change
                WriteToFile(habits);
            }
        }

        public void Delete(int id)
        {
            var habits = ReadFromFile();
            var habitToRemove = habits.FirstOrDefault(h => h.Id == id);
            if (habitToRemove != null)
            {
                habits.Remove(habitToRemove);
                WriteToFile(habits);
            }
        }
    }
}
