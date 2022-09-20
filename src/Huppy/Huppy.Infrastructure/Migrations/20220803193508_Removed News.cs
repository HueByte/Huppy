using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class RemovedNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsOutputRoom",
                table: "ServerRooms");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "NewsOutputRoom",
                table: "ServerRooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
