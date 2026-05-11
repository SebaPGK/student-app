using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentApp.Data;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser is not null)
            {
                return BadRequest("U¿ytkownik o takim adresie email ju¿ istnieje.");
            }

            var salt = GenerateSalt();
            var passwordHash = HashPassword(dto.Password, salt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Nieprawid³owy email lub has³o.");
            }

            var passwordHash = HashPassword(dto.Password, user.PasswordSalt);

            if (passwordHash != user.PasswordHash)
            {
                return Unauthorized("Nieprawid³owy email lub has³o.");
            }

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty)
            };

            return Ok(new {user = result });
        }

        private static string GenerateSalt()
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();

            var combinedValue = password + salt;
            var bytes = System.Text.Encoding.UTF8.GetBytes(combinedValue);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}