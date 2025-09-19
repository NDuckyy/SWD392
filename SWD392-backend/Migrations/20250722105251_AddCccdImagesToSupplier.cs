using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD392_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCccdImagesToSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "back_image",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "front_image",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "back_image",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "front_image",
                table: "suppliers");
        }
    }
}
