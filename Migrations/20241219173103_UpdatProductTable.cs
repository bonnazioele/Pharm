using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharm.Migrations
{
    /// <inheritdoc />
    public partial class UpdatProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresPrescription",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresPrescription",
                table: "Products");
        }
    }
}
