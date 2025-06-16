using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "WARRANTY",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "PURCHASE_ORDER",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "INVOICE",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "payment_status",
                table: "INVOICE",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "CHAT_ROOM",
                columns: table => new
                {
                    chat_room_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    room_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    last_message_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHAT_ROOM__ID", x => x.chat_room_id);
                    table.ForeignKey(
                        name: "FK__CHAT_ROOM__customer",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK__CHAT_ROOM__employee",
                        column: x => x.employee_id,
                        principalTable: "EMPLOYEE",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "CHAT_MESSAGE",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chat_room_id = table.Column<int>(type: "int", nullable: false),
                    sender_id = table.Column<int>(type: "int", nullable: false),
                    sender_type = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    message_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    attachment_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    read_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHAT_MESSAGE__ID", x => x.message_id);
                    table.ForeignKey(
                        name: "FK__CHAT_MESSAGE__room",
                        column: x => x.chat_room_id,
                        principalTable: "CHAT_ROOM",
                        principalColumn: "chat_room_id");
                });

            migrationBuilder.CreateTable(
                name: "CHAT_PARTICIPANT",
                columns: table => new
                {
                    participant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chat_room_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    user_type = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    left_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHAT_PARTICIPANT__ID", x => x.participant_id);
                    table.ForeignKey(
                        name: "FK__CHAT_PARTICIPANT__room",
                        column: x => x.chat_room_id,
                        principalTable: "CHAT_ROOM",
                        principalColumn: "chat_room_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_MESSAGE_IS_READ",
                table: "CHAT_MESSAGE",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_MESSAGE_ROOM_ID",
                table: "CHAT_MESSAGE",
                column: "chat_room_id");

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_MESSAGE_SENT_AT",
                table: "CHAT_MESSAGE",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "UQ__CHAT_PARTICIPANT__ROOM_USER",
                table: "CHAT_PARTICIPANT",
                columns: new[] { "chat_room_id", "user_id", "user_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_ROOM_customer_id",
                table: "CHAT_ROOM",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_ROOM_employee_id",
                table: "CHAT_ROOM",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHAT_MESSAGE");

            migrationBuilder.DropTable(
                name: "CHAT_PARTICIPANT");

            migrationBuilder.DropTable(
                name: "CHAT_ROOM");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "WARRANTY",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "PURCHASE_ORDER",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "INVOICE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "INVOICE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);
        }
    }
}
