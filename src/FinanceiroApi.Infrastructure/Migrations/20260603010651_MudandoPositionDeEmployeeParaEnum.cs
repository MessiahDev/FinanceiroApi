using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MudandoPositionDeEmployeeParaEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Employees",
                type: "integer",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
