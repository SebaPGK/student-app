using System.ComponentModel.DataAnnotations;

namespace StudentApp.Model.DTO
{
    public record LoginUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}