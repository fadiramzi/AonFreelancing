using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class AddingUserTypeToUserMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "verified",
                table: "TempUsers",
                newName: "Verified");

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Verified",
                table: "TempUsers",
                newName: "verified");
        }
    }
}
