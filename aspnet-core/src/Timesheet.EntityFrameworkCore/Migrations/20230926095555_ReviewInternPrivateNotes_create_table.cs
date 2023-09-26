using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class ReviewInternPrivateNotes_create_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewInternPrivateNotes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    ReviewDetailId = table.Column<long>(nullable: false),
                    NoteByUserId = table.Column<long>(nullable: false),
                    PrivateNote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewInternPrivateNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewInternPrivateNotes_AbpUsers_NoteByUserId",
                        column: x => x.NoteByUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewInternPrivateNotes_ReviewDetails_ReviewDetailId",
                        column: x => x.ReviewDetailId,
                        principalTable: "ReviewDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternPrivateNotes_NoteByUserId",
                table: "ReviewInternPrivateNotes",
                column: "NoteByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternPrivateNotes_ReviewDetailId",
                table: "ReviewInternPrivateNotes",
                column: "ReviewDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewInternPrivateNotes");
        }
    }
}
