using System.ComponentModel.DataAnnotations;

namespace InteractiveItinerary.Entities
{
    public class Stop
    {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Value object stored inline in the Stops table. Its own constraints are declared on <see cref="Entities.Location"/>
        /// (note: <c>Validator</c>/MVC model validation does not recurse into it automatically).
        /// </summary>
        public Location Location { get; set; }
    }
}
