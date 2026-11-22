using InteractiveItinerary.DTO;
using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;

namespace InteractiveItinerary.Services
{
    public class ItineraryService : IItineraryService
    {
        private readonly IItineraryRepository _itineraryRepository;

        public ItineraryService(IItineraryRepository itineraryRepository)
        {
            _itineraryRepository = itineraryRepository;
        }

        public async Task<ItineraryDTO> CreateDraftAsync(CreateItineraryDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var itinerary = new Itinerary
            {
                Title = dto.Title,
                Description = dto.Description,
                Theme = dto.Theme,
                GeographicArea = dto.GeographicArea,
                // Always start as DRAFT; stops are added in a subsequent step.
                Status = ItineraryStatus.DRAFT
            };

            var saved = await _itineraryRepository.AddAsync(itinerary, cancellationToken);
            return new ItineraryDTO(saved);
        }

        public async Task<ItineraryDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var itinerary = await _itineraryRepository.GetByIdAsync(id, cancellationToken);
            return itinerary is null ? null : new ItineraryDTO(itinerary);
        }
    }
}
