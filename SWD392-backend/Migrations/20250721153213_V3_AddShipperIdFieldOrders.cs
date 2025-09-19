using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD392_backend.Migrations
{
    /// <inheritdoc />
    public partial class V3_AddShipperIdFieldOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "shipper_id",
                table: "orders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shipper_id",
                table: "orders");
        }
    }
}
