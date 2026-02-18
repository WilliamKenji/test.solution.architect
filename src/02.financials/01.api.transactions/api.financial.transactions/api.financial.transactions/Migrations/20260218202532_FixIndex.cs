using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.financial.transactions.Migrations
{
    /// <inheritdoc />
    public partial class FixIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_Data_Desc",
                table: "Lancamentos",
                columns: new[] { "Data", "Id" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_Data_Desc",
                table: "Lancamentos");
        }
    }
}
