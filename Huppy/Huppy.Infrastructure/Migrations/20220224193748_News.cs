using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class News : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AreNewsEnabled",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "NewsOutputRoom",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreNewsEnabled",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "NewsOutputRoom",
                table: "Servers");
        }
    }
}
