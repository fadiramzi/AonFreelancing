using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class ProjectModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FreelancerId",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PriceType",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QualificationName",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FreelancerId",
                table: "Projects",
                column: "FreelancerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Freelancers_FreelancerId",
                table: "Projects",
                column: "FreelancerId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Freelancers_FreelancerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_FreelancerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "FreelancerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PriceType",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QualificationName",
                table: "Projects");
        }
    }
}
