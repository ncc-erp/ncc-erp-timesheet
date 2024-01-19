using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class ReviewInternPrivateNote_AddColumn_ReviewInternNoteType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewInternNoteType",
                table: "ReviewInternPrivateNotes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewInternNoteType",
                table: "ReviewInternPrivateNotes");
        }
    }
}
