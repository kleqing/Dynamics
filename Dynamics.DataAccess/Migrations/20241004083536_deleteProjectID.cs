using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dynamics.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deleteProjectID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganizationEmail = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OrganizationPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationPictures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateOnly>(type: "date", nullable: false),
                    ShutdownDay = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserFullName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserDOB = table.Column<DateOnly>(type: "date", nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAvatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationResources",
                columns: table => new
                {
                    ResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationResources", x => x.ResourceID);
                    table.ForeignKey(
                        name: "FK_OrganizationResources_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Awards",
                columns: table => new
                {
                    AwardID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AwardName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards", x => x.AwardID);
                    table.ForeignKey(
                        name: "FK_Awards_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMember",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMember", x => new { x.OrganizationID, x.UserID });
                    table.ForeignKey(
                        name: "FK_OrganizationMember_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationMember_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ObjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReporterID",
                        column: x => x.ReporterID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RequestPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isEmergency = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_Requests_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserToOrganizationTransactionHistories",
                columns: table => new
                {
                    TransactionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToOrganizationTransactionHistories", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_UserToOrganizationTransactionHistories_OrganizationResources_ResourceID",
                        column: x => x.ResourceID,
                        principalTable: "OrganizationResources",
                        principalColumn: "ResourceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToOrganizationTransactionHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectStatus = table.Column<int>(type: "int", nullable: false),
                    Attachment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateOnly>(type: "date", nullable: true),
                    EndTime = table.Column<DateOnly>(type: "date", nullable: true),
                    ShutdownReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                    table.ForeignKey(
                        name: "FK_Projects_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Requests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "Requests",
                        principalColumn: "RequestID");
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    HistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attachment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_Histories_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectID, x.UserID });
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectResources",
                columns: table => new
                {
                    ResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    ExpectedQuantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResources", x => x.ResourceID);
                    table.ForeignKey(
                        name: "FK_ProjectResources_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationToProjectTransactionHistory",
                columns: table => new
                {
                    TransactionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Time = table.Column<DateOnly>(type: "date", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationToProjectTransactionHistory", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_OrganizationToProjectTransactionHistory_OrganizationResources_OrganizationResourceID",
                        column: x => x.OrganizationResourceID,
                        principalTable: "OrganizationResources",
                        principalColumn: "ResourceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationToProjectTransactionHistory_ProjectResources_ProjectResourceID",
                        column: x => x.ProjectResourceID,
                        principalTable: "ProjectResources",
                        principalColumn: "ResourceID");
                    table.ForeignKey(
                        name: "FK_OrganizationToProjectTransactionHistory_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID");
                });

            migrationBuilder.CreateTable(
                name: "UserToProjectTransactionHistories",
                columns: table => new
                {
                    TransactionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectResourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateOnly>(type: "date", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToProjectTransactionHistories", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID",
                        column: x => x.ProjectResourceID,
                        principalTable: "ProjectResources",
                        principalColumn: "ResourceID");
                    table.ForeignKey(
                        name: "FK_UserToProjectTransactionHistories_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID");
                    table.ForeignKey(
                        name: "FK_UserToProjectTransactionHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Awards_UserID",
                table: "Awards",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_ProjectID",
                table: "Histories",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMember_UserID",
                table: "OrganizationMember",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationResources_OrganizationID",
                table: "OrganizationResources",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OrganizationEmail",
                table: "Organizations",
                column: "OrganizationEmail",
                unique: true,
                filter: "[OrganizationEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OrganizationName",
                table: "Organizations",
                column: "OrganizationName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationToProjectTransactionHistory_OrganizationResourceID",
                table: "OrganizationToProjectTransactionHistory",
                column: "OrganizationResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationToProjectTransactionHistory_ProjectID",
                table: "OrganizationToProjectTransactionHistory",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationToProjectTransactionHistory_ProjectResourceID",
                table: "OrganizationToProjectTransactionHistory",
                column: "ProjectResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserID",
                table: "ProjectMembers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResources_ProjectID",
                table: "ProjectResources",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationID",
                table: "Projects",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_RequestID",
                table: "Projects",
                column: "RequestID",
                unique: true,
                filter: "[RequestID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterID",
                table: "Reports",
                column: "ReporterID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_UserID",
                table: "Requests",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserFullName",
                table: "Users",
                column: "UserFullName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserToOrganizationTransactionHistories_ResourceID",
                table: "UserToOrganizationTransactionHistories",
                column: "ResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToOrganizationTransactionHistories_UserID",
                table: "UserToOrganizationTransactionHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToProjectTransactionHistories_ProjectID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToProjectTransactionHistories_ProjectResourceID",
                table: "UserToProjectTransactionHistories",
                column: "ProjectResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToProjectTransactionHistories_UserID",
                table: "UserToProjectTransactionHistories",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Awards");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "OrganizationMember");

            migrationBuilder.DropTable(
                name: "OrganizationToProjectTransactionHistory");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "UserToOrganizationTransactionHistories");

            migrationBuilder.DropTable(
                name: "UserToProjectTransactionHistories");

            migrationBuilder.DropTable(
                name: "OrganizationResources");

            migrationBuilder.DropTable(
                name: "ProjectResources");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
