using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.DTO
{
    /// <summary>
    /// Request payload for adding a new geolocated stop or editing an existing stop on a draft itinerary.
    /// </summary>
    public class StopRequestDTO
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Address is required.")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;
    }
}
