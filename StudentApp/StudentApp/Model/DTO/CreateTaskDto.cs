using StudentApp.Model.Enums;

namespace StudentApp.Model.DTO;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriorityDto Priority { get; set; } = TaskPriorityDto.Sredni;
}