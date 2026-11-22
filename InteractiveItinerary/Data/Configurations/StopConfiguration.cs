using InteractiveItinerary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InteractiveItinerary.Data.Configurations
{
    public class StopConfiguration : IEntityTypeConfiguration<Stop>
    {
        public void Configure(EntityTypeBuilder<Stop> builder)
        {
            // Location no longer validates in its constructor, so the database is the last line of defence.
            builder.ToTable("Stops", table =>
            {
                table.HasCheckConstraint("CK_Stops_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                table.HasCheckConstraint("CK_Stops_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
                table.HasCheckConstraint("CK_Stops_Name_NotEmpty", "length(trim(\"Name\")) > 0");
                table.HasCheckConstraint("CK_Stops_Address_NotEmpty", "length(trim(\"Address\")) > 0");
            });

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
            builder.Property(s => s.Description).HasMaxLength(2000).IsRequired();

            // Location is an immutable value object: stored inline in the Stops table.
            builder.ComplexProperty(s => s.Location, location =>
            {
                location.Property(l => l.Latitude).HasColumnName("Latitude").IsRequired();
                location.Property(l => l.Longitude).HasColumnName("Longitude").IsRequired();
                location.Property(l => l.Address).HasColumnName("Address").HasMaxLength(500).IsRequired();
            });
        }
    }
}
