using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dynamics.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateaftermerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationToProjectTransactionHistory_Projects_ProjectID",
                table: "OrganizationToProjectTransactionHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID",
                table: "UserToProjectTransactionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToProjectTransactionHistories_Projects_ProjectID",
                table: "UserToProjectTransactionHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserToProjectTransactionHistories_ProjectID",
                table: "UserToProjectTransactionHistories");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationToProjectTransactionHistory_ProjectID",
                table: "OrganizationToProjectTransactionHistory");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "UserToProjectTransactionHistories");

            migrationBuilder.DropColumn(
                name: "OrganizationID",
                table: "UserToOrganizationTransactionHistories");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "OrganizationToProjectTransactionHistory");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "UserToOrganizationTransactionHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RequestPhoneNumber",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RequestEmail",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectName",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectEmail",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "Organizations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationDescription",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrganizationResources",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OrganizationMember",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectResourceID",
                principalTable: "ProjectResources",
                principalColumn: "ResourceID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID",
                table: "UserToProjectTransactionHistories");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "UserToOrganizationTransactionHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectID",
                table: "UserToProjectTransactionHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationID",
                table: "UserToOrganizationTransactionHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "RequestPhoneNumber",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RequestEmail",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectName",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectEmail",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectID",
                table: "OrganizationToProjectTransactionHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationName",
                table: "Organizations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationDescription",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrganizationResources",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OrganizationMember",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserToProjectTransactionHistories_ProjectID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationToProjectTransactionHistory_ProjectID",
                table: "OrganizationToProjectTransactionHistory",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationToProjectTransactionHistory_Projects_ProjectID",
                table: "OrganizationToProjectTransactionHistory",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectResourceID",
                principalTable: "ProjectResources",
                principalColumn: "ResourceID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserToProjectTransactionHistories_Projects_ProjectID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");
        }
    }
}
