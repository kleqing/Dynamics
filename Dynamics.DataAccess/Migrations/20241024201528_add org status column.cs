using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dynamics.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addorgstatuscolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isBanned",
                table: "Organizations");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationStatus",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationStatus",
                table: "Organizations");

            migrationBuilder.AddColumn<bool>(
                name: "isBanned",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
