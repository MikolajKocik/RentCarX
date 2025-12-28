using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutUrlForPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                schema: "RentCarX",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3ca2f8b3-eeda-45f5-a931-16b859fbf358", "AQAAAAIAAYagAAAAEOvXHbRxBADxGX835dvoWwH3nj/NV9ZldbajO0aMpTf1RqY24TS6w2UsliLajXpHmQ==", "2954d99d-bade-47c5-b925-dc80ff7e3473" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                schema: "RentCarX",
                table: "Payments");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fe59eb5a-ec29-4533-af95-d7f1cc74abff", "AQAAAAIAAYagAAAAEHqmlIMvgJKYMFYdk+AHOeDh8Zya+tO0zfAe5nfwQH8abGu07+YRoO5o/Qq3O43pfQ==", "7dd559d3-e9e8-44e3-8995-cf3291b2153b" });
        }
    }
}
