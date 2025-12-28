using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupCarAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                schema: "RentCarX",
                table: "Cars");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "30081065-6861-4522-a104-73e0ea012f1d", "AQAAAAIAAYagAAAAEEaIlL6oAIFYsTYNKUh6Q4l5lVXPOq22776OEbSDfGBDQRfmOG1JW21meZHi+bUgmQ==", "bbee7635-d437-465d-8c36-0c1b97759ca6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                schema: "RentCarX",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3ca2f8b3-eeda-45f5-a931-16b859fbf358", "AQAAAAIAAYagAAAAEOvXHbRxBADxGX835dvoWwH3nj/NV9ZldbajO0aMpTf1RqY24TS6w2UsliLajXpHmQ==", "2954d99d-bade-47c5-b925-dc80ff7e3473" });
        }
    }
}
