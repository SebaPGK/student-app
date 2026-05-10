using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;

namespace StudentApp.Controllers.Endpoints
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // SprawdŸ unikalnoœæ emailu i nazwy u¿ytkownika
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return Conflict(new { message = "Email jest ju¿ zajêty." });
            }

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return Conflict(new { message = "Nazwa u¿ytkownika jest ju¿ zajêta." });
            }

            // Utwórz hash has³a
            CreatePasswordHash(dto.Password, out var passwordHash, out var passwordSalt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, result);
        }

        // GET: api/auth/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return NotFound();
            }

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(dto);
        }

        private static void CreatePasswordHash(string password, out string passwordHashBase64, out string saltBase64)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            // PBKDF2 hash
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            saltBase64 = Convert.ToBase64String(salt);
            passwordHashBase64 = Convert.ToBase64String(hash);
        }
    }
}