using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RunbookPlatform.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStepDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Command",
                table: "Steps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedResult",
                table: "Steps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Steps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Steps",
                type: "TEXT",
                nullable: false,
                defaultValue: "Action");

            migrationBuilder.AddColumn<string>(
                name: "Command",
                table: "RunbookVersionSteps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedResult",
                table: "RunbookVersionSteps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "RunbookVersionSteps",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "RunbookVersionSteps",
                type: "TEXT",
                nullable: false,
                defaultValue: "Action");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Command",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "ExpectedResult",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Command",
                table: "RunbookVersionSteps");

            migrationBuilder.DropColumn(
                name: "ExpectedResult",
                table: "RunbookVersionSteps");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "RunbookVersionSteps");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "RunbookVersionSteps");
        }
    }
}
