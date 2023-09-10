using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Linq.Extensions;
using Abp.UI;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.Positions.Dto;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Helper;
using Timesheet.Paging;
using Timesheet.Services.Project;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingDetails
{
    public class TeamBuildingDetailsAppService : AppServiceBase
    {
        public static ProjectService _projectService;
        private readonly ILogger<TeamBuildingDetailsAppService> _logger;
        private readonly GenerateDataTeamBuildingServices _generateDataTeamBuildingServices;
        public TeamBuildingDetailsAppService(ProjectService projectService, ILogger<TeamBuildingDetailsAppService> logger, GenerateDataTeamBuildingServices generateDataTeamBuildingServices, IWorkScope workScope) : base(workScope)
        {
            _projectService = projectService;
            _logger = logger;
            _generateDataTeamBuildingServices = generateDataTeamBuildingServices;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailHR_GenerateData)]
        public void AddDataToTeamBuildingDetail(InputGenerateDataTeamBuildingDto input)
        {
            _generateDataTeamBuildingServices.AddDataToTeamBuildingDetail(input);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailHR_ViewAllProject)]
        public async Task<GridResult<GetTeamBuildingDetailDto>> GetAllPagging(InputFilterTeamBuildingDetailPagingDto input)
        {
            var pmFromProject = _projectService.GetProjectPMName();

            var dicUsers = WorkScope.GetAll<User>()
                                    .Select(s => new
                                    {
                                        s.Id,
                                        s.FullName,
                                    })
                                    .ToDictionary(s => s.Id, s => s.FullName);

            var qTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                .WhereIf(input.Status.HasValue, s => s.Status == input.Status.Value)
                .Where(s => s.ApplyMonth.Year == input.Year)
                .WhereIf(input.Month.HasValue && input.Month != -1, s => s.ApplyMonth.Month == input.Month)
                .Select(s => new GetTeamBuildingDetailDto
                {
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                    ProjectCode = s.Project.Code,
                    EmployeeId = s.EmployeeId,
                    PMEmailAddress = pmFromProject.Where(x => x.ProjectCode == s.Project.Code).Select(x => x.PMEmail).FirstOrDefault(),
                    Id = s.Id,
                    EmployeeFullName = s.Employee.FullName,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    ApplyMonth = s.ApplyMonth,
                    RequesterId = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.RequesterId : default,
                    RequesterEmailAddress = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.EmailAddress : default,
                    RequesterFullName = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.FullName : default,
                    LastModifierTime = s.LastModificationTime,
                    CreationTime = s.CreationTime,
                    CreatedUserName = (s.CreatorUserId.HasValue && dicUsers.ContainsKey(s.CreatorUserId.Value)) ? dicUsers[s.CreatorUserId.Value] : "",
                    LastModifierUserName = (s.LastModifierUserId.HasValue && dicUsers.ContainsKey(s.LastModifierUserId.Value)) ? dicUsers[s.LastModifierUserId.Value] : "",
                    Status = s.Status,
                    Money = s.Money,
                })
                .OrderBy(s => s.CreationTime)
                .OrderBy(s => s.ApplyMonth.Month)
                .OrderBy(s => s.EmployeeEmailAddress);

            return await qTeamBuildingDetail.GetGridResult(qTeamBuildingDetail, input.GridParam);
        }

        [HttpGet]
        public async Task<List<GetAllRequesterEmailAddressInTeamBuildingDetailDto>> GetAllRequesterEmailAddressInTeamBuildingDetail()
        {
            var listRequest = await WorkScope.GetAll<TeamBuildingDetail>().Where(s => s.TeamBuildingRequestHistoryId != null)
                .Select(s => s.TeamBuildingRequestHistory.RequesterId).Distinct().ToListAsync();

            var emailAddressByUserId = WorkScope.GetAll<TeamBuildingRequestHistory>().Where(s => listRequest.Contains(s.RequesterId))
                .Select(s => new GetAllRequesterEmailAddressInTeamBuildingDetailDto
                {
                    RequesterEmailAddress = s.Requester.EmailAddress,
                    RequesterId = s.RequesterId,
                }).Distinct().ToListAsync();

            return await emailAddressByUserId;
        }

        [HttpGet]
        public async Task<List<GetAllProjectInTeamBuildingDetailDto>> GetAllProjectInTeamBuildingDetail()
        {
            var listProjectId = await WorkScope.GetAll<TeamBuildingDetail>()
                .Select(s => s.ProjectId).Distinct().ToListAsync();

            var projectInfoById = WorkScope.GetAll<Project>().Where(s => listProjectId.Contains(s.Id))
                .Select(s => new GetAllProjectInTeamBuildingDetailDto
                {
                    ProjectId = s.Id,
                    ProjectName = s.Name,
                }).ToListAsync();

            return await projectInfoById;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailHR_Management)]
        public async Task<CreateTeamBuildingDetailDto> AddNew(CreateTeamBuildingDetailDto input)
        {
            var userInTeamBuilding = WorkScope.GetAll<User>().FirstOrDefault(s => s.Id == input.EmployeeId);
            var existedTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.EmployeeId == input.EmployeeId && s.ApplyMonth.Month == input.Month && s.ApplyMonth.Year == input.Year)
                .Any();

            if (userInTeamBuilding == default)
                throw new UserFriendlyException("Not found employee by employee id");
            if (userInTeamBuilding.Level <= UserLevel.Intern_2)
                throw new UserFriendlyException("Cannot add employee because the employee's level is less than intern 2");
            if (userInTeamBuilding.IsActive == false || userInTeamBuilding.IsDeleted == true)
                throw new UserFriendlyException("This employee has retired");
            if (existedTeamBuildingDetail)
                throw new UserFriendlyException($"This employee already has a record in {input.Month}/{input.Year}");
            if (input.Month > DateTimeUtils.GetNow().Month && input.Year >= DateTimeUtils.GetNow().Year)
                throw new UserFriendlyException("The selected month cannot greater than the current month!");

            double moneyDefault;
            var parseResult = double.TryParse(SettingManager.GetSettingValueForApplication(AppSettingNames.TeamBuildingMoney), out moneyDefault);
            if (!parseResult)
            {
                _logger.LogError($"{AppSettingNames.TeamBuildingMoney} is not found or invalid data");
                throw new UserFriendlyException("Something error occurs, please contact to administrator for more information");
            }
            var employeeWorking = WorkScope.GetAll<MyTimesheet>()
                               .Select(s => new { 
                                   s.UserId, 
                                   DateAt = s.DateAt.Date, 
                                   s.TypeOfWork, 
                                   s.Status, 
                                   s.ProjectTask.ProjectId, 
                                   s.WorkingTime, 
                                   s.User.Level, 
                                   s.ProjectTask.Project.IsAllowTeamBuilding })
                               .Where(ts => ts.Status == TimesheetStatus.Approve)
                               .Where(ts => ts.DateAt.Year == input.Year && ts.DateAt.Month == input.Month)
                               .Where(ts => ts.UserId == input.EmployeeId)
                               .Where(s => s.IsAllowTeamBuilding)
                               .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                               .ToList();

            if(employeeWorking.Count <= 0)
                throw new UserFriendlyException("User does not have working time this month!");

            var requestOff = WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.DateAt.Month == input.Month)
                .Where(s => s.DateAt.Year == input.Year)
                .Where(s => s.Request.Status == RequestStatus.Approved)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.DateType != DayType.Custom)
                .Select(s => new
                {
                    s.Request.UserId,
                    s.DateAt,
                    s.DateType,
                })
                .ToList()
                .GroupBy(s => s.UserId)
                .ToDictionary(s => s.Key,
                              s => s.GroupBy(x => x.DateAt)
                                    .ToDictionary(x => x.Key,
                                                  x => x.Select(z => z.DateType == DayType.Fullday ? 1 : 0.5).FirstOrDefault()
                                                  ));

            var dayWorking = employeeWorking
                .Select(s => new
                {
                    s.UserId,
                    s.DateAt
                })
                .Distinct()
                .GroupBy(s => s.UserId)
                .Select(s => new
                {
                    UserId = s.Key,
                    DayWorking = s.Select(x => !requestOff.ContainsKey(s.Key) ? 1 : !requestOff[s.Key].ContainsKey(x.DateAt) ? 1 : (1 - requestOff[s.Key][x.DateAt])).Sum()
                })
                .ToList();

            var mostProjectEmployeeWorking = employeeWorking
                .Where(s => s.IsAllowTeamBuilding)
                .Select(s => new
                {
                    s.UserId,
                    s.ProjectId,
                    s.WorkingTime
                })
                .GroupBy(s => s.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    WorkingTime = g.Sum(s => s.WorkingTime),
                    MaxSumWorkingTimeProjectId = g
                    .GroupBy(s => s.ProjectId)
                    .Select(x => new
                    {
                        ProjectId = x.Key,
                        SumWorkingTime = x.Sum(s => s.WorkingTime)
                    })
                    .OrderByDescending(x => x.SumWorkingTime)
                    .FirstOrDefault().ProjectId,
                });

            var employeeWorkingMaxDictionary = mostProjectEmployeeWorking
                .ToDictionary(e => e.UserId, e => e.MaxSumWorkingTimeProjectId);

            var project = dayWorking.Select(s => employeeWorkingMaxDictionary[s.UserId]).FirstOrDefault();

            var standardDay = _generateDataTeamBuildingServices.GetListStandardDay(input.Year, input.Month);

            var money = (float)(moneyDefault * dayWorking.Select(s => s.DayWorking).FirstOrDefault() / standardDay);
            var UpdateTeamBuildingDetails = new TeamBuildingDetail
            {
                ApplyMonth = new DateTime(input.Year, input.Month, 1),
                EmployeeId = input.EmployeeId,
                ProjectId = project,
                Status = TeamBuildingStatus.Open,
                Money = money > 100000 ? 100000 : money,
            };
            await WorkScope.InsertAsync(UpdateTeamBuildingDetails);
            return input;
        }
        public async Task<List<GetProjectTeamBuildingDto>> GetAllProjectTeamBuilding()
        {
            return await WorkScope.GetAll<ProjectUser>()
                .Where(x => x.Project.IsAllowTeamBuilding)
                .Select(s => new GetProjectTeamBuildingDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                }).Distinct().OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<List<GetEmployeeTeamBuildingDto>> GetAllEmployeeTeamBuilding()
        {
            return await WorkScope.GetAll<User>()
                .Where(s => s.IsActive || !s.IsDeleted)
                .Where(x => x.Level >= UserLevel.Intern_2)
                .Select(x => new GetEmployeeTeamBuildingDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                }).ToListAsync();
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailHR_Management)]
        public async Task<EditMoneyTeamBuildingDetailDto> Update(EditMoneyTeamBuildingDetailDto input)
        {
            var isStatusRequestedOrDone = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.Id == input.Id).Select(s => s.Status).FirstOrDefaultAsync();

            if(isStatusRequestedOrDone == TeamBuildingStatus.Requested || isStatusRequestedOrDone == TeamBuildingStatus.Done)
            {
                throw new UserFriendlyException("Record cannot be Edit when Done or Requested!");
            }
            var maxTeamBuildingMoney = SettingManager.GetSettingValueForApplication(AppSettingNames.TeamBuildingMoney);
            float fmaxTeamBuildingMoney = float.Parse(maxTeamBuildingMoney);

            if (input.Money > fmaxTeamBuildingMoney)
            {
                throw new UserFriendlyException($"Cannot enter an amount greater than {Helpers.FormatMoneyVND(fmaxTeamBuildingMoney)}");
            }
            var item = await WorkScope.GetAsync<TeamBuildingDetail>(input.Id);
            ObjectMapper.Map<EditMoneyTeamBuildingDetailDto, TeamBuildingDetail>(input, item);
            await WorkScope.UpdateAsync(item);

            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailHR_Management)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var isStatusRequestedOrDone = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.Id == input.Id).Select(s => s.Status).FirstOrDefaultAsync();

            if (isStatusRequestedOrDone == TeamBuildingStatus.Requested || isStatusRequestedOrDone == TeamBuildingStatus.Done)
            {
                throw new UserFriendlyException("Record cannot be Delete when Done or Requested!");
            }

            await WorkScope.GetRepo<TeamBuildingDetail>().DeleteAsync(input.Id);
        }
    }
}
