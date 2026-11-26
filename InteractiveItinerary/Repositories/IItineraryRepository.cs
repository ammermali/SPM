using InteractiveItinerary.Entities;

namespace InteractiveItinerary.Repositories
{
    /// <summary>
    /// Data access for <see cref="Itinerary"/> aggregates (itinerary + its ordered stops).
    /// </summary>
    public interface IItineraryRepository
    {
        /// <summary>Returns the itinerary with its stops (ordered), or <c>null</c> if not found. The result is tracked.</summary>
        Task<Itinerary?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Returns all itineraries with their ordered stops (read-only, not tracked).</summary>
        Task<IReadOnlyList<Itinerary>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns the itineraries with the given status, with their ordered stops (read-only, not tracked).</summary>
        Task<IReadOnlyList<Itinerary>> GetByStatusAsync(ItineraryStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a new itinerary. Stops with <c>Id == 0</c> are created; stops with an existing
        /// <c>Id</c> are linked (shared) without being duplicated. Returns the entity with its generated Id.
        /// </summary>
        Task<Itinerary> AddAsync(Itinerary itinerary, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing itinerary and replaces its stop list (order included).
        /// Returns <c>false</c> if the itinerary does not exist.
        /// </summary>
        Task<bool> UpdateAsync(Itinerary itinerary, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the itinerary and its stop links (shared stops are kept).
        /// Returns <c>false</c> if the itinerary does not exist.
        /// </summary>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Returns the stops associated with an itinerary in route <c>Position</c> order (read-only).</summary>
        Task<IReadOnlyList<Stop>> GetStopsByItineraryIdAsync(int itineraryId, CancellationToken cancellationToken = default);

        /// <summary>Returns a single stop by its <paramref name="stopId"/>, or <c>null</c> if not found (read-only).</summary>
        Task<Stop?> GetStopByIdAsync(int stopId, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing stop's fields and location. Returns <c>false</c> if not found.</summary>
        Task<bool> UpdateStopAsync(Stop stop, CancellationToken cancellationToken = default);

        /// <summary>Defines the repository contract to unlink a specific stop from an itinerary and shift subsequent stop positions down by one. </summary>
        Task<bool> RemoveStopAssociationAsync(int itineraryId, int stopId, CancellationToken cancellationToken = default);
    }
}
