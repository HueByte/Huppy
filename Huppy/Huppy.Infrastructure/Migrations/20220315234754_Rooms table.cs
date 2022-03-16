using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class Roomstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsOutputRoom",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "OutputRoom",
                table: "Servers");

            migrationBuilder.AddColumn<ulong>(
                name: "ServerRoomsID",
                table: "Servers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerRooms",
                columns: table => new
                {
                    ServerRoomsID = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OutputRoom = table.Column<ulong>(type: "INTEGER", nullable: false),
                    NewsOutputRoom = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GreetingRoom = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ServerID = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerRooms", x => x.ServerRoomsID);
                    table.ForeignKey(
                        name: "FK_ServerRooms_Servers_ServerRoomsID",
                        column: x => x.ServerRoomsID,
                        principalTable: "Servers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerRooms");

            migrationBuilder.DropColumn(
                name: "ServerRoomsID",
                table: "Servers");

            migrationBuilder.AddColumn<ulong>(
                name: "NewsOutputRoom",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "OutputRoom",
                table: "Servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
