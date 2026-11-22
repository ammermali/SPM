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

        /// <summary>
        /// Verifies that the target itinerary exists and is in <see cref="Entities.ItineraryStatus.DRAFT"/> status,
        /// then associates an existing stop by its <paramref name="stopId"/>.
        /// </summary>
        Task<ItineraryDTO> AddStopAsync(int itineraryId, int stopId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies that the target itinerary exists and is in <see cref="Entities.ItineraryStatus.DRAFT"/> status,
        /// validates the geolocated stop payload, creates the stop, and associates it with the itinerary.
        /// </summary>
        Task<StopDTO> AddStopAsync(int itineraryId, StopRequestDTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies that the target itinerary exists, is in <see cref="Entities.ItineraryStatus.DRAFT"/> status,
        /// and contains <paramref name="stopId"/>, then updates the stop's data and coordinates.
        /// </summary>
        Task<StopDTO> UpdateStopAsync(int itineraryId, int stopId, StopRequestDTO dto, CancellationToken cancellationToken = default);

        /// <summary>Returns the ordered stops (with coordinates) for an itinerary, or <c>null</c> if the itinerary does not exist.</summary>
        Task<IReadOnlyList<StopDTO>?> GetStopsAsync(int itineraryId, CancellationToken cancellationToken = default);
    }
}
