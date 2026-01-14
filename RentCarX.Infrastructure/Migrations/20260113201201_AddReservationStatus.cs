using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "RentCarX",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a1a353b2-9340-47d8-9ea6-642635c65afb", "AQAAAAIAAYagAAAAENw2w5lop4WyfmWR2IJQfnT1wbf34FfNg5i0unsIhFHy9/G2GVu4a3pGygbnByr1EA==", "17e93d88-e03a-46b7-a4e1-bbe7663ad8cb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "RentCarX",
                table: "Reservations");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "01b3ca11-eeb1-4ddd-a202-80085ee8464f", "AQAAAAIAAYagAAAAEOHMVoVyftLNhFMhdX/4ncusm89DpjQXYBRjlkKRzEaMXlqIWb+pJArzNiIkEEdUzA==", "7cdbd1f6-6c5b-42e5-b937-18a4e36b2c10" });
        }
    }
}
