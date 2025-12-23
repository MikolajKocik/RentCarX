using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomers_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers");

            migrationBuilder.DropIndex(
                name: "IX_StripeCustomers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "RentCarX",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "RentCarX",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "RentCarX",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "DeletedAt", "IsDeleted", "PasswordHash", "SecurityStamp" },
                values: new object[] { "09521874-a937-4eb9-887c-725c6b08dd1a", null, false, "AQAAAAIAAYagAAAAEJpr7MYj/NW2VdCa+ApEoxLp1XftboEzUquMCNyJge1LeEHC1sv7bocvkKLkM6XXtQ==", "ee686289-3cfb-4047-a115-3978918c7c61" });

            migrationBuilder.CreateIndex(
                name: "IX_StripeCustomers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "Payments",
                column: "UserId",
                principalSchema: "RentCarX",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomers_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                column: "UserId",
                principalSchema: "RentCarX",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomers_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers");

            migrationBuilder.DropIndex(
                name: "IX_StripeCustomers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "RentCarX",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "RentCarX",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "RentCarX",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "RentCarX",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99000000-0000-0000-0000-00000000dddd"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3a525052-b787-4dd4-8d73-95a5926f2334", "AQAAAAIAAYagAAAAEDTbni+tdycE/W7nanDCMl1MpasyLGI42eSkQ+FJ294Gxwh80HfV/I4e6UYXPhas/A==", "53d8ad09-cb15-460b-b80f-37f4b76377e5" });

            migrationBuilder.CreateIndex(
                name: "IX_StripeCustomers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "Payments",
                column: "UserId",
                principalSchema: "RentCarX",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomers_AspNetUsers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                column: "UserId",
                principalSchema: "RentCarX",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
