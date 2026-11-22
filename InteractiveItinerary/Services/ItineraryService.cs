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

        public async Task<ItineraryDTO> AddStopAsync(int itineraryId, int stopId, CancellationToken cancellationToken = default)
        {
            if (stopId <= 0)
                throw new ArgumentOutOfRangeException(nameof(stopId), "Stop ID must be a positive integer.");

            var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Itinerary with Id {itineraryId} does not exist.");

            if (itinerary.Status != ItineraryStatus.DRAFT)
            {
                throw new InvalidOperationException(
                    $"Cannot add stops to itinerary {itineraryId} because its status is {itinerary.Status}; only DRAFT itineraries can be modified.");
            }

            if (itinerary.Stops.Any(s => s.Id == stopId))
                throw new InvalidOperationException($"Stop with Id {stopId} is already in itinerary {itineraryId}.");

            // Pass a link-only stub; ItineraryRepository.ResolveStopsAsync verifies the stop exists and links it.
            itinerary.Stops.Add(new Stop { Id = stopId });
            await _itineraryRepository.UpdateAsync(itinerary, cancellationToken);
            return new ItineraryDTO(itinerary);
        }

        public async Task<StopDTO> AddStopAsync(int itineraryId, StopRequestDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidateStopRequest(dto);

            var itinerary = await LoadDraftItineraryAsync(itineraryId, cancellationToken);

            var stop = new Stop
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = new Location(dto.Latitude, dto.Longitude, dto.Address)
            };

            itinerary.Stops.Add(stop);
            await _itineraryRepository.UpdateAsync(itinerary, cancellationToken);
            return new StopDTO(stop);
        }

        public async Task<StopDTO> UpdateStopAsync(int itineraryId, int stopId, StopRequestDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidateStopRequest(dto);

            var itinerary = await LoadDraftItineraryAsync(itineraryId, cancellationToken);

            var stop = itinerary.Stops.FirstOrDefault(s => s.Id == stopId)
                ?? throw new KeyNotFoundException($"Stop with Id {stopId} is not part of itinerary {itineraryId}.");

            stop.Name = dto.Name;
            stop.Description = dto.Description;
            stop.Location = new Location(dto.Latitude, dto.Longitude, dto.Address);

            await _itineraryRepository.UpdateStopAsync(stop, cancellationToken);
            return new StopDTO(stop);
        }

        public async Task<IReadOnlyList<StopDTO>?> GetStopsAsync(int itineraryId, CancellationToken cancellationToken = default)
        {
            if (!await _itineraryRepository.ExistsAsync(itineraryId, cancellationToken))
                return null;

            var stops = await _itineraryRepository.GetStopsByItineraryIdAsync(itineraryId, cancellationToken);
            return stops.Select(s => new StopDTO(s)).ToList();
        }

        private async Task<Itinerary> LoadDraftItineraryAsync(int itineraryId, CancellationToken cancellationToken)
        {
            var itinerary = await _itineraryRepository.GetByIdAsync(itineraryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Itinerary with Id {itineraryId} does not exist.");

            if (itinerary.Status != ItineraryStatus.DRAFT)
            {
                throw new InvalidOperationException(
                    $"Cannot modify stops of itinerary {itineraryId} because its status is {itinerary.Status}; only DRAFT itineraries can be modified.");
            }

            return itinerary;
        }

        private static void ValidateStopRequest(StopRequestDTO dto)
        {
            if (double.IsNaN(dto.Latitude) || double.IsInfinity(dto.Latitude) || dto.Latitude < -90 || dto.Latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(dto), "Latitude must be between -90 and 90.");

            if (double.IsNaN(dto.Longitude) || double.IsInfinity(dto.Longitude) || dto.Longitude < -180 || dto.Longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(dto), "Longitude must be between -180 and 180.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Stop name is required.", nameof(dto));
            if (dto.Name.Length > 200)
                throw new ArgumentException("Stop name cannot exceed 200 characters.", nameof(dto));

            if (dto.Description is null)
                throw new ArgumentException("Stop description is required.", nameof(dto));
            if (dto.Description.Length > 2000)
                throw new ArgumentException("Stop description cannot exceed 2000 characters.", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Stop address is required.", nameof(dto));
            if (dto.Address.Length > 500)
                throw new ArgumentException("Stop address cannot exceed 500 characters.", nameof(dto));
        }
    }
}
