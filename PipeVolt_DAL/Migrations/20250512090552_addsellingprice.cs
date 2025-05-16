using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class addsellingprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "selling_price",
                table: "PRODUCT",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "selling_price",
                table: "PRODUCT");
        }
    }
}
