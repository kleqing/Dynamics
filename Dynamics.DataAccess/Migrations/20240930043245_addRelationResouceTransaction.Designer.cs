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
    [Migration("20240930043245_addRelationResouceTransaction")]
    partial class addRelationResouceTransaction
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
                        .IsRequired()
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

                    b.Property<Guid?>("CEOID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrganizationPictures")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("ShutdownDay")
                        .HasColumnType("date");

                    b.Property<DateOnly?>("StartTime")
                        .HasColumnType("date");

                    b.HasKey("OrganizationID");

                    b.HasIndex("CEOID");

                    b.ToTable("Organizations");

                    b.HasData(
                        new
                        {
                            OrganizationID = new Guid("4641b799-7fba-4d20-a78a-4d68db162e98"),
                            OrganizationDescription = "llalalala",
                            OrganizationName = "Organization 1"
                        },
                        new
                        {
                            OrganizationID = new Guid("c2f983db-d9bb-4214-ae40-eab93cc2de72"),
                            OrganizationDescription = "llalalala222",
                            OrganizationName = "Organization 2"
                        });
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationMember", b =>
                {
                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("OrganizationID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("OrganizationMember");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationResource", b =>
                {
                    b.Property<Guid>("ResourceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ContentTransaction")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ExpectedQuantity")
                        .HasColumnType("int");

                    b.Property<Guid>("OrganizationID")
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

                    b.HasIndex("OrganizationID");

                    b.ToTable("OrganizationResources");
                });

            modelBuilder.Entity("Dynamics.Models.Models.OrganizationToProjectHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TransactionID");

                    b.HasIndex("OrganizationID");

                    b.HasIndex("ProjectID");

                    b.HasIndex("ResourceID");

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

                    b.Property<Guid?>("LeaderID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProjectDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProjectName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectStatus")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RequestID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateOnly?>("StartTime")
                        .HasColumnType("date");

                    b.HasKey("ProjectID");

                    b.HasIndex("LeaderID");

                    b.HasIndex("OrganizationID");

                    b.HasIndex("RequestID")
                        .IsUnique()
                        .HasFilter("[RequestID] IS NOT NULL");

                    b.ToTable("Projects");

                    b.HasData(
                        new
                        {
                            ProjectID = new Guid("2d3c8b76-4e52-4446-8a5a-25cfa18b12aa"),
                            LeaderID = new Guid("e4894112-58a3-4fe6-9222-8ad70816adfe"),
                            OrganizationID = new Guid("4641b799-7fba-4d20-a78a-4d68db162e98"),
                            ProjectDescription = "Project ds dg dg dfgdg ",
                            ProjectName = "Project 1",
                            ProjectStatus = 1,
                            RequestID = new Guid("ad111cb1-39d5-42e2-a2f5-e943898e59e6")
                        },
                        new
                        {
                            ProjectID = new Guid("bbe8d3dd-e15b-4151-b6ea-80dd44c2280f"),
                            LeaderID = new Guid("e4894112-58a3-4fe6-9222-8ad70816adfe"),
                            OrganizationID = new Guid("c2f983db-d9bb-4214-ae40-eab93cc2de72"),
                            ProjectDescription = "Project sdfsdf dg dg dg s",
                            ProjectName = "Project 2",
                            ProjectStatus = 1,
                            RequestID = new Guid("4398bfb2-5a6d-4daf-a8be-52ef92f455a9")
                        });
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
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("isEmergency")
                        .HasColumnType("int");

                    b.HasKey("RequestID");

                    b.HasIndex("UserID");

                    b.ToTable("Requests");

                    b.HasData(
                        new
                        {
                            RequestID = new Guid("ad111cb1-39d5-42e2-a2f5-e943898e59e6"),
                            Content = "Content of request 1",
                            Title = "Request 1",
                            UserID = new Guid("fbd9f087-d6d2-4a27-a763-f1951da04361")
                        },
                        new
                        {
                            RequestID = new Guid("4398bfb2-5a6d-4daf-a8be-52ef92f455a9"),
                            Content = "Content of request 2",
                            Title = "Request 2",
                            UserID = new Guid("fbd9f087-d6d2-4a27-a763-f1951da04361")
                        });
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
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserFullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserPhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToOrganizationTransactionHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateOnly>("Time")
                        .HasColumnType("date");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TransactionID");

                    b.HasIndex("OrganizationID");

                    b.HasIndex("UserID");

                    b.ToTable("UserToOrganizationTransactionHistories");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToProjectTransactionHistory", b =>
                {
                    b.Property<Guid>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProjectID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ResourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Time")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TransactionID");

                    b.HasIndex("ProjectID");

                    b.HasIndex("ResourceID");

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

            modelBuilder.Entity("Dynamics.Models.Models.Organization", b =>
                {
                    b.HasOne("Dynamics.Models.Models.User", "CEO")
                        .WithMany()
                        .HasForeignKey("CEOID");

                    b.Navigation("CEO");
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
                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("OrganizationToProjectTransactions")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.Project", "Project")
                        .WithMany("OrganizationToProjectTransactions")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.ProjectResource", "Resource")
                        .WithMany("OrganizationToProjectHistories")
                        .HasForeignKey("ResourceID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Organization");

                    b.Navigation("Project");

                    b.Navigation("Resource");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Project", b =>
                {
                    b.HasOne("Dynamics.Models.Models.User", "Leader")
                        .WithMany()
                        .HasForeignKey("LeaderID");

                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("Project")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.Request", "Request")
                        .WithOne("Project")
                        .HasForeignKey("Dynamics.Models.Models.Project", "RequestID");

                    b.Navigation("Leader");

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
                    b.HasOne("Dynamics.Models.Models.Organization", "Organization")
                        .WithMany("UserToOrganizationTransactions")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("UserToOrganizationTransactions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.UserToProjectTransactionHistory", b =>
                {
                    b.HasOne("Dynamics.Models.Models.Project", "Project")
                        .WithMany("UserToProjectTransactions")
                        .HasForeignKey("ProjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dynamics.Models.Models.ProjectResource", "Resource")
                        .WithMany("UserToProjectTransactionHistories")
                        .HasForeignKey("ResourceID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Dynamics.Models.Models.User", "User")
                        .WithMany("UserToProjectTransactions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("Resource");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Organization", b =>
                {
                    b.Navigation("OrganizationMember");

                    b.Navigation("OrganizationResource");

                    b.Navigation("OrganizationToProjectTransactions");

                    b.Navigation("Project");

                    b.Navigation("UserToOrganizationTransactions");
                });

            modelBuilder.Entity("Dynamics.Models.Models.Project", b =>
                {
                    b.Navigation("History");

                    b.Navigation("OrganizationToProjectTransactions");

                    b.Navigation("ProjectMember");

                    b.Navigation("ProjectResource");

                    b.Navigation("UserToProjectTransactions");
                });

            modelBuilder.Entity("Dynamics.Models.Models.ProjectResource", b =>
                {
                    b.Navigation("OrganizationToProjectHistories");

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

                    b.Navigation("Request");

                    b.Navigation("UserToOrganizationTransactions");

                    b.Navigation("UserToProjectTransactions");
                });
#pragma warning restore 612, 618
        }
    }
}
