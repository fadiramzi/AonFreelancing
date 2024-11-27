using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class projectlikesmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Freelancers_userId",
                table: "Skills");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Skills",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_userId",
                table: "Skills",
                newName: "IX_Skills_UserId");

            migrationBuilder.CreateTable(
                name: "ProjectLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectLikes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLikes_ProjectId_UserId",
                table: "ProjectLikes",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLikes_UserId",
                table: "ProjectLikes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Freelancers_UserId",
                table: "Skills",
                column: "UserId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Freelancers_UserId",
                table: "Skills");

            migrationBuilder.DropTable(
                name: "ProjectLikes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Skills",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_UserId",
                table: "Skills",
                newName: "IX_Skills_userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Freelancers_userId",
                table: "Skills",
                column: "userId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
