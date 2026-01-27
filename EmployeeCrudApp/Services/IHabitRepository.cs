using EmployeeCrudApp.Models;
using System.Collections.Generic;

namespace EmployeeCrudApp.Services
{
    public interface IHabitRepository
    {
        List<Habit> GetAll(string userId);
        Habit? GetById(int id);
        void Add(Habit habit);
        void Update(Habit habit);
        void Delete(int id);
    }
}
