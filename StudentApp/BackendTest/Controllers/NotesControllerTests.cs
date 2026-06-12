using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudentApp.Controllers;
using StudentApp.Data;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;

namespace BackendTest.Controllers
{
    public class NotesControllerTests
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
        public async Task CreateNote_AssignsUserIdAndReturnsCreated()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u1", Email = "u1@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var controller = new NotesController(context);
                SetUser(controller, user.Id);

                var dto = new NoteDto
                {
                    Title = "Note title",
                    Content = "Note content"
                };

                Microsoft.AspNetCore.Mvc.ActionResult<NoteDto> action = await controller.CreateNote(dto);

                var created = Assert.IsType<CreatedAtActionResult>(action.Result);
                var returned = Assert.IsType<NoteDto>(created.Value);
                Assert.Equal(dto.Title, returned.Title);
                Assert.Equal(dto.Content, returned.Content);

                var dbNote = context.Notes.SingleOrDefault(n => n.Id == returned.Id);
                Assert.NotNull(dbNote);
                Assert.Equal(user.Id, dbNote!.UserId);
            }
        }

        [Fact]
        public async Task GetNoteById_ReturnsNotFound_ForOtherUser()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user1 = new User { Username = "u1", Email = "u1@example.com" };
                var user2 = new User { Username = "u2", Email = "u2@example.com" };
                context.Users.AddRange(user1, user2);
                await context.SaveChangesAsync();

                var note = new Note
                {
                    UserId = user2.Id,
                    Title = "Secret",
                    Content = "Secret content",
                    CreatedAt = DateTime.UtcNow
                };
                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var controller = new NotesController(context);
                SetUser(controller, user1.Id);

                Microsoft.AspNetCore.Mvc.ActionResult<NoteDto> action = await controller.GetNoteById(note.Id);

                Assert.IsType<NotFoundResult>(action.Result);
            }
        }

        [Fact]
        public async Task UpdateNote_WhenOwner_UpdatesFieldsAndSetsUpdatedAt()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u", Email = "u@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var note = new Note
                {
                    UserId = user.Id,
                    Title = "OldTitle",
                    Content = "OldContent",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                };
                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var controller = new NotesController(context);
                SetUser(controller, user.Id);

                var dto = new NoteDto
                {
                    Id = note.Id,
                    Title = "NewTitle",
                    Content = "NewContent"
                };

                var result = await controller.UpdateNote(note.Id, dto);

                Assert.IsType<NoContentResult>(result);

                var dbNote = await context.Notes.FindAsync(note.Id);
                Assert.Equal("NewTitle", dbNote!.Title);
                Assert.Equal("NewContent", dbNote.Content);
                Assert.NotNull(dbNote.UpdatedAt);
                Assert.True(dbNote.UpdatedAt > dbNote.CreatedAt);
            }
        }

        [Fact]
        public async Task DeleteNote_RemovesNote_WhenOwner()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user = new User { Username = "u", Email = "u@example.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var note = new Note
                {
                    UserId = user.Id,
                    Title = "ToDelete",
                    Content = "Content",
                    CreatedAt = DateTime.UtcNow
                };
                context.Notes.Add(note);
                await context.SaveChangesAsync();

                var controller = new NotesController(context);
                SetUser(controller, user.Id);

                var result = await controller.DeleteNote(note.Id);

                Assert.IsType<NoContentResult>(result);
                var dbNote = await context.Notes.FindAsync(note.Id);
                Assert.Null(dbNote);
            }
        }

        [Fact]
        public async Task GetNotes_ReturnsAllNotes_OrderedByCreatedAtDesc()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var user1 = new User { Username = "u1", Email = "u1@example.com" };
                var user2 = new User { Username = "u2", Email = "u2@example.com" };
                context.Users.AddRange(user1, user2);
                await context.SaveChangesAsync();

                var now = DateTime.UtcNow;
                var n1 = new Note { UserId = user1.Id, Title = "First", Content = "A", CreatedAt = now.AddDays(-2) };
                var n2 = new Note { UserId = user2.Id, Title = "Second", Content = "B", CreatedAt = now.AddDays(-1) };
                var n3 = new Note { UserId = user1.Id, Title = "Third", Content = "C", CreatedAt = now };
                context.Notes.AddRange(n1, n2, n3);
                await context.SaveChangesAsync();

                var controller = new NotesController(context);

                Microsoft.AspNetCore.Mvc.ActionResult<IEnumerable<NoteDto>> action = await controller.GetNotes();

                var ok = Assert.IsType<OkObjectResult>(action.Result);
                var list = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(ok.Value);
                var arr = list.ToArray();

                Assert.Equal(3, arr.Length);
                Assert.Equal("Third", arr[0].Title);
                Assert.Equal("Second", arr[1].Title);
                Assert.Equal("First", arr[2].Title);
            }
        }
    }
}