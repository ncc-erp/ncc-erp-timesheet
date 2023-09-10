using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Timesheet.APIs.HRM;
using Timesheet.APIs.OverTimeHours;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Komu;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Info
{
    public class HRMAppServiceTestBase : TimesheetApplicationTestBase
    {
        private List<MyTimesheet> listNewMyTimeSheet = new List<MyTimesheet>
        {
            new MyTimesheet
            {
                Id=100,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,27),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=6,
            },
            new MyTimesheet
            {
                Id=101,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,26),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=102,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,28),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=9,
            },
            new MyTimesheet
            {
                Id=103,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,29),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
            },
            new MyTimesheet
            {
                Id=104,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,30),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=13,
            },
            new MyTimesheet
            {
                Id=105,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,31),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=106,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,24),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=107,
                ProjectTaskId = 14,
                Note="",
                WorkingTime=480,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,19),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=1,
            },
            new MyTimesheet
            {
                Id=108,
                ProjectTaskId = 1,
                Note="",
                WorkingTime=240,
                TargetUserWorkingTime=0,
                TypeOfWork=TypeOfWork.OverTime,
                IsCharged=false,
                DateAt=new DateTime(2022,12,01),
                Status=TimesheetStatus.Approve,
                ProjectTargetUserId=1,
                IsTemp=false,
                UserId=10,
            }
        };
        public HRMAppService InstanceHRMAppService() 
        {
            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            var settingManager = Substitute.For<ISettingManager>();

            var userManager = Resolve<UserManager>();

            var loggerKomu = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("_isNotifyToKomu");
            var komuService = Substitute.For<KomuService>(httpClient, loggerKomu, configuration, settingManager);
            var abpSession = Resolve<IAbpSession>();
            var roleManager = Resolve<RoleManager>();
            var objectMapper = Resolve<IObjectMapper>();
            var workScope = Resolve<IWorkScope>();

            var _userServices = Substitute.For<UserServices>(userManager, komuService, abpSession, roleManager, objectMapper, workScope);
            _userServices.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            _userServices.ObjectMapper = Resolve<IObjectMapper>();

            var mockOverTimeHour = Substitute.For<OverTimeHourAppService>(workScope);

            var hrmAppService = new HRMAppService(
                mockOverTimeHour,
                _userServices,
                workScope);
            hrmAppService.SettingManager = Resolve<ISettingManager>();
            hrmAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            hrmAppService.ObjectMapper = Resolve<IObjectMapper>();

            foreach (var ts in listNewMyTimeSheet)
            {
                workScope.InsertAsync(ts);
            }

            return hrmAppService;
        }
    }
}
