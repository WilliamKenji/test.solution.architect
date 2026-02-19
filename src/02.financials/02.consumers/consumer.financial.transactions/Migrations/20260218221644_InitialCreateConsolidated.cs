using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace consumer.financial.transactions.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateConsolidated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsolidatedDailies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateTime>(type: "date", nullable: false),
                    TotalCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Versao = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidatedDailies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidatedDaily_Data_Unique",
                table: "ConsolidatedDailies",
                column: "Data",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsolidatedDailies");
        }
    }
}
