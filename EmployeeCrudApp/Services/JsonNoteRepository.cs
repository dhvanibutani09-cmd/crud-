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

    public IEnumerable<Note> GetAll(string userId)
    {
        if (!File.Exists(_filePath)) return Enumerable.Empty<Note>();
        var json = File.ReadAllText(_filePath);
        var allNotes = JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
        return allNotes.Where(n => n.UserId == userId);
    }

    public Note? GetById(int id, string userId)
    {
        return GetAll(userId).FirstOrDefault(n => n.Id == id);
    }

    public void Add(Note note)
    {
        var allNotes = LoadAll();
        note.Id = allNotes.Any() ? allNotes.Max(n => n.Id) + 1 : 1;
        note.CreatedAt = DateTime.Now;
        allNotes.Add(note);
        SaveAll(allNotes);
    }

    public void Update(Note note, string userId)
    {
        var allNotes = LoadAll();
        var index = allNotes.FindIndex(n => n.Id == note.Id && n.UserId == userId);
        if (index != -1)
        {
            allNotes[index].Text = note.Text;
            SaveAll(allNotes);
        }
    }

    public void Delete(int id, string userId)
    {
        var allNotes = LoadAll();
        var note = allNotes.FirstOrDefault(n => n.Id == id && n.UserId == userId);
        if (note != null)
        {
            allNotes.Remove(note);
            SaveAll(allNotes);
        }
    }

    private List<Note> LoadAll()
    {
        if (!File.Exists(_filePath)) return new List<Note>();
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
    }

    private void SaveAll(List<Note> notes)
    {
        var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
