using InteractiveItinerary.Data;
using InteractiveItinerary.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Tests
{
    /// <summary>
    /// Creates an isolated in-memory SQLite database with all EF Core migrations applied.
    /// Keeps the connection open for the lifetime of the test so the in-memory DB persists across contexts.
    /// </summary>
    public sealed class TestSqliteDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        public DbContextOptions<ItineraryDbContext> Options { get; }

        public TestSqliteDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            Options = new DbContextOptionsBuilder<ItineraryDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var ctx = CreateContext();
            ctx.Database.Migrate();
        }

        public ItineraryDbContext CreateContext() => new(Options);

        public static Stop MakeStop(string name, double lat = 41.9, double lon = 12.5) => new()
        {
            Name = name,
            Description = $"{name} description",
            Location = new Location(lat, lon, $"{name} street")
        };

        public void Dispose() => _connection.Dispose();
    }
}
