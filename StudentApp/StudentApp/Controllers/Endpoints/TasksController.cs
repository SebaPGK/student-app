using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Model.DTO;
using StudentApp.Model.Enums;
using System.Security.Claims;

namespace StudentApp.Controllers.Endpoints
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tasks
        // Obsługuje listę zadań + filtrowanie.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] TaskFilterDto filter)
        {
            var userId = GetCurrentUserId();

            var query = _context.UserTasks
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filter.Priority.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);
            }

            if (filter.DueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchPhrase))
            {
                var phrase = filter.SearchPhrase.Trim();

                query = query.Where(t =>
                    t.Title.Contains(phrase) ||
                    (t.Description != null && t.Description.Contains(phrase)));
            }

            var tasks = await query
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/tasks/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskDto>> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(task));
        }

        // GET: api/tasks/upcoming?limit=5
        // Panel główny z najbliższymi terminami.
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<UpcomingTaskDto>>> GetUpcomingTasks([FromQuery] int limit = 5)
        {
            var userId = GetCurrentUserId();

            if (limit <= 0)
            {
                limit = 5;
            }

            var now = DateTime.UtcNow;

            var tasks = await _context.UserTasks
                .Where(t =>
                    t.UserId == userId &&
                    t.DueDate != null &&
                    t.DueDate >= now &&
                    t.Status != TaskStatusDto.Zrobione)
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .Select(t => new UpcomingTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto dto)
        {
            var userId = GetCurrentUserId();

            var task = new UserTask
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = TaskStatusDto.DoZrobienia,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync();

            var result = MapToDto(task);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = task.Id },
                result);
        }

        // PUT: api/tasks/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var userId = GetCurrentUserId();

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task is null)
            {
                return NotFound();
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            task.Priority = dto.Priority;
            task.Status = dto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/tasks/5/status
        // Zmiana samego statusu, np. DoZrobienia -> WTrakcie -> Zrobione.
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, UpdateTaskStatusDto dto)
        {
            var userId = GetCurrentUserId();

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task is null)
            {
                return NotFound();
            }

            task.Status = dto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/tasks/5/complete
        // Szybkie oznaczenie jako wykonane.
        [HttpPatch("{id:int}/complete")]
        public async Task<IActionResult> MarkTaskAsCompleted(int id)
        {
            var userId = GetCurrentUserId();

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task is null)
            {
                return NotFound();
            }

            task.Status = TaskStatusDto.Zrobione;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();

            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task is null)
            {
                return NotFound();
            }

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Brak identyfikatora użytkownika w tokenie.");
            }

            return int.Parse(userId);
        }
    }
}
