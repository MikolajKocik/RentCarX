using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "RentCarX",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "01b3ca11-eeb1-4ddd-a202-80085ee8464f", "AQAAAAIAAYagAAAAEOHMVoVyftLNhFMhdX/4ncusm89DpjQXYBRjlkKRzEaMXlqIWb+pJArzNiIkEEdUzA==", "7cdbd1f6-6c5b-42e5-b937-18a4e36b2c10" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "RentCarX",
                table: "Cars");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "673b5e41-f30d-4f7c-88d8-fb8552a48c3e", "AQAAAAIAAYagAAAAEMZgdDMgvUIynVkGQSPW9AouS37LgwE+reaC822b8OgFtY81OrmxpROAXTpZAwjoWA==", "35828b6a-b253-459f-9e88-5a529fb5bfa9" });
        }
    }
}
