using InteractiveItinerary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InteractiveItinerary.Data.Configurations
{
    public class ItineraryConfiguration : IEntityTypeConfiguration<Itinerary>
    {
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        public void Configure(EntityTypeBuilder<Itinerary> builder)
        {
            builder.ToTable("Itineraries", table =>
            {
                table.HasCheckConstraint("CK_Itineraries_Status", "\"Status\" IN ('DRAFT', 'PUBLISHED')");
                table.HasCheckConstraint("CK_Itineraries_UpdatedAt", "\"UpdatedAt\" >= \"CreatedAt\"");
            });

            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedOnAdd();

            builder.Property(i => i.Title).HasMaxLength(200);
            builder.Property(i => i.Description).HasMaxLength(2000);
            builder.Property(i => i.Theme).HasMaxLength(200);
            builder.Property(i => i.GeographicArea).HasMaxLength(200);

            builder.Property(i => i.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // SQLite has no DateTime type (stored as TEXT) and loses DateTimeKind:
            // timestamps are always written in UTC, so mark them as UTC when read back.
            builder.Property(i => i.CreatedAt).IsRequired().HasConversion(UtcConverter);
            builder.Property(i => i.UpdatedAt).IsRequired().HasConversion(UtcConverter);

            builder.HasIndex(i => i.Status);

            // Many-to-many Itinerary <-> Stop through the ItineraryStop join entity (with payload: Position).
            builder.HasMany(i => i.Stops)
                .WithMany()
                .UsingEntity<ItineraryStop>(
                    right => right.HasOne<Stop>()
                        .WithMany()
                        .HasForeignKey(j => j.StopId)
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<Itinerary>()
                        .WithMany()
                        .HasForeignKey(j => j.ItineraryId)
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("ItineraryStops", table =>
                            table.HasCheckConstraint("CK_ItineraryStops_Position", "\"Position\" >= 0"));
                        join.HasKey(j => new { j.ItineraryId, j.StopId });
                        join.Property(j => j.Position).IsRequired();
                        // Not unique on purpose: reordering stops updates rows one at a time,
                        // and a unique (ItineraryId, Position) index would fail on swaps.
                        join.HasIndex(j => new { j.ItineraryId, j.Position });
                        join.HasIndex(j => j.StopId);
                    });
        }
    }
}
