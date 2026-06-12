using StudentApp.Model.Entities;
using StudentApp.Model.Enums;
using StudentApp.Model.Mappers;

namespace BackendTest.Mappers
{
    public class TaskMapperTests
    {
        [Fact]
        public void TasksMapper_MapToDto_MapsAllFields()
        {
            var task = new UserTask
            {
                Id = 1,
                UserId = 42,
                Title = "Test task",
                Description = "Desc",
                DueDate = new DateTime(2026, 1, 1),
                Priority = TaskPriorityDto.High,
                Status = TaskStatusDto.InProgress,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 2, 1)
            };

            var dto = TasksMapper.MapToDto(task);

            Assert.Equal(task.Id, dto.Id);
            Assert.Equal(task.UserId, dto.UserId);
            Assert.Equal(task.Title, dto.Title);
            Assert.Equal(task.Description, dto.Description);
            Assert.Equal(task.DueDate, dto.DueDate);
            Assert.Equal(task.Priority, dto.Priority);
            Assert.Equal(task.Status, dto.Status);
            Assert.Equal(task.CreatedAt, dto.CreatedAt);
            Assert.Equal(task.UpdatedAt, dto.UpdatedAt);
        }
    }
}