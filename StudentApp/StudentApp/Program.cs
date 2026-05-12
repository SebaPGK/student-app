using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


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