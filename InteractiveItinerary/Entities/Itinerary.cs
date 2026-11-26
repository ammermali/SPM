using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using InteractiveItinerary.Exceptions;

namespace InteractiveItinerary.Entities
{
    public class Itinerary
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "Theme cannot exceed 200 characters.")]
        public string? Theme { get; set; }

        public List<Stop> Stops { get; set; } = new List<Stop>();

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ItineraryStatus), ErrorMessage = "Status must be DRAFT or PUBLISHED.")]
        public ItineraryStatus Status { get; set; } = ItineraryStatus.DRAFT;

        /// <summary>Set automatically (UTC) when the itinerary is first saved.</summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>Set automatically (UTC) every time the itinerary or its stop list changes.</summary>
        [Required]
        public DateTime UpdatedAt { get; set; }

        [MaxLength(200, ErrorMessage = "Geographic area cannot exceed 200 characters.")]
        public string? GeographicArea { get; set; }

        /// <summary>
        /// Reorders the stops currently linked to this draft itinerary according to <paramref name="stopIds"/>.
        /// </summary>
        public void ReorderStops(IReadOnlyList<int> stopIds)
        {
            ArgumentNullException.ThrowIfNull(stopIds);

            if (Status != ItineraryStatus.DRAFT)
                throw new InvalidOperationException($"Cannot reorder stops on itinerary {Id} because its status is {Status}; " +
                    $"only DRAFT itineraries can be modified.");

            if (stopIds.Count == 0)
                throw new ArgumentException("The stop sequence cannot be empty.", nameof(stopIds));

            if (stopIds.Distinct().Count() != stopIds.Count)
                throw new InvalidOperationException("The stop sequence cannot contain duplicate stop IDs.");

            if (stopIds.Count != Stops.Count)
            {
                throw new InvalidOperationException(
                    "The stop sequence must contain all stops currently linked to this itinerary without adding or omitting stops.");
            }

            var stopsById = Stops.ToDictionary(s => s.Id);
            var reorderedStops = new List<Stop>(stopIds.Count);

            foreach (var stopId in stopIds)
            {
                if (!stopsById.TryGetValue(stopId, out var stop))
                {
                    throw new InvalidOperationException(
                        $"Stop with Id {stopId} is not linked to itinerary {Id}.");
                }

                reorderedStops.Add(stop);
            }

            Stops.Clear();
            Stops.AddRange(reorderedStops);
        }
    }
}
