using InteractiveItinerary.Entities;

namespace InteractiveItinerary.DTO
{
    public class StopDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;

        public StopDTO()
        {
        }

        public StopDTO(Stop stop)
        {
            Id = stop.Id;
            Name = stop.Name;
            Description = stop.Description;
            Latitude = stop.Location.Latitude;
            Longitude = stop.Location.Longitude;
            Address = stop.Location.Address;
        }
    }
}
