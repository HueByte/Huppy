using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class TicketTopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Tickets");
        }
    }
}
