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
        public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options)
            : base(options)
        {
        }
    }
}