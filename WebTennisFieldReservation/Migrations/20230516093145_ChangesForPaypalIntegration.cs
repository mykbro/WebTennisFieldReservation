using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class ChangesForPaypalIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationEmailSent",
                table: "Reservations");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentConfirmationToken",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Reservations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentConfirmationToken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmationEmailSent",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
