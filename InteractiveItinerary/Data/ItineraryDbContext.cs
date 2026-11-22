using InteractiveItinerary.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractiveItinerary.Data
{
    public class ItineraryDbContext : DbContext
    {
        public ItineraryDbContext(DbContextOptions<ItineraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Itinerary> Itineraries => Set<Itinerary>();
        public DbSet<Stop> Stops => Set<Stop>();
        public DbSet<ItineraryStop> ItineraryStops => Set<ItineraryStop>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ItineraryDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyPersistenceRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyPersistenceRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyPersistenceRules()
        {
            ChangeTracker.DetectChanges();

            var joinEntries = ChangeTracker.Entries<ItineraryStop>()
                .Where(e => e.State != EntityState.Deleted && e.State != EntityState.Detached)
                .ToList();

            // Itineraries whose stop list changed (join rows added/removed) count as updated too.
            var itinerariesWithStopChanges = ChangeTracker.Entries<ItineraryStop>()
                .Where(e => e.State is EntityState.Added or EntityState.Deleted)
                .Select(e => e.Property(j => j.ItineraryId).CurrentValue)
                .ToHashSet();

            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<Itinerary>())
            {
                if (entry.State is EntityState.Deleted or EntityState.Detached)
                    continue;

                var itineraryId = entry.Property(i => i.Id).CurrentValue;

                SyncStopPositions(entry.Entity, itineraryId, joinEntries);

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Property(i => i.CreatedAt).IsModified = false;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Unchanged when itinerariesWithStopChanges.Contains(itineraryId):
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }
        }

        /// <summary>
        /// Copies the index of each stop in <see cref="Itinerary.Stops"/> into the join row's Position,
        /// so the order of the list is what gets persisted.
        /// Key values are read from the change tracker so temporary keys of new entities also match.
        /// </summary>
        private void SyncStopPositions(
            Itinerary itinerary,
            int itineraryId,
            List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ItineraryStop>> joinEntries)
        {
            for (var position = 0; position < itinerary.Stops.Count; position++)
            {
                var stopId = Entry(itinerary.Stops[position]).Property(s => s.Id).CurrentValue;

                var join = joinEntries.FirstOrDefault(j =>
                    j.Property(x => x.ItineraryId).CurrentValue == itineraryId &&
                    j.Property(x => x.StopId).CurrentValue == stopId);

                if (join != null && join.Entity.Position != position)
                    join.Entity.Position = position;
            }
        }
    }
}
