using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class SomeRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationEntries_ReservationId",
                table: "ReservationEntries");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Reservations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<int>(
                name: "EntryNr",
                table: "ReservationEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries",
                columns: new[] { "ReservationId", "EntryNr" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEntries_ReservationSlotId",
                table: "ReservationEntries",
                column: "ReservationSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationEntries_ReservationSlotId",
                table: "ReservationEntries");

            migrationBuilder.DropColumn(
                name: "EntryNr",
                table: "ReservationEntries");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Reservations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationEntries",
                table: "ReservationEntries",
                column: "ReservationSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationEntries_ReservationId",
                table: "ReservationEntries",
                column: "ReservationId");
        }
    }
}
