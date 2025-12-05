using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StripeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RentCarX");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "Reservations",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "IdentityUserLogin<string>",
                newName: "IdentityUserLogin<string>",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "Cars",
                newName: "Cars",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "RentCarX");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "RentCarX");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                schema: "RentCarX",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PricePerDay",
                schema: "RentCarX",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "RentCarX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StripeCustomers",
                schema: "RentCarX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StripeCustomers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "RentCarX",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "RentCarX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StripeCheckoutSessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SucceededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "RentCarX",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "RentCarX",
                        principalTable: "Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "RentCarX",
                        principalTable: "Reservations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                schema: "RentCarX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripeRefundId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StripeChargeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SucceededAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "RentCarX",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ItemId",
                schema: "RentCarX",
                table: "Payments",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReservationId",
                schema: "RentCarX",
                table: "Payments",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                schema: "RentCarX",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentId",
                schema: "RentCarX",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_StripeCustomers_UserId",
                schema: "RentCarX",
                table: "StripeCustomers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Refunds",
                schema: "RentCarX");

            migrationBuilder.DropTable(
                name: "StripeCustomers",
                schema: "RentCarX");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "RentCarX");

            migrationBuilder.DropTable(
                name: "Items",
                schema: "RentCarX");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                schema: "RentCarX",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                schema: "RentCarX",
                table: "Reservations");

            migrationBuilder.RenameTable(
                name: "Reservations",
                schema: "RentCarX",
                newName: "Reservations");

            migrationBuilder.RenameTable(
                name: "IdentityUserLogin<string>",
                schema: "RentCarX",
                newName: "IdentityUserLogin<string>");

            migrationBuilder.RenameTable(
                name: "Cars",
                schema: "RentCarX",
                newName: "Cars");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "RentCarX",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "RentCarX",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "RentCarX",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "RentCarX",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "RentCarX",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "RentCarX",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "RentCarX",
                newName: "AspNetRoleClaims");
        }
    }
}
