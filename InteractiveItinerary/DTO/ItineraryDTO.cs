using InteractiveItinerary.Entities;

namespace InteractiveItinerary.DTO
{
    public class ItineraryDTO
    {
        private int id;
        private string? title;
        private string? description;
        private string? theme;
        private List<int> stops = new List<int>();
        private ItineraryStatus status;
        private DateTime createdAt;
        private DateTime updatedAt;
        private string? geographicArea;

        public ItineraryDTO(Itinerary itinerary)
        {
            this.id = itinerary.Id;
            this.title = itinerary.Title;
            this.description = itinerary.Description;
            this.theme = itinerary.Theme;
            this.stops = itinerary.Stops.Select(s => s.Id).ToList();
            this.status = itinerary.Status;
            this.createdAt = itinerary.CreatedAt;
            this.updatedAt = itinerary.UpdatedAt;
            this.geographicArea = itinerary.GeographicArea;
        }
    }
}
