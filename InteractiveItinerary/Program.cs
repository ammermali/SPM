// PLACEHOLDER host — intentionally incomplete.
// Only wires up what the persistence layer needs (DbContext + repository) so the app builds
// and `dotnet ef` can create the DbContext at design time.
// TODO: error handling, auth, CORS, health checks, real controllers/endpoints, etc.

using InteractiveItinerary.Data;
using InteractiveItinerary.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Resolve a relative SQLite path against the project (content root) folder, so the database
// is always <project>/interactiveitinerary.db no matter which folder the app is started from.
var sqlite = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing."));

if (!Path.IsPathRooted(sqlite.DataSource) && sqlite.DataSource != ":memory:")
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);

builder.Services.AddDbContext<ItineraryDbContext>(options =>
    options.UseSqlite(sqlite.ToString()));

builder.Services.AddScoped<IItineraryRepository, ItineraryRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create the database file (if missing) and apply any pending migrations at startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ItineraryDbContext>();
    db.Database.Migrate();
    app.Logger.LogInformation("SQLite database ready at {Path}", sqlite.DataSource);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
