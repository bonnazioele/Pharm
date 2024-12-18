using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharm.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductOrder");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "Orders",
                newName: "UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Orders",
                newName: "CustomerName");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Orders",
                newName: "TotalPrice");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOrder_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrder_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrder_OrderId",
                table: "ProductOrder",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrder_ProductId",
                table: "ProductOrder",
                column: "ProductId");
        }
    }
}
