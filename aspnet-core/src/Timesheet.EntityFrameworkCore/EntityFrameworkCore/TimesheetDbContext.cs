using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.MultiTenancy;
using Ncc.Entities;
using System;
using Abp.Authorization;
using Timesheet.Entities;

namespace Ncc.EntityFrameworkCore
{
    public class TimesheetDbContext : AbpZeroDbContext<Tenant, Role, User, TimesheetDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<ProjectTask> TaskProjects { get; set; }
        public DbSet<MyTimesheet> MyTimesheets { get; set; }

        public DbSet<ProjectTargetUser> ProjectTargetUsers { get; set; }
        public DbSet<DayOffSetting> DayOffSettings { get; set; }
        public DbSet<DayOffType> DayOffTypes { get; set; }
        public DbSet<AbsenceDayDetail> AbsenceDayDetails { get; set; }
        public DbSet<AbsenceDayRequest> AbsenceDayRequests { get; set; }
        public DbSet<UnlockTimesheet> UnlockTimesheets { get; set; }
        public DbSet<Timekeeping> Timekeepings { get; set; }
        public DbSet<HistoryWorkingTime> HistoryWorkingTimes { get; set; }
        public DbSet<UserUnlockIms> UserUnlockIms { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<ReviewIntern> ReviewInterns { get; set; }
        public DbSet<ReviewDetail> ReviewDetails { get; set; }
        public DbSet<OverTimeSetting> OverTimeSettings { get; set; }
        public DbSet<KomuTracker> KomuTrackers { get; set; }
        public DbSet<Branch> Branchs { get; set; }

        public DbSet<Retro> Retros { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<RetroResult> RetroResults { get; set; }
        public DbSet<Capability> Capabilities { get; set; }
        public DbSet<CapabilitySetting> CapabilitySettings { get; set; }
        public DbSet<ReviewInternCapability> ReviewInternCapabilities { get; set; }
        public DbSet<TeamBuildingDetail> TeamBuildingDetails { get; set; }
        public DbSet<TeamBuildingRequestHistory> TeamBuildingRequestHistories { get; set; }
        public DbSet<TeamBuildingRequestHistoryFile> TeamBuildingRequestHistoryFiles { get; set; }
        public DbSet<ReviewInternPrivateNote> ReviewInternPrivateNotes { get; set; }
        public DbSet<ValueOfUserInProject> ValueOfUserInProjects { get; set; }
        public DbSet<OpenTalk> OpenTalk { get; set; }
        public DbSet<PunishmentSystem> PunishmentSystems { get; set; }
        public DbSet<UserPunishment> UserPunishments { get; set; }
        public DbSet<UserPunishmentPaid> UserPunishmentPaids { get; set; }
        public DbSet<UserPunishmentBalance> UserPunishmentBalances { get; set; }
        public DbSet<UserPunishmentRefund> UserPunishmentRefunds { get; set; }
        public DbSet<UserPunishmentHistory> UserPunishmentHistories { get; set; }
        public DbSet<RemoteBlacklist> RemoteBlacklists { get; set; }
        public DbSet<WhitelistSystem> WhitelistSystems { get; set; }
        public DbSet<UserWhitelist> UserWhitelists { get; set; }
        public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserPunishmentRefund to avoid cascade delete conflicts
            modelBuilder.Entity<UserPunishmentRefund>()
                .HasOne(r => r.UserPunishment)
                .WithMany()
                .HasForeignKey(r => r.UserPunishmentId)
                .OnDelete(DeleteBehavior.Restrict); // No cascade delete

            modelBuilder.Entity<UserPunishmentRefund>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // No cascade delete

            modelBuilder.Entity<UserPunishmentBalance>()
                .HasIndex(b => b.UserId)
                .IsUnique();
        }
    }
}