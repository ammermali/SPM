namespace InteractiveItinerary.Entities
{
    public class Itinerary
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Theme { get; set; }
        public List<Stop> Stops { get; set; } = new List<Stop>();
        public ItineraryStatus Status { get; set; } = ItineraryStatus.DRAFT;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? GeographicArea { get; set; }
    }
}
