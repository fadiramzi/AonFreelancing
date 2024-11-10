using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class otpmigwithcheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_CODE",
                table: "otps",
                sql: "length([Code]) = 6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CODE",
                table: "otps");
        }
    }
}
