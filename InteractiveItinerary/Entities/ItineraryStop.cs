using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.Entities
{
    /// <summary>
    /// Join entity for the many-to-many relationship between <see cref="Itinerary"/> and <see cref="Stop"/>.
    /// A stop can be reused by several itineraries; <see cref="Position"/> keeps the order
    /// of the stop inside a specific itinerary (0-based, mirrors the index in <see cref="Itinerary.Stops"/>).
    /// Composite key (ItineraryId, StopId) is configured in ItineraryConfiguration.
    /// </summary>
    public class ItineraryStop
    {
        [Range(1, int.MaxValue, ErrorMessage = "ItineraryId must be a valid itinerary id.")]
        public int ItineraryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "StopId must be a valid stop id.")]
        public int StopId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Position must be zero or greater.")]
        public int Position { get; set; }
    }
}
