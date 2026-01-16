using EmployeeCrudApp.Models;
using System.Text.Json;

namespace EmployeeCrudApp.Services;

public class JsonNoteRepository : INoteRepository
{
    private readonly string _filePath;

    public JsonNoteRepository(IWebHostEnvironment webHostEnvironment)
    {
        _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "notes.json");
    }

    public IEnumerable<Note> GetAll()
    {
        if (!File.Exists(_filePath)) return Enumerable.Empty<Note>();
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
    }

    public Note? GetById(int id)
    {
        return GetAll().FirstOrDefault(n => n.Id == id);
    }

    public void Add(Note note)
    {
        var notes = GetAll().ToList();
        note.Id = notes.Any() ? notes.Max(n => n.Id) + 1 : 1;
        note.CreatedAt = DateTime.Now;
        notes.Add(note);
        SaveAll(notes);
    }

    public void Update(Note note)
    {
        var notes = GetAll().ToList();
        var index = notes.FindIndex(n => n.Id == note.Id);
        if (index != -1)
        {
            notes[index].Text = note.Text;
            // Keep original CreatedAt
            SaveAll(notes);
        }
    }

    public void Delete(int id)
    {
        var notes = GetAll().ToList();
        var note = notes.FirstOrDefault(n => n.Id == id);
        if (note != null)
        {
            notes.Remove(note);
            SaveAll(notes);
        }
    }

    private void SaveAll(List<Note> notes)
    {
        var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
