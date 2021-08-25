using Microsoft.EntityFrameworkCore.Migrations;

namespace Piscesco.Migrations.PiscescoModel
{
    public partial class AddOrderOwnerColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerID",
                table: "Order",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerID",
                table: "Order");
        }
    }
}
