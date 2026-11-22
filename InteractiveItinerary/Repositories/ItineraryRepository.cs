using InteractiveItinerary.Data;
using InteractiveItinerary.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Repositories
{
    public class ItineraryRepository : IItineraryRepository
    {
        private readonly ItineraryDbContext _context;

        public ItineraryRepository(ItineraryDbContext context)
        {
            _context = context;
        }

        public async Task<Itinerary?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var itinerary = await _context.Itineraries
                .Include(i => i.Stops)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (itinerary != null)
                await OrderStopsAsync(new[] { itinerary }, cancellationToken);

            return itinerary;
        }

        public Task<IReadOnlyList<Itinerary>> GetAllAsync(CancellationToken cancellationToken = default)
            => QueryReadOnlyAsync(_context.Itineraries, cancellationToken);

        public Task<IReadOnlyList<Itinerary>> GetByStatusAsync(ItineraryStatus status, CancellationToken cancellationToken = default)
            => QueryReadOnlyAsync(_context.Itineraries.Where(i => i.Status == status), cancellationToken);

        public async Task<Itinerary> AddAsync(Itinerary itinerary, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itinerary);

            var resolvedStops = await ResolveStopsAsync(itinerary.Stops, cancellationToken);
            itinerary.Stops.Clear();
            itinerary.Stops.AddRange(resolvedStops);

            _context.Itineraries.Add(itinerary);
            await _context.SaveChangesAsync(cancellationToken);
            return itinerary;
        }

        public async Task<bool> UpdateAsync(Itinerary itinerary, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itinerary);

            // Snapshot first: 'itinerary' may be the same tracked instance returned by GetByIdAsync.
            var incomingStops = itinerary.Stops.ToList();

            var existing = await _context.Itineraries
                .Include(i => i.Stops)
                .FirstOrDefaultAsync(i => i.Id == itinerary.Id, cancellationToken);

            if (existing == null)
                return false;

            existing.Title = itinerary.Title;
            existing.Description = itinerary.Description;
            existing.Theme = itinerary.Theme;
            existing.Status = itinerary.Status;
            existing.GeographicArea = itinerary.GeographicArea;
            // CreatedAt / UpdatedAt are managed by ItineraryDbContext.

            var resolvedStops = await ResolveStopsAsync(incomingStops, cancellationToken);
            existing.Stops.Clear();
            existing.Stops.AddRange(resolvedStops);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Itineraries.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null)
                return false;

            // ItineraryStops rows are removed by the cascade delete; Stops are shared and kept.
            _context.Itineraries.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
            => _context.Itineraries.AnyAsync(i => i.Id == id, cancellationToken);

        private async Task<IReadOnlyList<Itinerary>> QueryReadOnlyAsync(
            IQueryable<Itinerary> query, CancellationToken cancellationToken)
        {
            var itineraries = await query
                .AsNoTracking()
                .Include(i => i.Stops)
                .OrderBy(i => i.Id)
                .ToListAsync(cancellationToken);

            await OrderStopsAsync(itineraries, cancellationToken);
            return itineraries;
        }

        /// <summary>
        /// Sorts each itinerary's Stops by the Position stored in the join table
        /// (EF cannot order a skip navigation by a join-entity column inside Include).
        /// </summary>
        private async Task OrderStopsAsync(IReadOnlyCollection<Itinerary> itineraries, CancellationToken cancellationToken)
        {
            if (itineraries.Count == 0)
                return;

            var ids = itineraries.Select(i => i.Id).ToList();

            var positions = await _context.ItineraryStops
                .AsNoTracking()
                .Where(j => ids.Contains(j.ItineraryId))
                .Select(j => new { j.ItineraryId, j.StopId, j.Position })
                .ToListAsync(cancellationToken);

            var lookup = positions.ToDictionary(p => (p.ItineraryId, p.StopId), p => p.Position);

            foreach (var itinerary in itineraries)
            {
                itinerary.Stops.Sort((a, b) =>
                    lookup.GetValueOrDefault((itinerary.Id, a.Id), int.MaxValue)
                        .CompareTo(lookup.GetValueOrDefault((itinerary.Id, b.Id), int.MaxValue)));
            }
        }

        /// <summary>
        /// Maps incoming stops to tracked instances: new stops (Id == 0) are kept as-is and will be inserted;
        /// existing stops are loaded from the database and their values updated, so they are linked, not duplicated.
        /// </summary>
        private async Task<List<Stop>> ResolveStopsAsync(IEnumerable<Stop> stops, CancellationToken cancellationToken)
        {
            var incoming = stops.ToList();

            if (incoming.Select(s => s.Id).Where(id => id != 0).GroupBy(id => id).Any(g => g.Count() > 1)
                || incoming.Where(s => s.Id == 0).Distinct().Count() != incoming.Count(s => s.Id == 0))
            {
                throw new InvalidOperationException("The same stop cannot appear more than once in an itinerary.");
            }

            var existingIds = incoming.Where(s => s.Id != 0).Select(s => s.Id).ToList();

            var existingStops = existingIds.Count == 0
                ? new Dictionary<int, Stop>()
                : await _context.Stops
                    .Where(s => existingIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, cancellationToken);

            var result = new List<Stop>(incoming.Count);
            foreach (var stop in incoming)
            {
                if (stop.Id == 0)
                {
                    result.Add(stop);
                    continue;
                }

                if (!existingStops.TryGetValue(stop.Id, out var tracked))
                    throw new KeyNotFoundException($"Stop with Id {stop.Id} does not exist.");

                var isLinkOnlyStub = string.IsNullOrEmpty(stop.Name)
                    && string.IsNullOrEmpty(stop.Description)
                    && stop.Location.Address is null;

                if (!ReferenceEquals(tracked, stop) && !isLinkOnlyStub)
                {
                    tracked.Name = stop.Name;
                    tracked.Description = stop.Description;
                    tracked.Location = stop.Location;
                }

                result.Add(tracked);
            }

            return result;
        }
    }
}
