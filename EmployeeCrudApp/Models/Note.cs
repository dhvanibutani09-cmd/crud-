using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudApp.Models;

public class Note
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string? UserId { get; set; }
}
