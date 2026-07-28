using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallifyDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "HallServices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Halls",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "BookingServices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "HallServices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "BookingServices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Bookings");
        }
    }
}
