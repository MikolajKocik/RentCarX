using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBooleanIsNotifiedToPaymentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRefundNotified",
                schema: "RentCarX",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "673b5e41-f30d-4f7c-88d8-fb8552a48c3e", "AQAAAAIAAYagAAAAEMZgdDMgvUIynVkGQSPW9AouS37LgwE+reaC822b8OgFtY81OrmxpROAXTpZAwjoWA==", "35828b6a-b253-459f-9e88-5a529fb5bfa9" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRefundNotified",
                schema: "RentCarX",
                table: "Payments");

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "30081065-6861-4522-a104-73e0ea012f1d", "AQAAAAIAAYagAAAAEEaIlL6oAIFYsTYNKUh6Q4l5lVXPOq22776OEbSDfGBDQRfmOG1JW21meZHi+bUgmQ==", "bbee7635-d437-465d-8c36-0c1b97759ca6" });
        }
    }
}
