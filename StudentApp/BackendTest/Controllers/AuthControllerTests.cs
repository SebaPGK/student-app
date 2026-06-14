using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentApp.Controllers;
using StudentApp.Data;
using StudentApp.Model.DTO;
using StudentApp.Model.Entities;

namespace BackendTest.Controllers
{
    public class AuthControllerTests
    {
        private static IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "super_secret_test_key_which_should_be_long" },
                { "Jwt:Issuer", "testIssuer" },
                { "Jwt:Audience", "testAudience" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

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

        [Fact]
        public async Task Register_WithUniqueEmail_CreatesUserAndReturnsOk()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var config = CreateTestConfiguration();
                var controller = new AuthController(context, config);

                var dto = new RegisterUserDto
                {
                    Username = "tester",
                    Email = "tester@example.com",
                    Password = "Password123"
                };

                ActionResult<UserDto> actionResult = await controller.Register(dto);

                if (actionResult.Result is OkObjectResult okResult)
                {
                    var returnedUser = Assert.IsType<UserDto>(okResult.Value);
                    Assert.Equal(dto.Email, returnedUser.Email);
                    Assert.Equal(dto.Username, returnedUser.Username);
                }
                else
                {
                    var returnedUser = Assert.IsType<UserDto>(actionResult.Value);
                    Assert.Equal(dto.Email, returnedUser.Email);
                    Assert.Equal(dto.Username, returnedUser.Username);
                }

                var userInDb = context.Users.SingleOrDefault(u => u.Email == dto.Email);
                Assert.NotNull(userInDb);
                Assert.False(string.IsNullOrWhiteSpace(userInDb.PasswordHash));
                Assert.False(string.IsNullOrWhiteSpace(userInDb.PasswordSalt));
            }
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var config = CreateTestConfiguration();
                var controller = new AuthController(context, config);

                context.Users.Add(new User
                {
                    Username = "existing",
                    Email = "exists@example.com",
                    PasswordHash = "hash",
                    PasswordSalt = "salt",
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();

                var dto = new RegisterUserDto
                {
                    Username = "new",
                    Email = "exists@example.com",
                    Password = "Password123"
                };

                ActionResult<UserDto> actionResult = await controller.Register(dto);

                // oczekujemy BadRequestResult w .Result
                Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            }
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var config = CreateTestConfiguration();
                var controller = new AuthController(context, config);

                var registerDto = new RegisterUserDto
                {
                    Username = "loginuser",
                    Email = "login@example.com",
                    Password = "SecurePass1"
                };

                ActionResult<UserDto> regAction = await controller.Register(registerDto);

                UserDto registered;
                if (regAction.Result is OkObjectResult regOk)
                {
                    registered = Assert.IsType<UserDto>(regOk.Value);
                }
                else
                {
                    registered = Assert.IsType<UserDto>(regAction.Value);
                }

                var loginDto = new LoginUserDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };

                ActionResult loginAction = await controller.Login(loginDto);

                var okResult = Assert.IsType<OkObjectResult>(loginAction);
                var value = okResult.Value!;
                var tokenProp = value.GetType().GetProperty("token");
                var userProp = value.GetType().GetProperty("user");

                Assert.NotNull(tokenProp);
                Assert.NotNull(userProp);

                var token = tokenProp!.GetValue(value) as string;
                var returnedUser = userProp!.GetValue(value) as UserDto;

                Assert.False(string.IsNullOrWhiteSpace(token));
                Assert.NotNull(returnedUser);
                Assert.Equal(registered.Id, returnedUser!.Id);
                Assert.Equal(registered.Email, returnedUser.Email);

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token!);
                var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                Assert.NotNull(nameIdClaim);
                Assert.Equal(registered.Id.ToString(), nameIdClaim!.Value);
                Assert.NotNull(emailClaim);
                Assert.Equal(registered.Email, emailClaim!.Value);
            }
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var (context, connection) = CreateSqliteInMemoryContext();
            using (connection)
            using (context)
            {
                var config = CreateTestConfiguration();
                var controller = new AuthController(context, config);

                var loginDto = new LoginUserDto
                {
                    Email = "nonexistent@example.com",
                    Password = "Nope"
                };

                ActionResult loginAction = await controller.Login(loginDto);

                Assert.IsType<UnauthorizedObjectResult>(loginAction);
            }
        }
    }
}