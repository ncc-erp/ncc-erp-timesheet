using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timesheet.Migrations
{
    public partial class ReviewInternComments_create_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewInternComments",
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
                    CommentUserId = table.Column<long>(nullable: true),
                    PrivateNote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewInternComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewInternComments_AbpUsers_CommentUserId",
                        column: x => x.CommentUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewInternComments_ReviewDetails_ReviewDetailId",
                        column: x => x.ReviewDetailId,
                        principalTable: "ReviewDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternComments_CommentUserId",
                table: "ReviewInternComments",
                column: "CommentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewInternComments_ReviewDetailId",
                table: "ReviewInternComments",
                column: "ReviewDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewInternComments");
        }
    }
}
