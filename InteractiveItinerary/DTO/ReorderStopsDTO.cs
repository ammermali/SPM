using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.DTO
{
    public class ReorderStopsDTO
    {
        [Required(ErrorMessage = "StopIds is required.")]
        [MinLength(1, ErrorMessage = "StopIds must contain at least one stop ID.")]
        public List<int> StopIds { get; set; } = new();
    }
}