using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBooleanFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                values: new object[] { "fe59eb5a-ec29-4533-af95-d7f1cc74abff", "AQAAAAIAAYagAAAAEHqmlIMvgJKYMFYdk+AHOeDh8Zya+tO0zfAe5nfwQH8abGu07+YRoO5o/Qq3O43pfQ==", "7dd559d3-e9e8-44e3-8995-cf3291b2153b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                values: new object[] { "09521874-a937-4eb9-887c-725c6b08dd1a", "AQAAAAIAAYagAAAAEJpr7MYj/NW2VdCa+ApEoxLp1XftboEzUquMCNyJge1LeEHC1sv7bocvkKLkM6XXtQ==", "ee686289-3cfb-4047-a115-3978918c7c61" });
        }
    }
}
