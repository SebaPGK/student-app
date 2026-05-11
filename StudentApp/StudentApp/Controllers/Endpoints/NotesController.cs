using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes()
        {
            var userId = GetCurrentUserId();

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => MapToDto(n))
                .ToListAsync();

            return Ok(notes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NoteDto>> GetNoteById(int id)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(note));
        }

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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, UpdateNoteDto dto)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound();
            }

            if (dto.Title != null)
            {
                note.Title = dto.Title;
            }

            if (dto.Content != null)
            {
                note.Content = dto.Content;
            }

            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetCurrentUserId();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static NoteDto MapToDto(Note note)
        {
            return new NoteDto
            {
                Id = note.Id,
                UserId = note.UserId,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
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