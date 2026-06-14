using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentApp.Data;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    var endpointDataSource = app.Services.GetService<EndpointDataSource>();
    if (endpointDataSource is not null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Registered endpoints:");
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            var routeEndpoint = endpoint as RouteEndpoint;
            var pattern = routeEndpoint?.RoutePattern?.RawText ?? endpoint.DisplayName ?? "<unknown>";
            sb.AppendLine($"{pattern}  ->  {endpoint.DisplayName}");
        }

        Debug.WriteLine(sb.ToString());
        app.Logger.LogInformation(sb.ToString());
    }
}

app.Run();
