using StudentApp.Model.DTO;

namespace StudentApp.Model.Mappers
{
    static public class NotesMapper
    {
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
    }
}
