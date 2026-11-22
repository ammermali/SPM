using InteractiveItinerary.DTO;
using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;
using InteractiveItinerary.Services;

namespace InteractiveItinerary.Tests
{
    public class ItineraryServiceTests
    {
        [Fact]
        public async Task CreateDraftAsync_MapsFieldsAndPersists_AC01()
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var service = new ItineraryService(new ItineraryRepository(ctx));

            var input = new CreateItineraryDTO
            {
                Title = "Hidden Rome",
                Theme = "Architecture",
                Description = "Backstreet churches and courtyards.",
                GeographicArea = "Rome, Italy"
            };

            var created = await service.CreateDraftAsync(input);

            Assert.Equal("Hidden Rome", created.Title);
            Assert.Equal("Architecture", created.Theme);
            Assert.Equal("Backstreet churches and courtyards.", created.Description);
            Assert.Equal("Rome, Italy", created.GeographicArea);
            Assert.Empty(created.Stops);
        }

        [Fact]
        public async Task CreateDraftAsync_SetsDraftStatusAndExcludesFromPublishedCatalog_AC02()
        {
            using var db = new TestSqliteDb();

            int draftId;
            using (var ctx = db.CreateContext())
            {
                var service = new ItineraryService(new ItineraryRepository(ctx));
                var created = await service.CreateDraftAsync(new CreateItineraryDTO
                {
                    Title = "Street Food Tour",
                    Theme = "Food",
                    Description = "Local markets.",
                    GeographicArea = "Naples"
                });

                draftId = created.Id;
                Assert.Equal(ItineraryStatus.DRAFT, created.Status);
            }

            using (var ctx = db.CreateContext())
            {
                var repo = new ItineraryRepository(ctx);
                var published = await repo.GetByStatusAsync(ItineraryStatus.PUBLISHED);
                Assert.DoesNotContain(published, i => i.Id == draftId);
            }
        }

        [Fact]
        public async Task CreateDraftAsync_AssignsIncrementalIdsAndUtcTimestamps_AC03()
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var service = new ItineraryService(new ItineraryRepository(ctx));

            var first = await service.CreateDraftAsync(new CreateItineraryDTO
            {
                Title = "First",
                Theme = "History",
                Description = "First desc",
                GeographicArea = "Florence"
            });
            var second = await service.CreateDraftAsync(new CreateItineraryDTO
            {
                Title = "Second",
                Theme = "Art",
                Description = "Second desc",
                GeographicArea = "Venice"
            });

            Assert.Equal(1, first.Id);
            Assert.Equal(2, second.Id);
            Assert.Equal(DateTimeKind.Utc, first.CreatedAt.Kind);
            Assert.Equal(first.CreatedAt, first.UpdatedAt);
        }

        [Fact]
        public async Task CreateDraftAsync_NullDto_ThrowsArgumentNullException_AC04()
        {
            using var db = new TestSqliteDb();
            using var ctx = db.CreateContext();
            var service = new ItineraryService(new ItineraryRepository(ctx));

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateDraftAsync(null!));
            Assert.Empty(ctx.Itineraries);
        }

        [Fact]
        public async Task CreateDraftAsync_ReopeningSavedDraftPreservesAllData_AC05()
        {
            using var db = new TestSqliteDb();

            int draftId;
            using (var ctx = db.CreateContext())
            {
                var service = new ItineraryService(new ItineraryRepository(ctx));
                var created = await service.CreateDraftAsync(new CreateItineraryDTO
                {
                    Title = "Baroque Walk",
                    Theme = "Baroque",
                    Description = "Evening walk",
                    GeographicArea = "Centro Storico"
                });
                draftId = created.Id;
            }

            using (var ctx = db.CreateContext())
            {
                var service = new ItineraryService(new ItineraryRepository(ctx));
                var reopened = await service.GetByIdAsync(draftId);

                Assert.NotNull(reopened);
                Assert.Equal("Baroque Walk", reopened.Title);
                Assert.Equal("Baroque", reopened.Theme);
                Assert.Equal("Evening walk", reopened.Description);
                Assert.Equal("Centro Storico", reopened.GeographicArea);
                Assert.Equal(ItineraryStatus.DRAFT, reopened.Status);
                Assert.Empty(reopened.Stops);
            }
        }
    }
}
