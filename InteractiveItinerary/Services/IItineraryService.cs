using InteractiveItinerary.DTO;

namespace InteractiveItinerary.Services
{
    public interface IItineraryService
    {
        /// <summary>
        /// Maps the incoming DTO to an <see cref="Entities.Itinerary"/>, forces its status to
        /// <see cref="Entities.ItineraryStatus.DRAFT"/>, and persists it with a system-assigned Id.
        /// </summary>
        Task<ItineraryDTO> CreateDraftAsync(CreateItineraryDTO dto, CancellationToken cancellationToken = default);

        Task<ItineraryDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
