using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    /// <inheritdoc />
    public partial class TemplateChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_CourtsAvailabilityTemplates_TemplateId",
                table: "Courts");

            migrationBuilder.DropTable(
                name: "CourtAvailabilityOverrides");

            migrationBuilder.DropTable(
                name: "CourtAvailabilityTemplateEntries");

            migrationBuilder.DropTable(
                name: "CourtsAvailabilityTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Courts_TemplateId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Courts");

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateEntries",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    WeekSlot = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateEntries", x => new { x.TemplateId, x.WeekSlot });
                    table.ForeignKey(
                        name: "FK_TemplateEntries_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Name",
                table: "Templates",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateEntries");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Courts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CourtAvailabilityOverrides",
                columns: table => new
                {
                    CourtId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaySlot = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtAvailabilityOverrides", x => new { x.CourtId, x.Day, x.DaySlot });
                    table.ForeignKey(
                        name: "FK_CourtAvailabilityOverrides_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourtsAvailabilityTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtsAvailabilityTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourtAvailabilityTemplateEntries",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    WeekDay = table.Column<byte>(type: "tinyint", nullable: false),
                    DaySlot = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtAvailabilityTemplateEntries", x => new { x.TemplateId, x.WeekDay, x.DaySlot });
                    table.ForeignKey(
                        name: "FK_CourtAvailabilityTemplateEntries_CourtsAvailabilityTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CourtsAvailabilityTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courts_TemplateId",
                table: "Courts",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CourtsAvailabilityTemplates_Name",
                table: "CourtsAvailabilityTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_CourtsAvailabilityTemplates_TemplateId",
                table: "Courts",
                column: "TemplateId",
                principalTable: "CourtsAvailabilityTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
