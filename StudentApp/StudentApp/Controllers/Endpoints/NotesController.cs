using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Model.DTO;
using System.Security.Claims;

namespace StudentApp.Controllers.Endpoints
{
    [ApiController]
    [Route("api/notes")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes()
        {
            var userId = GetCurrentUserId();

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();

            return Ok(notes);
        }

        // GET: api/notes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<NoteDto>> GetNoteById(int id)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(note));
        }

        // POST: api/notes
        [HttpPost]
        public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto dto)
        {
            var userId = GetCurrentUserId();

            var note = new Note
            {
                UserId = userId,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var result = MapToDto(note);

            return CreatedAtAction(
                nameof(GetNoteById),
                new { id = note.Id },
                result);
        }

        // PUT: api/notes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, UpdateNoteDto dto)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note is null)
            {
                return NotFound();
            }

            note.Title = dto.Title;
            note.Content = dto.Content;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/notes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note is null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
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