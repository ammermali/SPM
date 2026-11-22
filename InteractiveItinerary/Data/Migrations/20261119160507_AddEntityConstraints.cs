using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveItinerary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Stops_Address_NotEmpty",
                table: "Stops",
                sql: "length(trim(\"Address\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stops_Latitude",
                table: "Stops",
                sql: "\"Latitude\" BETWEEN -90 AND 90");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stops_Longitude",
                table: "Stops",
                sql: "\"Longitude\" BETWEEN -180 AND 180");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stops_Name_NotEmpty",
                table: "Stops",
                sql: "length(trim(\"Name\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItineraryStops_Position",
                table: "ItineraryStops",
                sql: "\"Position\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Itineraries_Status",
                table: "Itineraries",
                sql: "\"Status\" IN ('DRAFT', 'PUBLISHED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Itineraries_UpdatedAt",
                table: "Itineraries",
                sql: "\"UpdatedAt\" >= \"CreatedAt\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Stops_Address_NotEmpty",
                table: "Stops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stops_Latitude",
                table: "Stops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stops_Longitude",
                table: "Stops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stops_Name_NotEmpty",
                table: "Stops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItineraryStops_Position",
                table: "ItineraryStops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Itineraries_Status",
                table: "Itineraries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Itineraries_UpdatedAt",
                table: "Itineraries");
        }
    }
}
