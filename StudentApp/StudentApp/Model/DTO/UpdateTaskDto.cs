using StudentApp.Model.Enums;

namespace StudentApp.Model.DTO;

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriorityDto Priority { get; set; } = TaskPriorityDto.Sredni;

    public TaskStatusDto Status { get; set; } = TaskStatusDto.DoZrobienia;
}