using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class thaydoirangbuoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_ACCOUNT_employee_id",
                table: "USER_ACCOUNT");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_employee_id",
                table: "USER_ACCOUNT",
                column: "employee_id",
                unique: true,
                filter: "[employee_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_ACCOUNT_employee_id",
                table: "USER_ACCOUNT");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_employee_id",
                table: "USER_ACCOUNT",
                column: "employee_id");
        }
    }
}
