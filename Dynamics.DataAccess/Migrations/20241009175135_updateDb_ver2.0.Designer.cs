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
    [Migration("20241009175135_updateDb_ver2.0")]
    partial class updateDb_ver20
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
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationEmail")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OrganizationName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("OrganizationPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationPictures")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("ShutdownDay")
                        .HasColumnType("date");

                    b.Property<DateOnly>("StartTime")
                        .HasColumnType("date");

                    b.Property<bool>("isBanned")
                        .HasColumnType("bit");

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

                    b.Property<int?>("Status")
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

                    b.Property<int>("Quantity")
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

                    b.Property<string>("Attachments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ProjectResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

                    b.HasKey("TransactionID");

                    b.HasIndex("OrganizationResourceID");

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
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ProjectPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectStatus")
                        .HasColumnType("int");

                    b.Property<string>("ReportFile")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RequestID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ShutdownReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("StartTime")
                        .HasColumnType("date");

                    b.Property<bool>("isBanned")
                        .HasColumnType("bit");

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

                    b.Property<int>("ExpectedQuantity")
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

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("isEmergency")
                        .HasColumnType("int");

                    b.HasKey("RequestID");

                    b.HasIndex("UserID");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("Dynamics.Models.Models.User", b =>
                {
                    b.Property<Guid>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

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

                    b.Property<string>("UserRole")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isBanned")
                        .HasColumnType("bit");

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

                    b.Property<string>("Attachments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

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

                    b.Property<string>("Attachments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TransactionID");

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

                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("OrganizationToProjectHistory")
                        .HasForeignKey("ProjectResourceID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("OrganizationResource");

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
                        .WithMany("UserToOrganizationTransactionHistory")
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
                    b.HasOne("Dynamics.Models.Models.ProjectResource", "ProjectResource")
                        .WithMany("UserToProjectTransactionHistory")
                        .HasForeignKey("ProjectResourceID")
                        .OnDelete(DeleteBehavior.Cascade)
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

                    b.Navigation("UserToOrganizationTransactionHistory");
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
