using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipeVolt_DAL.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BRAND",
                columns: table => new
                {
                    brand_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BRAND__5E5A8E2748CDEFCA", x => x.brand_id);
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    registration_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CUSTOMER__CD65CB85C2818D2A", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "EMPLOYEE",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    employee_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    hire_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EMPLOYEE__C52E0BA83BED5CBA", x => x.employee_id);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_CATEGORY",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PRODUCT___D54EE9B4968BF011", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "SUPPLIER",
                columns: table => new
                {
                    supplier_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    contact_person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SUPPLIER__6EE594E812C79687", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "WAREHOUSE",
                columns: table => new
                {
                    warehouse_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    warehouse_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WAREHOUS__734FE6BFE44CB61E", x => x.warehouse_id);
                });

            migrationBuilder.CreateTable(
                name: "CART",
                columns: table => new
                {
                    cart_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CART", x => x.cart_id);
                    table.ForeignKey(
                        name: "FK_CART_CUSTOMER_customer_id",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SALES_ORDER",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    order_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    total_amount = table.Column<double>(type: "float", nullable: true),
                    discount_amount = table.Column<double>(type: "float", nullable: true),
                    tax_amount = table.Column<double>(type: "float", nullable: true),
                    net_amount = table.Column<double>(type: "float", nullable: true, computedColumnSql: "(([total_amount]-[discount_amount])+[tax_amount])", stored: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SALES_OR__4659622993289FF9", x => x.order_id);
                    table.ForeignKey(
                        name: "FK__SALES_ORD__custo__6477ECF3",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK__SALES_ORD__emplo__656C112C",
                        column: x => x.employee_id,
                        principalTable: "EMPLOYEE",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNT",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    user_type = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USER_ACC__B9BE370FE3A6149D", x => x.user_id);
                    table.ForeignKey(
                        name: "FK__USER_ACCO__custo__5FB337D6",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK__USER_ACCO__emplo__5EBF139D",
                        column: x => x.employee_id,
                        principalTable: "EMPLOYEE",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    brand_id = table.Column<int>(type: "int", nullable: true),
                    selling_price = table.Column<double>(type: "float", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PRODUCT__47027DF521C9A1DD", x => x.product_id);
                    table.ForeignKey(
                        name: "FK__PRODUCT__brand_i__440B1D61",
                        column: x => x.brand_id,
                        principalTable: "BRAND",
                        principalColumn: "brand_id");
                    table.ForeignKey(
                        name: "FK__PRODUCT__categor__4316F928",
                        column: x => x.category_id,
                        principalTable: "PRODUCT_CATEGORY",
                        principalColumn: "category_id");
                });

            migrationBuilder.CreateTable(
                name: "PURCHASE_ORDER",
                columns: table => new
                {
                    purchase_order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    purchase_order_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    supplier_id = table.Column<int>(type: "int", nullable: true),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    order_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    total_amount = table.Column<double>(type: "float", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PURCHASE__AFCA88E6D458B84D", x => x.purchase_order_id);
                    table.ForeignKey(
                        name: "FK__PURCHASE___emplo__5165187F",
                        column: x => x.employee_id,
                        principalTable: "EMPLOYEE",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK__PURCHASE___suppl__5070F446",
                        column: x => x.supplier_id,
                        principalTable: "SUPPLIER",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "CART_ITEM",
                columns: table => new
                {
                    cart_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cart_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<double>(type: "float", nullable: false),
                    line_total = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CART_ITEM", x => x.cart_item_id);
                    table.ForeignKey(
                        name: "FK_CART_ITEM_CART_cart_id",
                        column: x => x.cart_id,
                        principalTable: "CART",
                        principalColumn: "cart_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CART_ITEM_PRODUCT_product_id",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_DETAIL",
                columns: table => new
                {
                    order_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    unit_price = table.Column<double>(type: "float", nullable: true),
                    discount = table.Column<double>(type: "float", nullable: true),
                    line_total = table.Column<double>(type: "float", nullable: true, computedColumnSql: "([quantity]*[unit_price]-[discount])", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ORDER_DE__3C5A4080964198A2", x => x.order_detail_id);
                    table.ForeignKey(
                        name: "FK__ORDER_DET__order__693CA210",
                        column: x => x.order_id,
                        principalTable: "SALES_ORDER",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK__ORDER_DET__produ__6A30C649",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "SUPPLY",
                columns: table => new
                {
                    supply_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    supplier_id = table.Column<int>(type: "int", nullable: false),
                    supply_date = table.Column<DateOnly>(type: "date", nullable: false),
                    cost_price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SUPPLY__4870CD837B5C9A2E", x => x.supply_id);
                    table.ForeignKey(
                        name: "FK__SUPPLY__product___48CFD27E",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "FK__SUPPLY__supplier__49C3F6B7",
                        column: x => x.supplier_id,
                        principalTable: "SUPPLIER",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "WARRANTY",
                columns: table => new
                {
                    warranty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    serial_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WARRANTY__24E65B0430326D8D", x => x.warranty_id);
                    table.ForeignKey(
                        name: "FK__WARRANTY__custom__71D1E811",
                        column: x => x.customer_id,
                        principalTable: "CUSTOMER",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK__WARRANTY__produc__70DDC3D8",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "INVENTORY",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    warehouse_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    purchase_order_id = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVENTOR__B59ACC491540FD43", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK__INVENTORY__produ__5629CD9C",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "FK__INVENTORY__purch__571DF1D5",
                        column: x => x.purchase_order_id,
                        principalTable: "PURCHASE_ORDER",
                        principalColumn: "purchase_order_id");
                    table.ForeignKey(
                        name: "FK__INVENTORY__wareh__5535A963",
                        column: x => x.warehouse_id,
                        principalTable: "WAREHOUSE",
                        principalColumn: "warehouse_id");
                });

            migrationBuilder.CreateTable(
                name: "PURCHASE_ORDER_DETAIL",
                columns: table => new
                {
                    purchase_order_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    purchase_order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    unit_cost = table.Column<double>(type: "float", nullable: true),
                    line_total = table.Column<double>(type: "float", nullable: true, computedColumnSql: "([quantity]*[unit_cost])", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PURCHASE__4B370F6D511BCF68", x => x.purchase_order_detail_id);
                    table.ForeignKey(
                        name: "FK__PURCHASE___produ__6E01572D",
                        column: x => x.product_id,
                        principalTable: "PRODUCT",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "FK__PURCHASE___purch__6D0D32F4",
                        column: x => x.purchase_order_id,
                        principalTable: "PURCHASE_ORDER",
                        principalColumn: "purchase_order_id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__BRAND__0C0C3B583015C4E1",
                table: "BRAND",
                column: "brand_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CART_customer_id",
                table: "CART",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_CART_ITEM_cart_id",
                table: "CART_ITEM",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "IX_CART_ITEM_product_id",
                table: "CART_ITEM",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "UQ__CUSTOMER__6A9E4CB785B4A11F",
                table: "CUSTOMER",
                column: "customer_code",
                unique: true,
                filter: "[customer_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__EMPLOYEE__B0AA7345A2B15837",
                table: "EMPLOYEE",
                column: "employee_code",
                unique: true,
                filter: "[employee_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_product_id",
                table: "INVENTORY",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_purchase_order_id",
                table: "INVENTORY",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_warehouse_id",
                table: "INVENTORY",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAIL_order_id",
                table: "ORDER_DETAIL",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAIL_product_id",
                table: "ORDER_DETAIL",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_brand_id",
                table: "PRODUCT",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_category_id",
                table: "PRODUCT",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "UQ__PRODUCT__AE1A8CC4FB8207A6",
                table: "PRODUCT",
                column: "product_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDER_employee_id",
                table: "PURCHASE_ORDER",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDER_supplier_id",
                table: "PURCHASE_ORDER",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "UQ__PURCHASE__19DA46F1BE5EB09D",
                table: "PURCHASE_ORDER",
                column: "purchase_order_code",
                unique: true,
                filter: "[purchase_order_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDER_DETAIL_product_id",
                table: "PURCHASE_ORDER_DETAIL",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDER_DETAIL_purchase_order_id",
                table: "PURCHASE_ORDER_DETAIL",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_customer_id",
                table: "SALES_ORDER",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDER_employee_id",
                table: "SALES_ORDER",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "UQ__SALES_OR__99D12D3F11C3C006",
                table: "SALES_ORDER",
                column: "order_code",
                unique: true,
                filter: "[order_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLY_product_id",
                table: "SUPPLY",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLY_supplier_id",
                table: "SUPPLY",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_customer_id",
                table: "USER_ACCOUNT",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_employee_id",
                table: "USER_ACCOUNT",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "UQ__USER_ACC__F3DBC5721AE09BBF",
                table: "USER_ACCOUNT",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WARRANTY_customer_id",
                table: "WARRANTY",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_WARRANTY_product_id",
                table: "WARRANTY",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CART_ITEM");

            migrationBuilder.DropTable(
                name: "INVENTORY");

            migrationBuilder.DropTable(
                name: "ORDER_DETAIL");

            migrationBuilder.DropTable(
                name: "PURCHASE_ORDER_DETAIL");

            migrationBuilder.DropTable(
                name: "SUPPLY");

            migrationBuilder.DropTable(
                name: "USER_ACCOUNT");

            migrationBuilder.DropTable(
                name: "WARRANTY");

            migrationBuilder.DropTable(
                name: "CART");

            migrationBuilder.DropTable(
                name: "WAREHOUSE");

            migrationBuilder.DropTable(
                name: "SALES_ORDER");

            migrationBuilder.DropTable(
                name: "PURCHASE_ORDER");

            migrationBuilder.DropTable(
                name: "PRODUCT");

            migrationBuilder.DropTable(
                name: "CUSTOMER");

            migrationBuilder.DropTable(
                name: "EMPLOYEE");

            migrationBuilder.DropTable(
                name: "SUPPLIER");

            migrationBuilder.DropTable(
                name: "BRAND");

            migrationBuilder.DropTable(
                name: "PRODUCT_CATEGORY");
        }
    }
}
