using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationEntries_CourtId_Day_DaySlot",
                table: "ReservationEntries");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "ReservationEntries");

            migrationBuilder.DropColumn(
                name: "DaySlot",
                table: "ReservationEntries");

            migrationBuilder.RenameColumn(
                name: "ReservationEntryWeakId",
                table: "ReservationEntries",
                newName: "ReservationSlotId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "CourtId",
                table: "ReservationEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ReservationEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries",
                column: "ReservationSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEntries_CourtId",
                table: "ReservationEntries",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEntries_ReservationId",
                table: "ReservationEntries",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationEntries_ReservationsSlots_ReservationSlotId",
                table: "ReservationEntries",
                column: "ReservationSlotId",
                principalTable: "ReservationsSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationEntries_ReservationsSlots_ReservationSlotId",
                table: "ReservationEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationEntries_CourtId",
                table: "ReservationEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationEntries_ReservationId",
                table: "ReservationEntries");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ReservationEntries");

            migrationBuilder.RenameColumn(
                name: "ReservationSlotId",
                table: "ReservationEntries",
                newName: "ReservationEntryWeakId");

            migrationBuilder.AlterColumn<int>(
                name: "CourtId",
                table: "ReservationEntries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Day",
                table: "ReservationEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte>(
                name: "DaySlot",
                table: "ReservationEntries",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries",
                columns: new[] { "ReservationId", "ReservationEntryWeakId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEntries_CourtId_Day_DaySlot",
                table: "ReservationEntries",
                columns: new[] { "CourtId", "Day", "DaySlot" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
