using StudentApp.Model.Entities;
using StudentApp.Model.DTO;

namespace StudentApp.Model.Mappers
{
    static public class TasksMapper
    {
        public static TaskDto MapToDto(UserTask task)
        {
            return new TaskDto
            {
                Id = task.Id,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
