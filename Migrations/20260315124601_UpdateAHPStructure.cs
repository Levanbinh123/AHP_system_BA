using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSSStudentRisk.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAHPStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AHPAlternativeWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CriteriaName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    A1 = table.Column<double>(type: "double", nullable: false),
                    A2 = table.Column<double>(type: "double", nullable: false),
                    A3 = table.Column<double>(type: "double", nullable: false),
                    AHPCriteriaId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AHPAlternativeWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AHPAlternativeWeights_AHPCriteria_AHPCriteriaId",
                        column: x => x.AHPCriteriaId,
                        principalTable: "AHPCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AHPFinalResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    A1 = table.Column<double>(type: "double", nullable: false),
                    A2 = table.Column<double>(type: "double", nullable: false),
                    A3 = table.Column<double>(type: "double", nullable: false),
                    BestAlternative = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AHPCriteriaId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AHPFinalResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AHPFinalResults_AHPCriteria_AHPCriteriaId",
                        column: x => x.AHPCriteriaId,
                        principalTable: "AHPCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPerformances_StudentId",
                table: "StudentPerformances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskResults_StudentId",
                table: "RiskResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AHPAlternativeWeights_AHPCriteriaId",
                table: "AHPAlternativeWeights",
                column: "AHPCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AHPFinalResults_AHPCriteriaId",
                table: "AHPFinalResults",
                column: "AHPCriteriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskResults_Students_StudentId",
                table: "RiskResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPerformances_Students_StudentId",
                table: "StudentPerformances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskResults_Students_StudentId",
                table: "RiskResults");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentPerformances_Students_StudentId",
                table: "StudentPerformances");

            migrationBuilder.DropTable(
                name: "AHPAlternativeWeights");

            migrationBuilder.DropTable(
                name: "AHPFinalResults");

            migrationBuilder.DropIndex(
                name: "IX_StudentPerformances_StudentId",
                table: "StudentPerformances");

            migrationBuilder.DropIndex(
                name: "IX_RiskResults_StudentId",
                table: "RiskResults");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "Students");
        }
    }
}
