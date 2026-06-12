using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudentApp.Controllers;
using StudentApp.Data;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;
using StudentApp.Model.Enums;

namespace BackendTest.Controllers
{
    public class TasksControllerTests
    {
        private static (ApplicationDbContext Context, SqliteConnection Connection) CreateSqliteInMemoryContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            return (context, connection);
        }

        private static void SetUser(ControllerBase controller, int userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "test"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateTask_SetsUserIdAndToDoStatus_ReturnsCreated()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u1", Email = "u1@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var controller = new TasksController(context);
                SetUser(controller, user.Id);

                var dto = new TaskDto
                {
                    Title = "New Task",
                    Description = "Desc",
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Priority = TaskPriorityDto.High
                };

                ActionResult<TaskDto> action = await controller.CreateTask(dto);

                var created = Assert.IsType<CreatedAtActionResult>(action.Result);
                var returned = Assert.IsType<TaskDto>(created.Value);
                Assert.Equal(dto.Title, returned.Title);
                Assert.Equal(dto.Priority, returned.Priority);

                var dbTask = context.UserTasks.SingleOrDefault(t => t.Id == returned.Id);
                Assert.NotNull(dbTask);
                Assert.Equal(user.Id, dbTask!.UserId);
                Assert.Equal(TaskStatusDto.ToDo, dbTask.Status);
            }
        }

        [Fact]
        public async Task GetTaskById_ReturnsNotFound_ForOtherUser()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user1 = new User { Username = "u1", Email = "u1@example.com" };
                var user2 = new User { Username = "u2", Email = "u2@example.com" };
                context.Users.AddRange(user1, user2);
                await context.SaveChangesAsync();

                var task = new UserTask
                {
                    UserId = user2.Id,
                    Title = "Private",
                    CreatedAt = DateTime.UtcNow
                };
                context.UserTasks.Add(task);
                await context.SaveChangesAsync();

                var controller = new TasksController(context);
                SetUser(controller, user1.Id);

                ActionResult<TaskDto> action = await controller.GetTaskById(task.Id);

                Assert.IsType<NotFoundResult>(action.Result);
            }
        }

        [Fact]
        public async Task UpdateTask_WhenExists_UpdatesFieldsAndSetsUpdatedAt()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u", Email = "u@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var task = new UserTask
                {
                    UserId = user.Id,
                    Title = "Old",
                    Description = "OldDesc",
                    Status = TaskStatusDto.ToDo,
                    Priority = TaskPriorityDto.Low,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };
                context.UserTasks.Add(task);
                await context.SaveChangesAsync();

                var controller = new TasksController(context);
                SetUser(controller, user.Id);

                var dto = new TaskDto
                {
                    Id = task.Id,
                    Title = "Updated",
                    Description = "NewDesc",
                    DueDate = DateTime.UtcNow.AddDays(2),
                    Priority = TaskPriorityDto.High,
                    Status = TaskStatusDto.InProgress
                };

                var result = await controller.UpdateTask(task.Id, dto);

                Assert.IsType<NoContentResult>(result);

                var dbTask = await context.UserTasks.FindAsync(task.Id);
                Assert.Equal("Updated", dbTask!.Title);
                Assert.Equal(TaskPriorityDto.High, dbTask.Priority);
                Assert.Equal(TaskStatusDto.InProgress, dbTask.Status);
                Assert.NotNull(dbTask.UpdatedAt);
                Assert.True(dbTask.UpdatedAt > dbTask.CreatedAt);
            }
        }

        [Fact]
        public async Task DeleteTask_RemovesTask_WhenOwner()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u", Email = "u@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var task = new UserTask
                {
                    UserId = user.Id,
                    Title = "ToDelete",
                    CreatedAt = DateTime.UtcNow
                };
                context.UserTasks.Add(task);
                await context.SaveChangesAsync();

                var controller = new TasksController(context);
                SetUser(controller, user.Id);

                var result = await controller.DeleteTask(task.Id);

                Assert.IsType<NoContentResult>(result);
                var dbTask = await context.UserTasks.FindAsync(task.Id);
                Assert.Null(dbTask);
            }
        }

        [Fact]
        public async Task GetUpcomingTasks_ReturnsUpcoming_NotDone_AndRespectsLimit()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u", Email = "u@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var now = DateTime.UtcNow;

                var t1 = new UserTask { UserId = user.Id, Title = "A", DueDate = now.AddDays(1), Status = TaskStatusDto.ToDo };
                var t2 = new UserTask { UserId = user.Id, Title = "B", DueDate = now.AddDays(2), Status = TaskStatusDto.InProgress };
                var t3 = new UserTask { UserId = user.Id, Title = "C", DueDate = now.AddDays(3), Status = TaskStatusDto.Done };
                var t4 = new UserTask { UserId = user.Id, Title = "D", DueDate = now.AddDays(-1), Status = TaskStatusDto.ToDo };
                var t5 = new UserTask { UserId = user.Id, Title = "E", DueDate = now.AddDays(4), Status = TaskStatusDto.ToDo };

                context.UserTasks.AddRange(t1, t2, t3, t4, t5);
                await context.SaveChangesAsync();

                var controller = new TasksController(context);
                SetUser(controller, user.Id);

                ActionResult<IEnumerable<TaskDto>> action = await controller.GetUpcomingTasks(limit: 2);

                var ok = Assert.IsType<OkObjectResult>(action.Result);
                var list = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(ok.Value);
                var arr = list.ToArray();

                Assert.Equal(2, arr.Length);
                Assert.Equal("A", arr[0].Title);
                Assert.Equal("B", arr[1].Title);
            }
        }
    }
}