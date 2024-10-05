﻿// <auto-generated />
using System;
using Dynamics.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Dynamics.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
    [Migration("20241002075604_Initial")]
    partial class Initial
========
    [Migration("20241004083536_deleteProjectID")]
    partial class deleteProjectID
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Dynamics.Models.Models.Award", b =>
                {
                    b.Property<Guid>("AwardID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AwardName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AwardID");

                    b.HasIndex("UserID");

                    b.ToTable("Awards");
                });

            modelBuilder.Entity("Dynamics.Models.Models.History", b =>
                {
                    b.Property<Guid>("HistoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Attachment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Phase")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("HistoryID");

                    b.HasIndex("ProjectID");

                    b.ToTable("Histories");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Organization", b =>
                {
                    b.Property<Guid>("OrganizationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationEmail")
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                        .HasColumnType("nvarchar(max)");
========
                        .HasColumnType("nvarchar(450)");
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs

                    b.Property<string>("OrganizationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OrganizationPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationPictures")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("ShutdownDay")
                        .HasColumnType("date");

                    b.Property<DateOnly>("StartTime")
                        .HasColumnType("date");

                    b.HasKey("OrganizationID");

                    b.HasIndex("OrganizationEmail")
                        .IsUnique()
                        .HasFilter("[OrganizationEmail] IS NOT NULL");

                    b.HasIndex("OrganizationName")
                        .IsUnique();

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationMember", b =>
                {
                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<int?>("Status")
========
                    b.Property<int>("Status")
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasColumnType("int");

                    b.HasKey("OrganizationID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("OrganizationMember");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationResource", b =>
                {
                    b.Property<Guid>("ResourceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<int>("Quantity")
========
                    b.Property<int?>("Quantity")
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasColumnType("int");

                    b.Property<string>("ResourceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ResourceID");

                    b.HasIndex("OrganizationID");

                    b.ToTable("OrganizationResources");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationToProjectHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationResourceID")
                        .HasColumnType("uniqueidentifier");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
========
                    b.Property<Guid?>("ProjectID")
                        .HasColumnType("uniqueidentifier");

>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                    b.Property<Guid?>("ProjectResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

========
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                    b.HasKey("TransactionID");

                    b.HasIndex("OrganizationResourceID");

                    b.HasIndex("ProjectResourceID");

                    b.HasIndex("ProjectResourceID");

                    b.ToTable("OrganizationToProjectTransactionHistory");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Project", b =>
                {
                    b.Property<Guid>("ProjectID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Attachment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("EndTime")
                        .HasColumnType("date");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectPhoneNumber")
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                        .IsRequired()
========
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectStatus")
                        .HasColumnType("int");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<Guid?>("RequestID")
                        .HasColumnType("uniqueidentifier");
========
                    b.Property<string>("ReportFile")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RequestID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ShutdownReason")
                        .HasColumnType("nvarchar(max)");
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs

                    b.Property<DateOnly?>("StartTime")
                        .HasColumnType("date");

                    b.Property<string>("shutdownReanson")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ProjectID");

                    b.HasIndex("OrganizationID");

                    b.HasIndex("RequestID")
                        .IsUnique()
                        .HasFilter("[RequestID] IS NOT NULL");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectMember", b =>
                {
                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ProjectID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("ProjectMembers");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectResource", b =>
                {
                    b.Property<Guid>("ResourceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("ExpectedQuantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("ResourceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ResourceID");

                    b.HasIndex("ProjectID");

                    b.ToTable("ProjectResources");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Report", b =>
                {
                    b.Property<Guid>("ReportID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ObjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ReportDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("ReporterID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportID");

                    b.HasIndex("ReporterID");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Request", b =>
                {
                    b.Property<Guid>("RequestID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Attachment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("CreationDate")
                        .HasColumnType("date");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<string>("RequestAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestPhoneNumber")
========
                    b.Property<string>("RequestEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestPhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestTitle")
                        .IsRequired()
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("isEmergency")
                        .HasColumnType("int");

                    b.Property<string>("requestTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RequestID");

                    b.HasIndex("UserID");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("Dynamics.Models.Models.User", b =>
                {
                    b.Property<Guid>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserAvatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("UserDOB")
                        .HasColumnType("date");

                    b.Property<string>("UserDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserFullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserID");

                    b.HasIndex("UserEmail")
                        .IsUnique();

                    b.HasIndex("UserFullName")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToOrganizationTransactionHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<Guid>("ResourceID")
                        .HasColumnType("uniqueidentifier");

========
                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ResourceID")
                        .HasColumnType("uniqueidentifier");

>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

========
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TransactionID");

                    b.HasIndex("ResourceID");

                    b.HasIndex("UserID");

                    b.ToTable("UserToOrganizationTransactionHistories");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToProjectTransactionHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Property<Guid>("ResourceID")
========
                    b.Property<Guid?>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProjectResourceID")
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");
========
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TransactionID");

                    b.HasIndex("ResourceID");

                    b.HasIndex("ProjectResourceID");

                    b.HasIndex("UserID");

                    b.ToTable("UserToProjectTransactionHistories");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Award", b =>
                {
                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("Award")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.History", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Project", "Project")
                        .WithMany("History")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationMember", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("OrganizationMember")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("OrganizationMember")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationResource", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("OrganizationResource")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationToProjectHistory", b =>
                {
                    b.HasOne("Dynamics.Models.Models.OrganizationResource", "OrganizationResource")
                        .WithMany("OrganizationToProjectHistory")
                        .HasForeignKey("OrganizationResourceID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("OrganizationToProjectHistory")
                        .HasForeignKey("ProjectResourceID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("OrganizationResource");

========
                    b.HasOne("Dynamics.Models.Models.Project", null)
                        .WithMany("OrganizationToProjectTransactions")
                        .HasForeignKey("ProjectID");

                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("OrganizationToProjectTransactionHistories")
                        .HasForeignKey("ProjectResourceID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("OrganizationResource");

>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                    b.Navigation("ProjectResource");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Project", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("Project")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.Request", "Request")
                        .WithOne("Project")
                        .HasForeignKey("Dynamics.Models.Models.Project", "RequestID");

                    b.Navigation("Organization");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectMember", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Project", "Project")
                        .WithMany("ProjectMember")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("ProjectMember")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectResource", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Project", "Project")
                        .WithMany("ProjectResource")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Report", b =>
                {
                    b.HasOne("Dynamics.Models.Models.User", "Reporter")
                        .WithMany("ReportsMade")
                        .HasForeignKey("ReporterID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Reporter");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Request", b =>
                {
                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("Request")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToOrganizationTransactionHistory", b =>
                {
                    b.HasOne("Dynamics.Models.Models.OrganizationResource", "OrganizationResource")
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                        .WithMany("UserToOrganizationTransactionHistory")
========
                        .WithMany("UserToOrganizationTransactionHistories")
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .HasForeignKey("ResourceID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("UserToOrganizationTransactionHistories")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrganizationResource");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToProjectTransactionHistory", b =>
                {
<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("UserToProjectTransactionHistory")
                        .HasForeignKey("ResourceID")
                        .OnDelete(DeleteBehavior.Cascade)
========
                    b.HasOne("Dynamics.Models.Models.Project", null)
                        .WithMany("UserToProjectTransactions")
                        .HasForeignKey("ProjectID");

                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("UserToProjectTransactionHistories")
                        .HasForeignKey("ProjectResourceID")
                        .OnDelete(DeleteBehavior.NoAction)
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("UserToProjectTransactionHistories")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProjectResource");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Organization", b =>
                {
                    b.Navigation("OrganizationMember");

                    b.Navigation("OrganizationResource");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationResource", b =>
                {
                    b.Navigation("OrganizationToProjectHistory");

<<<<<<<< HEAD:Dynamics.DataAccess/Migrations/20241002075604_Initial.Designer.cs
                    b.Navigation("UserToOrganizationTransactionHistory");
========
                    b.Navigation("UserToOrganizationTransactionHistories");
>>>>>>>> e41eb2e9bd46e8dc32ce81fc3c55e4ce4f7a4eac:Dynamics.DataAccess/Migrations/20241004083536_deleteProjectID.Designer.cs
                });

            modelBuilder.Entity("Dynamics.Models.Models.Project", b =>
                {
                    b.Navigation("History");

                    b.Navigation("ProjectMember");

                    b.Navigation("ProjectResource");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectResource", b =>
                {
                    b.Navigation("OrganizationToProjectHistory");

                    b.Navigation("UserToProjectTransactionHistory");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectResource", b =>
                {
                    b.Navigation("OrganizationToProjectTransactionHistories");

                    b.Navigation("UserToProjectTransactionHistories");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Request", b =>
                {
                    b.Navigation("Project")
                        .IsRequired();
                });

            modelBuilder.Entity("Dynamics.Models.Models.User", b =>
                {
                    b.Navigation("Award");

                    b.Navigation("OrganizationMember");

                    b.Navigation("ProjectMember");

                    b.Navigation("ReportsMade");

                    b.Navigation("Request");

                    b.Navigation("UserToOrganizationTransactionHistories");

                    b.Navigation("UserToProjectTransactionHistories");
                });
#pragma warning restore 612, 618
        }
    }
}
