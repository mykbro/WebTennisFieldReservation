using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class ReintroducedSurrogateKeyForReservationSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationsSlots",
                table: "ReservationsSlots");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ReservationsSlots",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationsSlots",
                table: "ReservationsSlots",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationsSlots_CourtId_Date_DaySlot",
                table: "ReservationsSlots",
                columns: new[] { "CourtId", "Date", "DaySlot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationsSlots",
                table: "ReservationsSlots");

            migrationBuilder.DropIndex(
                name: "IX_ReservationsSlots_CourtId_Date_DaySlot",
                table: "ReservationsSlots");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ReservationsSlots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationsSlots",
                table: "ReservationsSlots",
                columns: new[] { "CourtId", "Date", "DaySlot" });
        }
    }
}
