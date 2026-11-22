using System.ComponentModel.DataAnnotations;
using InteractiveItinerary.DTO;
using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Tests
{
    public class EntityAndConstraintTests
    {
        private static IList<ValidationResult> Validate(object instance)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void ItineraryDto_ExceedingMaxLengths_FailsValidation()
        {
            var dto = new ItineraryDTO
            {
                Title = new string('a', 201),
                Theme = new string('b', 201),
                GeographicArea = new string('c', 201),
                Description = new string('d', 2001)
            };

            var errors = Validate(dto);
            Assert.Equal(4, errors.Count);
        }

        [Fact]
        public void Stop_EmptyNameAndLongDescription_FailsValidation()
        {
            var stop = new Stop
            {
                Name = "",
                Description = new string('x', 2001),
                Location = new Location(45.0, 9.0, "Valid")
            };

            var errors = Validate(stop);
            Assert.Equal(2, errors.Count);
        }

        [Theory]
        [InlineData(-91, 0, "Valid")]
        [InlineData(91, 0, "Valid")]
        [InlineData(0, -181, "Valid")]
        [InlineData(0, 181, "Valid")]
        [InlineData(0, 0, "")]
        public void Location_InvalidValues_FailValidation(double lat, double lon, string address)
        {
            object loc = new Location(lat, lon, address);
            var errors = Validate(loc);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task DatabaseCheckConstraints_RejectInvalidStopCoordinatesAndBlankName()
        {
            using var db = new TestSqliteDb();

            await AssertDbRejected(db, new Itinerary
            {
                Stops = { new Stop { Name = "Bad Lat", Description = "d", Location = new Location(91, 0, "a") } }
            });

            await AssertDbRejected(db, new Itinerary
            {
                Stops = { new Stop { Name = "Bad Lon", Description = "d", Location = new Location(0, -181, "a") } }
            });

            await AssertDbRejected(db, new Itinerary
            {
                Stops = { new Stop { Name = "   ", Description = "d", Location = new Location(0, 0, "a") } }
            });

            await AssertDbRejected(db, new Itinerary
            {
                Stops = { new Stop { Name = "No Address", Description = "d" } }
            });
        }

        [Fact]
        public async Task DatabaseCheckConstraint_RejectsInvalidItineraryStatus()
        {
            using var db = new TestSqliteDb();

            await AssertDbRejected(db, new Itinerary
            {
                Title = "Invalid Status",
                Status = (ItineraryStatus)99
            });
        }

        private static async Task AssertDbRejected(TestSqliteDb db, Itinerary invalid)
        {
            using var ctx = db.CreateContext();
            var repo = new ItineraryRepository(ctx);
            await Assert.ThrowsAsync<DbUpdateException>(() => repo.AddAsync(invalid));
        }
    }
}
