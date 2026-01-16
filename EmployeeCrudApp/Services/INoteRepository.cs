using EmployeeCrudApp.Models;

namespace EmployeeCrudApp.Services;

public interface INoteRepository
{
    IEnumerable<Note> GetAll();
    Note? GetById(int id);
    void Add(Note note);
    void Update(Note note);
    void Delete(int id);
}
