using EmployeeCrudApp.Models;

namespace EmployeeCrudApp.Services;

public interface INoteRepository
{
    IEnumerable<Note> GetAll(string userId);
    Note? GetById(int id, string userId);
    void Add(Note note);
    void Update(Note note, string userId);
    void Delete(int id, string userId);
}
