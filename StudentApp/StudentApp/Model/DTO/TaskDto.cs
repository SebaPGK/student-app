using StudentApp.Model.Enums;

namespace StudentApp.Model.DTO
{
    public record TaskDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public TaskPriorityDto Priority { get; set; }

        public TaskStatusDto Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
