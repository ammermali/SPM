using InteractiveItinerary.DTO;
using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;
using InteractiveItinerary.Services;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Tests
{
    public class AddStopTests
    {
        [Fact]
        public async Task AddStopAsync_DraftItinerary_AssociatesExistingStopsInOrderWithoutDuplicating()
        {
            using var db = new TestSqliteDb();
            int itineraryId, stop1Id, stop2Id;

            using (var ctx = db.CreateContext())
            {
                var s1 = TestSqliteDb.MakeStop("Colosseum", 41.8902, 12.4922);
                var s2 = TestSqliteDb.MakeStop("Pantheon", 41.8986, 12.4769);
                ctx.Stops.AddRange(s1, s2);
                await ctx.SaveChangesAsync();
                stop1Id = s1.Id;
                stop2Id = s2.Id;

                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Rome Walk" })).Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                await svc.AddStopAsync(itineraryId, stop1Id);
                var afterSecond = await svc.AddStopAsync(itineraryId, stop2Id);

                Assert.Equal(new[] { stop1Id, stop2Id }, afterSecond.Stops);
            }

            using (var ctx = db.CreateContext())
            {
                Assert.Equal(2, await ctx.Stops.CountAsync());
                var repo = new ItineraryRepository(ctx);
                var reloaded = await repo.GetByIdAsync(itineraryId);

                Assert.NotNull(reloaded);
                Assert.Equal(new[] { "Colosseum", "Pantheon" }, reloaded.Stops.Select(s => s.Name));
                Assert.Equal(41.8902, reloaded.Stops[0].Location.Latitude);
            }
        }

        [Fact]
        public async Task AddStopAsync_MissingItinerary_ThrowsKeyNotFound()
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
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.AddStopAsync(9999, stopId));
                Assert.Empty(ctx.ItineraryStops);
            }
        }

        [Fact]
        public async Task AddStopAsync_MissingStop_ThrowsKeyNotFoundAndDoesNotLink()
        {
            using var db = new TestSqliteDb();
            int itineraryId;

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Draft" })).Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.AddStopAsync(itineraryId, 9999));
                Assert.Empty(ctx.ItineraryStops);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddStopAsync_NonPositiveStopId_ThrowsArgumentOutOfRange(int badStopId)
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var svc = new ItineraryService(new ItineraryRepository(ctx));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.AddStopAsync(1, badStopId));
        }

        [Fact]
        public async Task AddStopAsync_DuplicateStopInSameItinerary_ThrowsInvalidOperation()
        {
            using var db = new TestSqliteDb();
            int itineraryId, stopId;

            using (var ctx = db.CreateContext())
            {
                var stop = TestSqliteDb.MakeStop("Colosseum");
                ctx.Stops.Add(stop);
                await ctx.SaveChangesAsync();
                stopId = stop.Id;

                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Draft" })).Id;
                await svc.AddStopAsync(itineraryId, stopId);
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddStopAsync(itineraryId, stopId));
                Assert.Equal(1, await ctx.ItineraryStops.CountAsync(j => j.ItineraryId == itineraryId));
            }
        }

        [Fact]
        public async Task AddStopAsync_NonDraftItinerary_ThrowsInvalidOperationAndDoesNotLink()
        {
            using var db = new TestSqliteDb();
            int publishedId, stopId;

            using (var ctx = db.CreateContext())
            {
                var stop = TestSqliteDb.MakeStop("Colosseum");
                ctx.Stops.Add(stop);
                var repo = new ItineraryRepository(ctx);
                var published = await repo.AddAsync(new Itinerary
                {
                    Title = "Published Route",
                    Status = ItineraryStatus.PUBLISHED
                });
                publishedId = published.Id;
                stopId = stop.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddStopAsync(publishedId, stopId));
                Assert.Empty(ctx.ItineraryStops);
            }
        }

        [Fact]
        public async Task AddStopAsync_WithPayload_AppearsInListAndStopsQuery_AC01_AC02()
        {
            using var db = new TestSqliteDb();
            int itineraryId;

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Rome" })).Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                var added = await svc.AddStopAsync(itineraryId, new StopRequestDTO
                {
                    Name = "Trevi Fountain",
                    Description = "Baroque fountain",
                    Latitude = 41.9009,
                    Longitude = 12.4833,
                    Address = "Piazza di Trevi"
                });

                Assert.True(added.Id > 0);
                Assert.Equal("Trevi Fountain", added.Name);
                Assert.Equal(41.9009, added.Latitude);
                Assert.Equal(12.4833, added.Longitude);

                var itinerary = await svc.GetByIdAsync(itineraryId);
                Assert.NotNull(itinerary);
                Assert.Contains(added.Id, itinerary.Stops);

                var stops = await svc.GetStopsAsync(itineraryId);
                Assert.NotNull(stops);
                Assert.Single(stops);
                Assert.Equal(41.9009, stops[0].Latitude);
                Assert.Equal(12.4833, stops[0].Longitude);
            }
        }

        [Fact]
        public async Task UpdateStopAsync_EditsDataAndPersistsAfterSave_AC03()
        {
            using var db = new TestSqliteDb();
            int itineraryId, stopId;

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Rome" })).Id;
                stopId = (await svc.AddStopAsync(itineraryId, new StopRequestDTO
                {
                    Name = "Old Name",
                    Description = "Old desc",
                    Latitude = 41.9,
                    Longitude = 12.4,
                    Address = "Old street"
                })).Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                var updated = await svc.UpdateStopAsync(itineraryId, stopId, new StopRequestDTO
                {
                    Name = "New Name",
                    Description = "Updated desc",
                    Latitude = 41.9028,
                    Longitude = 12.4964,
                    Address = "New street"
                });

                Assert.Equal("New Name", updated.Name);
                Assert.Equal(41.9028, updated.Latitude);
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                var stops = await svc.GetStopsAsync(itineraryId);
                Assert.NotNull(stops);
                Assert.Single(stops);
                Assert.Equal("New Name", stops[0].Name);
                Assert.Equal("Updated desc", stops[0].Description);
                Assert.Equal(41.9028, stops[0].Latitude);
                Assert.Equal(12.4964, stops[0].Longitude);
                Assert.Equal("New street", stops[0].Address);
            }
        }

        [Theory]
        [InlineData(-90.0001, 0)]
        [InlineData(90.0001, 0)]
        [InlineData(0, -180.0001)]
        [InlineData(0, 180.0001)]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.PositiveInfinity)]
        public async Task AddAndUpdateStopAsync_OutOfRangeCoordinates_ThrowsAndDoesNotPersist_AC04(double lat, double lon)
        {
            using var db = new TestSqliteDb();
            int itineraryId, stopId;

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                itineraryId = (await svc.CreateDraftAsync(new CreateItineraryDTO { Title = "Coord Test" })).Id;
                stopId = (await svc.AddStopAsync(itineraryId, new StopRequestDTO
                {
                    Name = "Valid",
                    Description = "d",
                    Latitude = 45.0,
                    Longitude = 9.0,
                    Address = "Addr"
                })).Id;
            }

            using (var ctx = db.CreateContext())
            {
                var svc = new ItineraryService(new ItineraryRepository(ctx));
                var bad = new StopRequestDTO { Name = "Bad", Description = "d", Latitude = lat, Longitude = lon, Address = "Addr" };

                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.AddStopAsync(itineraryId, bad));
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.UpdateStopAsync(itineraryId, stopId, bad));

                var stops = await svc.GetStopsAsync(itineraryId);
                Assert.Single(stops!);
                Assert.Equal(45.0, stops![0].Latitude);
            }
        }
    }
}
