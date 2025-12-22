using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "RentCarX",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("99000000-0000-0000-0000-00000000aaaa"), null, "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                schema: "RentCarX",
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("99000000-0000-0000-0000-00000000dddd"), 0, "5c02c903-daa0-4909-aa38-6db47fef72c7", "admin@rentcarx.com", true, false, null, "ADMIN@RENTCARX.COM", "ADMIN@RENTCARX.COM", "AQAAAAIAAYagAAAAEGm7tJIPbjZG7YeTSlqm3mf5mL2bIBfohGo4g5qMsEp3cVqvIprCTnWhxPrbU3KNIw==", null, false, "44637aad-6597-4648-81a2-6ba3b72e62db", false, "admin@rentcarx.com" });

            migrationBuilder.InsertData(
                schema: "RentCarX",
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("99000000-0000-0000-0000-00000000aaaa"), new Guid("99000000-0000-0000-0000-00000000dddd") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "RentCarX",
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("99000000-0000-0000-0000-00000000aaaa"), new Guid("99000000-0000-0000-0000-00000000dddd") });

            migrationBuilder.DeleteData(
                schema: "RentCarX",
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000aaaa"));

            migrationBuilder.DeleteData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"));
        }
    }
}
