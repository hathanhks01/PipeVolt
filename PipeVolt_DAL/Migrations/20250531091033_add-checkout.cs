using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class addcheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "SALES_ORDER");

            migrationBuilder.AddColumn<int>(
                name: "payment_method_id",
                table: "SALES_ORDER",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "INVOICE",
                columns: table => new
                {
                    invoice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    invoice_template = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    issue_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    subtotal = table.Column<double>(type: "float", nullable: false),
                    vat_rate = table.Column<double>(type: "float", nullable: false),
                    vat_amount = table.Column<double>(type: "float", nullable: false),
                    total_amount = table.Column<double>(type: "float", nullable: false),
                    discount_amount = table.Column<double>(type: "float", nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    customer_address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    customer_tax_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    customer_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVOICE__ID", x => x.invoice_id);
                    table.ForeignKey(
                        name: "FK_INVOICE_CUSTOMER_customer_id",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK_INVOICE_EMPLOYEE_employee_id",
                        column: x => x.employee_id,
                        principalTable: "EMPLOYEE",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_INVOICE_SALES_ORDER_order_id",
                        column: x => x.order_id,
                        principalTable: "SALES_ORDER",
                        principalColumn: "order_id");
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT_METHOD",
                columns: table => new
                {
                    payment_method_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    method_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_online = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PAYMENT_METHOD__E8C9B7A3", x => x.payment_method_id);
                });

            migrationBuilder.CreateTable(
                name: "INVOICE_DETAIL",
                columns: table => new
                {
                    invoice_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    product_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<double>(type: "float", nullable: false),
                    discount = table.Column<double>(type: "float", nullable: true),
                    line_total = table.Column<double>(type: "float", nullable: true, computedColumnSql: "(([quantity]*[unit_price])-isnull([discount],0))", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVOICE_DETAIL__ID", x => x.invoice_detail_id);
                    table.ForeignKey(
                        name: "FK_INVOICE_DETAIL_INVOICE_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "INVOICE",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INVOICE_DETAIL_PRODUCT_product_id",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT_TRANSACTION",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    payment_method_id = table.Column<int>(type: "int", nullable: false),
                    transaction_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    amount = table.Column<double>(type: "float", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gateway_response = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PAYMENT_TRANSACTION__A1C3D8F2", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK__PAYMENT_TRANS__method__7B573F34",
                        column: x => x.payment_method_id,
                        principalTable: "PAYMENT_METHOD",
                        principalColumn: "payment_method_id");
                    table.ForeignKey(
                        name: "FK__PAYMENT_TRANS__order__7A672E12",
                        column: x => x.order_id,
                        principalTable: "SALES_ORDER",
                        principalColumn: "order_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_payment_method_id",
                table: "SALES_ORDER",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVOICE_customer_id",
                table: "INVOICE",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVOICE_employee_id",
                table: "INVOICE",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVOICE_order_id",
                table: "INVOICE",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "UQ__INVOICE__number",
                table: "INVOICE",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INVOICE_DETAIL_invoice_id",
                table: "INVOICE_DETAIL",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVOICE_DETAIL_product_id",
                table: "INVOICE_DETAIL",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_TRANSACTION_order_id",
                table: "PAYMENT_TRANSACTION",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_TRANSACTION_payment_method_id",
                table: "PAYMENT_TRANSACTION",
                column: "payment_method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_SALES_ORDER_PAYMENT_METHOD_payment_method_id",
                table: "SALES_ORDER",
                column: "payment_method_id",
                principalTable: "PAYMENT_METHOD",
                principalColumn: "payment_method_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SALES_ORDER_PAYMENT_METHOD_payment_method_id",
                table: "SALES_ORDER");

            migrationBuilder.DropTable(
                name: "INVOICE_DETAIL");

            migrationBuilder.DropTable(
                name: "PAYMENT_TRANSACTION");

            migrationBuilder.DropTable(
                name: "INVOICE");

            migrationBuilder.DropTable(
                name: "PAYMENT_METHOD");

            migrationBuilder.DropIndex(
                name: "IX_SALES_ORDER_payment_method_id",
                table: "SALES_ORDER");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "SALES_ORDER");

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "SALES_ORDER",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
