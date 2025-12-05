using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                schema: "RentCarX",
                table: "Cars");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "RentCarX",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "RentCarX",
                table: "Reservations");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                schema: "RentCarX",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
