using System.ComponentModel.DataAnnotations;

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
    }
}
