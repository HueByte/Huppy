using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class KeysRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerRooms_Servers_ServerRoomsID",
                table: "ServerRooms");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Servers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ServerRoomsID",
                table: "ServerRooms",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerRooms_Servers_Id",
                table: "ServerRooms",
                column: "Id",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerRooms_Servers_Id",
                table: "ServerRooms");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Servers",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ServerRooms",
                newName: "ServerRoomsID");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerRooms_Servers_ServerRoomsID",
                table: "ServerRooms",
                column: "ServerRoomsID",
                principalTable: "Servers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
