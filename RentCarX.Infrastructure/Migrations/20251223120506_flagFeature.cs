using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class flagFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "70ea13dd-705b-4233-9bcb-70b4cb8592c9", "AQAAAAIAAYagAAAAEBKc8vPS5RKFe1KTtr+Zpv2zH5KYEqrUxmsJiWl76hKbDST0M2FuqjEQDbMRkvfxkg==", "7ae87fb3-bc44-4506-9889-7c3abab51193" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5c02c903-daa0-4909-aa38-6db47fef72c7", "AQAAAAIAAYagAAAAEGm7tJIPbjZG7YeTSlqm3mf5mL2bIBfohGo4g5qMsEp3cVqvIprCTnWhxPrbU3KNIw==", "44637aad-6597-4648-81a2-6ba3b72e62db" });
        }
    }
}
