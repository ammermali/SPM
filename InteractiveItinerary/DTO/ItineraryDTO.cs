using System.ComponentModel.DataAnnotations;
using InteractiveItinerary.Entities;

namespace InteractiveItinerary.DTO
{
    public class ItineraryDTO
    {
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "Theme cannot exceed 200 characters.")]
        public string? Theme { get; set; }

        public List<int> Stops { get; set; } = new List<int>();

        public ItineraryStatus Status { get; set; } = ItineraryStatus.DRAFT;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [MaxLength(200, ErrorMessage = "Geographic area cannot exceed 200 characters.")]
        public string? GeographicArea { get; set; }

        public ItineraryDTO()
        {
        }

        public ItineraryDTO(Itinerary itinerary)
        {
            Id = itinerary.Id;
            Title = itinerary.Title;
            Description = itinerary.Description;
            Theme = itinerary.Theme;
            Stops = itinerary.Stops.Select(s => s.Id).ToList();
            Status = itinerary.Status;
            CreatedAt = itinerary.CreatedAt;
            UpdatedAt = itinerary.UpdatedAt;
            GeographicArea = itinerary.GeographicArea;
        }
    }
}
