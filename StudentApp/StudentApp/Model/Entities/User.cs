namespace StudentApp.Model.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}