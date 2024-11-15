using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class projectMirgation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressStatus",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Qualification",
                table: "Projects",
                newName: "QualificationName");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PRICE_TYPE",
                table: "Projects",
                sql: "[PriceType] IN ('Fixed', 'PerHour')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects",
                sql: "[QualificationName] IN ('uiux', 'frontend', 'mobile', 'backend', 'fullstack')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_STATUS",
                table: "Projects",
                sql: "[Status] IN ('Available', 'Closed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PRICE_TYPE",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QUALIFICATION_NAME",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_STATUS",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "About",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "QualificationName",
                table: "Projects",
                newName: "Qualification");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "Available");

            migrationBuilder.AddColumn<string>(
                name: "ProgressStatus",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
