using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class FreelanceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeLancerId",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FreeLancerId",
                table: "Projects",
                column: "FreeLancerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Freelancers_FreeLancerId",
                table: "Projects",
                column: "FreeLancerId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Freelancers_FreeLancerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_FreeLancerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "FreeLancerId",
                table: "Projects");
        }
    }
}
