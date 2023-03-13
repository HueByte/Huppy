using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class TicketsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketAnswer",
                table: "Tickets",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketAnswer",
                table: "Tickets");
        }
    }
}
