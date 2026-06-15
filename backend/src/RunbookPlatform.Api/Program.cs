using Microsoft.EntityFrameworkCore;
using RunbookPlatform.Api.Data;
using RunbookPlatform.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// FR-010: no authentication, accounts, or permissions in this slice.
var dbPath = builder.Configuration["Database:Path"] ?? "runbook-platform.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.MapRunbookEndpoints();
app.MapExecutionEndpoints();

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in tests.
public partial class Program { }
