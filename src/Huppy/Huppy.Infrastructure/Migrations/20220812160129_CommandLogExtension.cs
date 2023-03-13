using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    public partial class CommandLogExtension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "ChannelId",
                table: "CommandLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "CommandLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "CommandLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommandLogs_GuildId",
                table: "CommandLogs",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLogs_Servers_GuildId",
                table: "CommandLogs",
                column: "GuildId",
                principalTable: "Servers",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandLogs_Servers_GuildId",
                table: "CommandLogs");

            migrationBuilder.DropIndex(
                name: "IX_CommandLogs_GuildId",
                table: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "CommandLogs");
        }
    }
}
