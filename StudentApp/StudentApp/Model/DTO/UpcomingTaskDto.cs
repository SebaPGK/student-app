using StudentApp.Model.Enums;

namespace StudentApp.Model.DTO;

public class UpcomingTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public TaskPriorityDto Priority { get; set; }

    public TaskStatusDto Status { get; set; }
}