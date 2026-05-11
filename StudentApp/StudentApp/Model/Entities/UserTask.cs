using StudentApp.Model.Enums;

namespace StudentApp.Model.Entities;

public class UserTask
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriorityDto Priority { get; set; } = TaskPriorityDto.Sredni;

    public TaskStatusDto Status { get; set; } = TaskStatusDto.DoZrobienia;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}