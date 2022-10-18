using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTrackerPro.Data.Migrations
{
    public partial class UpdateDataModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TicketStatuses",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TicketStatuses",
                newName: "Title");
        }
    }
}
