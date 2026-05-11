using StudentApp.Model.Enums;

namespace StudentApp.Model.DTO;

public class TaskFilterDto
{
    public TaskStatusDto? Status { get; set; }

    public TaskPriorityDto? Priority { get; set; }

    public DateTime? DueDateFrom { get; set; }

    public DateTime? DueDateTo { get; set; }

    public string? SearchPhrase { get; set; }
}