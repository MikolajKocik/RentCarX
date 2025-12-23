using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCarAvailabilityFLag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsAvailableFlag",
                schema: "RentCarX",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3a525052-b787-4dd4-8d73-95a5926f2334", "AQAAAAIAAYagAAAAEDTbni+tdycE/W7nanDCMl1MpasyLGI42eSkQ+FJ294Gxwh80HfV/I4e6UYXPhas/A==", "53d8ad09-cb15-460b-b80f-37f4b76377e5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailableFlag",
                schema: "RentCarX",
                table: "Cars");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "70ea13dd-705b-4233-9bcb-70b4cb8592c9", "AQAAAAIAAYagAAAAEBKc8vPS5RKFe1KTtr+Zpv2zH5KYEqrUxmsJiWl76hKbDST0M2FuqjEQDbMRkvfxkg==", "7ae87fb3-bc44-4506-9889-7c3abab51193" });
        }
    }
}
