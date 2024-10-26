
using Dynamics.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Award> Awards { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationMember> OrganizationMember { get; set; }
        public virtual DbSet<OrganizationResource> OrganizationResources { get; set; }
        public virtual DbSet<OrganizationToProjectHistory> OrganizationToProjectTransactionHistory { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        //public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserToOrganizationTransactionHistory> UserToOrganizationTransactionHistories { get; set; }
        public virtual DbSet<UserToProjectTransactionHistory> UserToProjectTransactionHistories { get; set; }
        public virtual DbSet<ProjectMember> ProjectMembers { get; set; }
        public virtual DbSet<ProjectResource> ProjectResources { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable sensitive data logging
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<Organization>().HasIndex(o => o.OrganizationName).IsUnique();
            modelBuilder.Entity<Organization>().HasIndex(o => o.OrganizationEmail).IsUnique();

            // Primary Keys
            modelBuilder.Entity<Report>().HasKey(r => r.ReportID);
            modelBuilder.Entity<Award>().HasKey(a => a.AwardID);
            modelBuilder.Entity<OrganizationResource>().HasKey(or => or.ResourceID);
            modelBuilder.Entity<Organization>().HasKey(o => o.OrganizationID);
            modelBuilder.Entity<OrganizationToProjectHistory>().HasKey(o => o.TransactionID);
            modelBuilder.Entity<UserToOrganizationTransactionHistory>().HasKey(u => u.TransactionID);
            modelBuilder.Entity<UserToProjectTransactionHistory>().HasKey(u => u.TransactionID);
            modelBuilder.Entity<ProjectResource>().HasKey(pr => pr.ResourceID);
            modelBuilder.Entity<Project>().HasKey(p => p.ProjectID);
            modelBuilder.Entity<ProjectMember>().HasKey(pm => new { pm.ProjectID, pm.UserID });
            modelBuilder.Entity<OrganizationMember>().HasKey(om => new { om.OrganizationID, om.UserID });
            modelBuilder.Entity<Request>().HasKey(r => r.RequestID);
            modelBuilder.Entity<History>().HasKey(r => r.HistoryID);
            modelBuilder.Entity<Notification>().HasKey(n => n.NotificationID);
            modelBuilder.Entity<User>().HasKey(u => u.Id);

            // Relationships (Foreign Keys)

            // Report to User: one user can have many reports
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(u => u.ReportsMade)
                .HasForeignKey(r => r.ReporterID)
                .OnDelete(DeleteBehavior.NoAction);

            // Award to User
            modelBuilder.Entity<Award>()
                .HasOne(a => a.User)
                .WithMany(u => u.Award)
                .HasForeignKey(a => a.UserID);

            // OrganizationResource to Organization
            modelBuilder.Entity<OrganizationResource>()
                .HasOne(or => or.Organization)
                .WithMany(o => o.OrganizationResource)
                .HasForeignKey(or => or.OrganizationID);

            // Project to Organization
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Organization)
                .WithMany(o => o.Project)
                .HasForeignKey(p => p.OrganizationID);

            // Project to Request
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Request)
                .WithOne(r => r.Project)
                .HasForeignKey<Project>(p => p.RequestID);

            // ProjectResource to Project
            modelBuilder.Entity<ProjectResource>()
                .HasOne(pr => pr.Project)
                .WithMany(p => p.ProjectResource)
                .HasForeignKey(pr => pr.ProjectID);


            // Request to User
            modelBuilder.Entity<Request>()
                .HasOne(r => r.User)
                .WithMany(u => u.Request)
                .HasForeignKey(r => r.UserID);

            // ProjectMember to User
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMember)
                .HasForeignKey(pm => pm.UserID);

            // ProjectMember to Project
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMember)
                .HasForeignKey(pm => pm.ProjectID);

            // OrganizationMember to User
            modelBuilder.Entity<OrganizationMember>()
                .HasOne(om => om.User)
                .WithMany(u => u.OrganizationMember)
                .HasForeignKey(om => om.UserID);

            // OrganizationMember to Organization
            modelBuilder.Entity<OrganizationMember>()
                .HasOne(om => om.Organization)
                .WithMany(o => o.OrganizationMember)
                .HasForeignKey(om => om.OrganizationID);

            // UserToOrganizationTransactionHistory to User
            modelBuilder.Entity<UserToOrganizationTransactionHistory>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserToOrganizationTransactionHistories)
                .HasForeignKey(ut => ut.UserID);

            // UserToProjectTransactionHistory to User
            modelBuilder.Entity<UserToProjectTransactionHistory>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserToProjectTransactionHistories)
                .HasForeignKey(ut => ut.UserID);

            // UserToProjectTransactionHistory to ProjectResource 
            modelBuilder.Entity<UserToProjectTransactionHistory>()
                .HasOne(ut => ut.ProjectResource)
                .WithMany(p => p.UserToProjectTransactionHistory)
                .HasForeignKey(ut => ut.ProjectResourceID);

            // History to Project
            modelBuilder.Entity<History>()
                .HasOne(h => h.Project)
                .WithMany(p => p.History)
                .HasForeignKey(h => h.ProjectID);

            //UserToOrganizationTransactionHistory to OrganizationResource
            modelBuilder.Entity<UserToOrganizationTransactionHistory>()
                .HasOne(ut => ut.OrganizationResource)
                .WithMany(or => or.UserToOrganizationTransactionHistory)
                .HasForeignKey(ut => ut.ResourceID);

            //OrganizationToProjectHistory to OrganizationResource
            modelBuilder.Entity<OrganizationToProjectHistory>()
                .HasOne(o => o.OrganizationResource)
                .WithMany(or => or.OrganizationToProjectHistory)
                .HasForeignKey(o => o.OrganizationResourceID);

            //ProjectResource to OrganizationToProjectHistory
            //modelBuilder.Entity<ProjectResource>()
            //    .HasMany(pr => pr.OrganizationToProjectHistory)
            //    .WithOne(p => p.ProjectResource)
            //    .HasForeignKey(pr => pr.ProjectResourceID)
            //    .OnDelete(DeleteBehavior.NoAction);

            //OrganizationToProjectHistory to ProjectResource
            modelBuilder.Entity<OrganizationToProjectHistory>()
                .HasOne(a => a.ProjectResource)
                .WithMany(p => p.OrganizationToProjectHistory)
                .HasForeignKey(k => k.ProjectResourceID)
                .OnDelete(DeleteBehavior.NoAction);

            //Notification to User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserID);
        }
    }
}
