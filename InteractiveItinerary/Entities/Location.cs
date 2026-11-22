using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.Entities
{
    public readonly struct Location
    {
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string Address { get; }

        public Location(double latitude, double longitude, string address)
        {
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
        }
    }
}