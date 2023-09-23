using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Entities.Auditing;
using Ganss.Excel;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Timesheet.Entities;

namespace Timesheet.Tests.Seeders
{
    public class DataSeederConsumer
    {
        private List<TEntity> Create<TEntity, TKey>(string fileName, List<User> users = default(List<User>), Dictionary<string, string> customFields = default(Dictionary<string, string>), List<string> ignoreFields = default(List<string>))
            where TEntity : FullAuditedEntity<TKey>
        {
            string path = Path.Combine(Directory.GetCurrentDirectory().Replace("\\bin\\Debug\\netcoreapp3.1", ""), "Tables", fileName).Replace("/bin/Debug/netcoreapp3.1", "");
            var excel = new ExcelMapper(path);
            if (customFields != null)
            {
                foreach (var item in customFields)
                {
                    excel.AddMapping(typeof(TEntity), item.Key, item.Value);
                }
            }
            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }
            ignoreFields.Add("Id");

            foreach (var item in ignoreFields)
            {
                excel.Ignore(typeof(TEntity), item);
            }

            var data = excel.Fetch<TEntity>().ToList();

            var first = data.FirstOrDefault();
            if (first == null)
            {
                return new List<TEntity>();
            }

            PropertyInfo propertyInfo = first.GetType().GetProperty("CreatorUser");
            PropertyInfo propertyInfo2 = first.GetType().GetProperty("LastModifierUser");

            if (propertyInfo != null && propertyInfo2 != null)
            {
                if (users != null)
                {
                    foreach (var item in data)
                    {
                        propertyInfo.SetValue(item, Convert.ChangeType(users.FirstOrDefault(x => x.Id == item.CreatorUserId), propertyInfo.PropertyType), null);
                        propertyInfo2.SetValue(item, Convert.ChangeType(users.FirstOrDefault(x => x.Id == item.LastModifierUserId), propertyInfo2.PropertyType), null);
                    }
                }
            }
            return data;
        }

        private List<TEntity> CreateAuditedEntity<TEntity, TKey>(string fileName, List<User> users = default(List<User>), List<string> ignoreFields = default(List<string>))
                    where TEntity : AuditedEntity<TKey>

        {
            string path = Path.Combine(Directory.GetCurrentDirectory().Replace("\\bin\\Debug\\netcoreapp3.1", ""), "Tables", fileName).Replace("/bin/Debug/netcoreapp3.1", "");
            var excel = new ExcelMapper(path);

            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }
            ignoreFields.Add("Id");

            foreach (var item in ignoreFields)
            {
                excel.Ignore(typeof(TEntity), item);
            }

            var data = excel.Fetch<TEntity>().ToList();
            var first = data.FirstOrDefault();
            if (first == null)
            {
                return new List<TEntity>();
            }

            PropertyInfo propertyInfo = first.GetType().GetProperty("CreatorUser");
            PropertyInfo propertyInfo2 = first.GetType().GetProperty("LastModifierUser");

            if (propertyInfo != null && propertyInfo2 != null)
            {
                if (users != null)
                {
                    foreach (var item in data)
                    {
                        propertyInfo.SetValue(item, Convert.ChangeType(users.FirstOrDefault(x => x.Id == item.CreatorUserId), propertyInfo.PropertyType), null);
                        propertyInfo2.SetValue(item, Convert.ChangeType(users.FirstOrDefault(x => x.Id == item.LastModifierUserId), propertyInfo2.PropertyType), null);
                    }
                }
            }
            return data;
        }
        private List<TEntity> MapFK<TEntity, FKEntity>(List<TEntity> data, List<FKEntity> fkData, string fieldNeedMap, string fkField)
            where TEntity : FullAuditedEntity<long>
            where FKEntity : FullAuditedEntity<long>
        {
            foreach (var item in data)
            {
                var value = (long?)item.GetType().GetProperty(fkField).GetValue(item, null);
                var fk = fkData.FirstOrDefault(x => x.Id == value);
                PropertyInfo propertyInfo = item.GetType().GetProperty(fieldNeedMap);
                propertyInfo.SetValue(item, Convert.ChangeType(fk, propertyInfo.PropertyType), null);
            }
            return data;
        }
        private List<TEntity> MapICollection<TEntity, ICEntity>(List<TEntity> data, List<ICEntity> icData, string fieldNeedMap, string fkField)
            where TEntity : FullAuditedEntity<long>
            where ICEntity : FullAuditedEntity<long>
        {
            foreach (var item in data)
            {
                var ic = icData.Where(x => (long)x.GetType().GetProperty(fkField).GetValue(x, null) == item.Id).ToList();
                if (ic.Count == 0)
                {
                    ic = null;
                }
                PropertyInfo propertyInfo = item.GetType().GetProperty(fieldNeedMap);
                propertyInfo.SetValue(item, ic);
            }
            return data;
        }
        public void Seed(TimesheetDbContext context)
        {
            //Positions
            var Positions = Create<Position, long>("dbo.Positions.xlsx");
            context.Positions.AddRange(Positions);

            // Branchs
            var Branchs = Create<Branch, long>("dbo.Branchs.xlsx");
            context.Branchs.AddRange(Branchs);

            //"Users",\
            var Users = Create<User, long>("dbo.AbpUsers.xlsx", null, null, new List<string> { "Tokens", "Logins", "Roles", "Claims", "Permissions", "Settings", "Branch","Position", "Manager" });
            Users = MapFK(Users, Branchs, "Branch", "BranchId");
            Users = MapFK(Users, Positions, "Position", "PositionId");
            Users = MapFK(Users, Users, "Manager", "ManagerId");
            context.Users.AddRange(Users);

            // Roles
            var Roles = Create<Role, int>("dbo.AbpRoles.xlsx", null, null, new List<string> { "CreatorUser", "LastModifierUser", "DeleterUser" });
            context.Roles.AddRange(Roles);

            //"Retros",\
            var Retros = Create<Retro, long>("dbo.Retros.xlsx");
            context.Retros.AddRange(Retros);

            //"Customers",\
            var Customers = Create<Customer, long>("dbo.Customers.xlsx");
            context.Customers.AddRange(Customers);

            //"Tasks",
            var Tasks = Create<Ncc.Entities.Task, long>("dbo.Tasks.xlsx");
            context.Tasks.AddRange(Tasks);

            //"ProjectTargetUsers",\
            var ProjectTargetUsers = Create<ProjectTargetUser, long>("dbo.ProjectTargetUsers.xlsx");
            context.ProjectTargetUsers.AddRange(ProjectTargetUsers);

            //"DayOffSettings",\
            var DayOffSettings = Create<DayOffSetting, long>("dbo.DayOffSettings.xlsx");
            context.DayOffSettings.AddRange(DayOffSettings);

            //"DayOffTypes",\
            var DayOffTypes = Create<DayOffType, long>("dbo.DayOffTypes.xlsx");
            context.DayOffTypes.AddRange(DayOffTypes);

            //"UserUnlockIms",\
            var UserUnlockIms = Create<UserUnlockIms, long>("dbo.UserUnlockIms.xlsx");
            context.UserUnlockIms.AddRange(UserUnlockIms);

            ////"Funds",\
            //var Funds = Create<Fund, long>("dbo.Funds.xlsx");
            //context.Funds.AddRange(Funds);

            //"ReviewInterns",\
            var ReviewInterns = Create<ReviewIntern, long>("dbo.ReviewInterns.xlsx");
            context.ReviewInterns.AddRange(ReviewInterns);

            //"Projects",\
            var Projects = Create<Project, long>("dbo.Projects.xlsx");
            Projects = MapFK(Projects, Customers, "Customer", "CustomerId");
            context.Projects.AddRange(Projects);

            //"ProjectUsers",\
            var ProjectUsers = Create<ProjectUser, long>("dbo.ProjectUsers.xlsx");
            ProjectUsers = MapFK(ProjectUsers, Users, "User", "UserId");
            ProjectUsers = MapFK(ProjectUsers, Projects, "Project", "ProjectId");

            context.ProjectUsers.AddRange(ProjectUsers);

            //"TaskProjects",\
            var TaskProjects = Create<ProjectTask, long>("dbo.TaskProjects.xlsx");
            TaskProjects = MapFK(TaskProjects, Tasks, "Task", "TaskId");
            TaskProjects = MapFK(TaskProjects, Projects, "Project", "ProjectId");

            context.TaskProjects.AddRange(TaskProjects);

            //"MyTimesheets",\
            var MyTimesheets = Create<MyTimesheet, long>("dbo.MyTimesheets.xlsx");
            MyTimesheets = MapFK(MyTimesheets, TaskProjects, "ProjectTask", "ProjectTaskId");
            MyTimesheets = MapFK(MyTimesheets, Users, "User", "UserId");
            MyTimesheets = MapFK(MyTimesheets, ProjectTargetUsers, "ProjectTargetUser", "ProjectTargetUserId");

            context.MyTimesheets.AddRange(MyTimesheets);

            //"AbsenceDayRequest",\
            var AbsenceDayRequests = Create<AbsenceDayRequest, long>("dbo.AbsenceDayRequests.xlsx");
            AbsenceDayRequests = MapFK(AbsenceDayRequests, Users, "User", "UserId");
            AbsenceDayRequests = MapFK(AbsenceDayRequests, DayOffTypes, "DayOffType", "DayOffTypeId");

            context.AbsenceDayRequests.AddRange(AbsenceDayRequests);

            //"AbsenceDayDetails",\
            var AbsenceDayDetails = Create<AbsenceDayDetail, long>("dbo.AbsenceDayDetails.xlsx");
            AbsenceDayDetails = MapFK(AbsenceDayDetails, AbsenceDayRequests, "Request", "RequestId");

            context.AbsenceDayDetails.AddRange(AbsenceDayDetails);

            //"UnlockTimesheets",\
            var UnlockTimesheets = Create<UnlockTimesheet, long>("dbo.UnlockTimesheets.xlsx");
            UnlockTimesheets = MapFK(UnlockTimesheets, Users, "User", "UserId");

            context.UnlockTimesheets.AddRange(UnlockTimesheets);

            //"Timekeeping",\
            var Timekeepings = Create<Timekeeping, long>("dbo.Timekeepings.xlsx");
            Timekeepings = MapFK(Timekeepings, Users, "User", "UserId");

            context.Timekeepings.AddRange(Timekeepings);

            //"HistoryWorkingTimes",\
            var HistoryWorkingTimes = Create<HistoryWorkingTime, long>("dbo.HistoryWorkingTimes.xlsx");
            HistoryWorkingTimes = MapFK(HistoryWorkingTimes, Users, "User", "UserId");

            context.HistoryWorkingTimes.AddRange(HistoryWorkingTimes);

            //"ReviewDetails",\
            var ReviewDetails = Create<ReviewDetail, long>("dbo.ReviewDetails.xlsx");
            ReviewDetails = MapFK(ReviewDetails, ReviewInterns, "Review", "ReviewId");
            ReviewDetails = MapFK(ReviewDetails, Users, "InterShip", "InternshipId");
            ReviewDetails = MapFK(ReviewDetails, Users, "Reviewer", "ReviewerId");

            context.ReviewDetails.AddRange(ReviewDetails);

            //"RetroResults",\
            var RetroResults = Create<RetroResult, long>("dbo.RetroResults.xlsx");
            RetroResults = MapFK(RetroResults, Retros, "Retro", "RetroId");
            RetroResults = MapFK(RetroResults, Users, "User", "UserId");
            RetroResults = MapFK(RetroResults, Positions, "Position", "PositionId");
            RetroResults = MapFK(RetroResults, Projects, "Project", "ProjectId");
            RetroResults = MapFK(RetroResults, Branchs, "Branch", "BranchId");
            RetroResults = MapFK(RetroResults, Users, "Pm", "PmId");

            context.RetroResults.AddRange(RetroResults);


            //CapabilitySettings
            var CapabilitySettings = Create<CapabilitySetting, long>("dbo.CapabilitySettings.xlsx", null, null, new List<string> { "Position", "Capability" });

            //Capabilities
            var Capabilities = Create<Capability, long>("dbo.Capabilities.xlsx", null, null, new List<string> { "CapabilitySettings" });
            Capabilities = MapICollection(Capabilities, CapabilitySettings, "CapabilitySettings", "CapabilityId");
            context.Capabilities.AddRange(Capabilities);

            CapabilitySettings = MapFK(CapabilitySettings, Capabilities, "Capability", "CapabilityId");
            CapabilitySettings = MapFK(CapabilitySettings, Positions, "Position", "PositionId");
            context.CapabilitySettings.AddRange(CapabilitySettings);

            //"OverTimeSettings",\
            var OverTimeSettings = Create<OverTimeSetting, long>("dbo.OverTimeSettings.xlsx");
            OverTimeSettings = MapFK(OverTimeSettings, Projects, "Project", "ProjectId");

            context.OverTimeSettings.AddRange(OverTimeSettings);

            // KomuTracker
            var KomuTrackers = Create<KomuTracker, long>("dbo.KomuTrackers.xlsx");

            context.KomuTrackers.AddRange(KomuTrackers);

            var Setting = CreateAuditedEntity<Setting, long>("dbo.AbpSettings.xlsx");
            context.Settings.AddRange(Setting);

            //"ReviewInternCapabilities",\
            var ReviewInternCapabilities = Create<ReviewInternCapability, long>("dbo.ReviewInternCapabilities.xlsx");
            ReviewInternCapabilities = MapFK(ReviewInternCapabilities, ReviewDetails, "ReviewDetail", "ReviewDetailId");
            ReviewInternCapabilities = MapFK(ReviewInternCapabilities, Capabilities, "Capability", "CapabilityId");

            context.ReviewInternCapabilities.AddRange(ReviewInternCapabilities);

            //"UserRoles",\
            /*var UserRoles = CreateCreationAuditedEntity<UserRole, long>("dbo.AbpUserRoles.xlsx",null,null,new List<string> {"User","Role"});
            //UserRoles = MapFK(UserRoles, Users, "User", "UserId");
            UserRoles = MapAbpRole(UserRoles, Roles, "Role", "RoleId");
            context.UserRoles.AddRange(UserRoles);*/

            //TeamBuildingRequestHistory
            var TeamBuildingRequestHistories = Create<TeamBuildingRequestHistory, long>("dbo.TeamBuildingRequestHistories.xlsx");
            TeamBuildingRequestHistories = MapFK(TeamBuildingRequestHistories, Users, "Requester", "RequesterId");
            context.TeamBuildingRequestHistories.AddRange(TeamBuildingRequestHistories);
            //TeamBuildingDetails
            var TeamBuildingDetails = Create<TeamBuildingDetail, long>("dbo.TeamBuildingDetails.xlsx");
            TeamBuildingDetails = MapFK(TeamBuildingDetails, Projects, "Project", "ProjectId");
            TeamBuildingDetails = MapFK(TeamBuildingDetails, Users, "Employee", "EmployeeId");
            TeamBuildingDetails = MapFK(TeamBuildingDetails, TeamBuildingRequestHistories, "TeamBuildingRequestHistory", "TeamBuildingRequestHistoryId");
            context.TeamBuildingDetails.AddRange(TeamBuildingDetails);

            //RequestHistoryFiles
            var TeamBuildingRequestHistoryFiles = Create<TeamBuildingRequestHistoryFile, long>("dbo.TeamBuildingRequestHistoryFiles.xlsx");
             TeamBuildingRequestHistoryFiles = MapFK(TeamBuildingRequestHistoryFiles, TeamBuildingRequestHistories, "TeamBuildingRequestHistory", "TeamBuildingRequestHistoryId");
            context.TeamBuildingRequestHistoryFiles.AddRange(TeamBuildingRequestHistoryFiles);

            context.SaveChanges();
        }
    }
}
