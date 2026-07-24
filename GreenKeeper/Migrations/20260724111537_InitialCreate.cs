using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenKeeper.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Care = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCaredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextDueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IntervalUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    IntervalAmount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareSchedules_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SunlightRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Hours = table.Column<int>(type: "INTEGER", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SunlightRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SunlightRequirements_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareSchedules_PlantId_Care",
                table: "CareSchedules",
                columns: new[] { "PlantId", "Care" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SunlightRequirements_PlantId",
                table: "SunlightRequirements",
                column: "PlantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareSchedules");

            migrationBuilder.DropTable(
                name: "SunlightRequirements");

            migrationBuilder.DropTable(
                name: "Plants");
        }
    }
}
