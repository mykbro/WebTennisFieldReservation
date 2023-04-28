using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class WithAllNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CourtId",
                table: "CourtAvailabilityTemplateEntries",
                newName: "TemplateId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CourtsAvailabilityTemplates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourtAvailabilityOverrides_Courts_CourtId",
                table: "CourtAvailabilityOverrides",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourtAvailabilityTemplateEntries_CourtsAvailabilityTemplates_TemplateId",
                table: "CourtAvailabilityTemplateEntries",
                column: "TemplateId",
                principalTable: "CourtsAvailabilityTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourtAvailabilityOverrides_Courts_CourtId",
                table: "CourtAvailabilityOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_CourtAvailabilityTemplateEntries_CourtsAvailabilityTemplates_TemplateId",
                table: "CourtAvailabilityTemplateEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationEntries_Courts_CourtId",
                table: "ReservationEntries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CourtsAvailabilityTemplates");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "CourtAvailabilityTemplateEntries",
                newName: "CourtId");
        }
    }
}
