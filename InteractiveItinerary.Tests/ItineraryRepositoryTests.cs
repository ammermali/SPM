using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Tests
{
    public class ItineraryRepositoryTests
    {
        [Fact]
        public async Task AddAsync_And_GetByIdAsync_PreservesStopOrderAndLocation()
        {
            using var db = new TestSqliteDb();
            int id;

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var itinerary = new Itinerary
                {
                    Title = "Milan Art",
                    Stops =
                    {
                        TestSqliteDb.MakeStop("Duomo", 45.4642, 9.1900),
                        TestSqliteDb.MakeStop("Brera", 45.4720, 9.1879),
                        TestSqliteDb.MakeStop("Castello", 45.4705, 9.1793)
                    }
                };
                await repo.AddAsync(itinerary);
                id = itinerary.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var loaded = await repo.GetByIdAsync(id);

                Assert.NotNull(loaded);
                Assert.Equal(new[] { "Duomo", "Brera", "Castello" }, loaded.Stops.Select(s => s.Name));
                Assert.Equal(45.4642, loaded.Stops[0].Location.Latitude);
                Assert.Equal("Brera street", loaded.Stops[1].Location.Address);
            }
        }

        [Fact]
        public async Task AddAsync_ReusesExistingStopWithoutDuplicating()
        {
            using var db = new TestSqliteDb();
            int sharedStopId;

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var first = new Itinerary
                {
                    Title = "Route 1",
                    Stops = { TestSqliteDb.MakeStop("Central Station") }
                };
                await repo.AddAsync(first);
                sharedStopId = first.Stops[0].Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var second = new Itinerary
                {
                    Title = "Route 2",
                    Stops = { new Stop { Id = sharedStopId }, TestSqliteDb.MakeStop("Harbor") }
                };
                await repo.AddAsync(second);
            }

            using (var ctx = db.CreateContext())
            {
                Assert.Equal(2, await ctx.Stops.CountAsync());
                Assert.Equal(2, await ctx.ItineraryStops.CountAsync(j => j.StopId == sharedStopId));
            }
        }

        [Fact]
        public async Task GetByIdAsync_MissingId_ReturnsNull()
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var repo = new ItineraryRepository(ctx);

            Assert.Null(await repo.GetByIdAsync(12345));
        }

        [Fact]
        public async Task GetAllAsync_And_GetByStatusAsync_FilterAndPreserveOrder()
        {
            using var db = new TestSqliteDb();

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                await repo.AddAsync(new Itinerary { Title = "Draft One", Status = ItineraryStatus.DRAFT });
                await repo.AddAsync(new Itinerary
                {
                    Title = "Published One",
                    Status = ItineraryStatus.PUBLISHED,
                    Stops = { TestSqliteDb.MakeStop("S1"), TestSqliteDb.MakeStop("S2") }
                });
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var all = await repo.GetAllAsync();
                var published = await repo.GetByStatusAsync(ItineraryStatus.PUBLISHED);
                var drafts = await repo.GetByStatusAsync(ItineraryStatus.DRAFT);

                Assert.Equal(2, all.Count);
                Assert.Single(published);
                Assert.Equal("Published One", published[0].Title);
                Assert.Equal(new[] { "S1", "S2" }, published[0].Stops.Select(s => s.Name));
                Assert.Single(drafts);
                Assert.Equal("Draft One", drafts[0].Title);
            }
        }

        [Fact]
        public async Task UpdateAsync_ReordersStopsAndBumpsUpdatedAtOnly()
        {
            using var db = new TestSqliteDb();
            int id;
            DateTime createdAt;

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var it = new Itinerary
                {
                    Title = "Original",
                    Stops = { TestSqliteDb.MakeStop("A"), TestSqliteDb.MakeStop("B"), TestSqliteDb.MakeStop("C") }
                };
                await repo.AddAsync(it);
                id = it.Id;
                createdAt = it.CreatedAt;
            }

            await Task.Delay(15);

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var tracked = (await repo.GetByIdAsync(id))!;
                tracked.Title = "Updated";
                // Drop B, reverse A and C
                tracked.Stops = new List<Stop> { tracked.Stops[2], tracked.Stops[0] };

                Assert.True(await repo.UpdateAsync(tracked));
                Assert.Equal(createdAt, tracked.CreatedAt);
                Assert.True(tracked.UpdatedAt > createdAt);
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var reloaded = (await repo.GetByIdAsync(id))!;
                Assert.Equal("Updated", reloaded.Title);
                Assert.Equal(new[] { "C", "A" }, reloaded.Stops.Select(s => s.Name));
            }
        }

        [Fact]
        public async Task UpdateAsync_MissingItinerary_ReturnsFalse()
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var repo = new ItineraryRepository(ctx);

            Assert.False(await repo.UpdateAsync(new Itinerary { Id = 9999, Title = "Ghost" }));
        }

        [Fact]
        public async Task DeleteAsync_RemovesJoinRowsAndKeepsSharedStops()
        {
            using var db = new TestSqliteDb();
            int id;

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var it = new Itinerary
                {
                    Title = "To Delete",
                    Stops = { TestSqliteDb.MakeStop("Keep Me") }
                };
                await repo.AddAsync(it);
                id = it.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                Assert.True(await repo.ExistsAsync(id));
                Assert.True(await repo.DeleteAsync(id));
                Assert.False(await repo.ExistsAsync(id));
                Assert.False(await repo.DeleteAsync(id));
                Assert.Empty(ctx.ItineraryStops);
                Assert.Single(ctx.Stops);
            }
        }

        [Fact]
        public async Task AddAsync_MissingOrDuplicateStopId_ThrowsAndDoesNotSave()
        {
            using var db = new TestSqliteDb();

            int stopId;
            using (var ctx = db.CreateContext())
            {
                var stop = TestSqliteDb.MakeStop("Colosseum");
                ctx.Stops.Add(stop);
                await ctx.SaveChangesAsync();
                stopId = stop.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    repo.AddAsync(new Itinerary { Title = "Missing", Stops = { new Stop { Id = 999 } } }));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    repo.AddAsync(new Itinerary { Title = "Dup", Stops = { new Stop { Id = stopId }, new Stop { Id = stopId } } }));
                Assert.Empty(ctx.Itineraries);
            }
        }

        [Fact]
        public async Task GetStopsByItineraryIdAsync_ReturnsOrderedStopsAndEmptyWhenMissing()
        {
            using var db = new TestSqliteDb();
            int itineraryId;

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var it = await repo.AddAsync(new Itinerary
                {
                    Title = "Ordered Stops",
                    Stops = { TestSqliteDb.MakeStop("First"), TestSqliteDb.MakeStop("Second"), TestSqliteDb.MakeStop("Third") }
                });
                itineraryId = it.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var stops = await repo.GetStopsByItineraryIdAsync(itineraryId);
                Assert.Equal(new[] { "First", "Second", "Third" }, stops.Select(s => s.Name));

                var missing = await repo.GetStopsByItineraryIdAsync(9999);
                Assert.Empty(missing);
            }
        }

        [Fact]
        public async Task GetStopByIdAsync_ReturnsStopWhenFoundAndNullWhenMissing()
        {
            using var db = new TestSqliteDb();
            int stopId;

            using (var ctx = db.CreateContext())
            {
                var stop = TestSqliteDb.MakeStop("Piazza Navona", 41.8992, 12.4731);
                ctx.Stops.Add(stop);
                await ctx.SaveChangesAsync();
                stopId = stop.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var found = await repo.GetStopByIdAsync(stopId);
                Assert.NotNull(found);
                Assert.Equal("Piazza Navona", found.Name);
                Assert.Equal(41.8992, found.Location.Latitude);

                Assert.Null(await repo.GetStopByIdAsync(9999));
            }
        }
    }
}
