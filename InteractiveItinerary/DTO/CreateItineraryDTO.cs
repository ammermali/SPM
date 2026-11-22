using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.DTO
{
    /// <summary>
    /// Request payload for creating a new draft itinerary.
    /// System-assigned fields (<c>Id</c>, <c>Status</c>, <c>CreatedAt</c>, <c>UpdatedAt</c>) and <c>Stops</c>
    /// are intentionally excluded—stops are added in a later step before publishing.
    /// </summary>
    public class CreateItineraryDTO
    {
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "Theme cannot exceed 200 characters.")]
        public string? Theme { get; set; }

        [MaxLength(200, ErrorMessage = "Geographic area cannot exceed 200 characters.")]
        public string? GeographicArea { get; set; }
    }
}
