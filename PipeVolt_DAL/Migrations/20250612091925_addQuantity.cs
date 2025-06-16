using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class addQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "PRODUCT",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantity",
                table: "PRODUCT");
        }
    }
}
