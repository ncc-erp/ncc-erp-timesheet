using Abp.BackgroundJobs;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.MyWorkingTimes.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using System.Text.RegularExpressions;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Authorization;
using Timesheet.DomainServices.Dto;
using JetBrains.Annotations;
using Abp.AutoMapper;
using Timesheet.Uitls;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Ncc.Entities;
using Microsoft.EntityFrameworkCore;
using Ncc.IoC;
using Timesheet.Services.Komu;

namespace Timesheet.APIs.MyWorkingTimes
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime)]
    public class MyWorkingTimeAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly KomuService _komuService;

        public MyWorkingTimeAppService(IBackgroundJobManager backgroundJobManager, KomuService komuService, IWorkScope workScope) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _komuService = komuService;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime_View)]
        public async Task<GetMyWorkingTimeDto> GetMyCurrentWorkingTime()
        {
            var user = await WorkScope.GetAsync<User>(AbpSession.UserId.Value);

            return new GetMyWorkingTimeDto {
            AfternoonEndTime =  user.AfternoonEndAt,
            AfternoonStartTime = user.AfternoonStartAt,
            AfternoonWorkingTime = user.AfternoonWorking.Value,
            MorningEndTime = user.MorningEndAt,
            MorningStartTime = user.MorningStartAt,
            MorningWorkingTime = user.MorningWorking.Value
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime_RegistrationTime)]
        [HttpPost]
        public async Task<ChangeWorkingTimeDto> SubmitNewWorkingTime(ChangeWorkingTimeDto newData)
        {
            long id = AbpSession.UserId.Value;

            var requester = GetSessionUserInfoDto();

            if (newData.MorningWorkingTime + newData.AfternoonWorkingTime < 8)
            {
                throw new UserFriendlyException("Total working time min 8 hours");
            }

            if (newData.ApplyDate == null)
            {
                throw new UserFriendlyException("Apply date cannot be null");
            }

            if (string.IsNullOrEmpty(newData.MorningStartTime) ||
                string.IsNullOrEmpty(newData.MorningEndTime) ||
                newData.MorningWorkingTime <= 0d ||
                string.IsNullOrEmpty(newData.AfternoonStartTime) ||
                string.IsNullOrEmpty(newData.AfternoonEndTime) ||
                newData.AfternoonWorkingTime <= 0d)
            {
                throw new UserFriendlyException("Working time need to be completed");
            }

            string stringRegex = "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$";

            if(!Regex.IsMatch(newData.MorningStartTime, stringRegex) ||
                !Regex.IsMatch(newData.MorningEndTime, stringRegex) ||               
                !Regex.IsMatch(newData.AfternoonStartTime, stringRegex) ||
                !Regex.IsMatch(newData.AfternoonEndTime, stringRegex) )
            {
                throw new UserFriendlyException("Wrong working time format must be (HH:mm)");
            } 

            if (newData.ApplyDate.Date <= DateTimeUtils.GetNow().Date)
            {
                throw new UserFriendlyException("Apply time has to greater today");
            }

            var data = new HistoryWorkingTime();
            data.UserId = id;
            data.MorningStartTime = newData.MorningStartTime;
            data.MorningEndTime = newData.MorningEndTime;
            data.MorningWorkingTime = newData.MorningWorkingTime;
            data.AfternoonStartTime = newData.AfternoonStartTime;
            data.AfternoonEndTime = newData.AfternoonEndTime;
            data.AfternoonWorkingTime = newData.AfternoonWorkingTime;
            data.ApplyDate = newData.ApplyDate;
            data.Status = RequestStatus.Pending;
            newData.Id = await WorkScope.GetRepo<HistoryWorkingTime>().InsertAndGetIdAsync(data);

            await notifySubmitNewMyWorkingTime(requester, newData);

            return newData;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime_View)]
        public List<HistoryWorkingTimeDto> GetAllMyHistoryWorkingTime()
        {
            long id = AbpSession.UserId.Value;
            var historyWorkingTime = WorkScope.GetAll<HistoryWorkingTime>()
                .Where(history => history.UserId == id)
                .Select(h => new HistoryWorkingTimeDto
                {
                    Id = h.Id,
                    MorningStartTime = h.MorningStartTime,
                    MorningEndTime = h.MorningEndTime,
                    MorningWorkingTime = h.MorningWorkingTime,
                    AfternoonStartTime = h.AfternoonStartTime,
                    AfternoonEndTime = h.AfternoonEndTime,
                    AfternoonWorkingTime = h.AfternoonWorkingTime,
                    ApplyDate = h.ApplyDate,
                    ReqestTime = h.CreationTime,
                    Status = h.Status
                })
                .OrderByDescending(s => s.ReqestTime)
                .ToList();

            return historyWorkingTime;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime_Delete)]
        [HttpDelete]
        public async System.Threading.Tasks.Task DeleteWorkingTime(long id)
        {
            var item = await WorkScope.GetAsync<HistoryWorkingTime>(id);

            if (item == null)
            {
                throw new UserFriendlyException("This working time does not exist");
            }

            if (item.Status == RequestStatus.Approved)
            {
                throw new UserFriendlyException("This working time is approved");
            }

            await WorkScope.DeleteAsync<HistoryWorkingTime>(item);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyWorkingTime_Edit)]
        [HttpPost]
        public async Task<HistoryWorkingTimeDto> EditWorkingTime(HistoryWorkingTimeDto newData)
        {
            long id = AbpSession.UserId.Value;
            var requester = GetSessionUserInfoDto();
            if (newData.ApplyDate == null)
            {
                throw new UserFriendlyException("Apply date cannot be null");
            }

            if (newData.Status == RequestStatus.Approved)
            {
                throw new UserFriendlyException("This working time is approved");
            }

            string stringRegex = "^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$";

            if (!Regex.IsMatch(newData.MorningStartTime, stringRegex) ||
                !Regex.IsMatch(newData.MorningEndTime, stringRegex) ||                
                !Regex.IsMatch(newData.AfternoonStartTime, stringRegex) ||
                !Regex.IsMatch(newData.AfternoonEndTime, stringRegex) 
                )
            {
                throw new UserFriendlyException("Wrong working time format must be (HH:mm)");
            }

            var data = new HistoryWorkingTime();
            data.Id = newData.Id;
            data.UserId = id;
            data.MorningStartTime = newData.MorningStartTime;
            data.MorningEndTime = newData.MorningEndTime;
            data.MorningWorkingTime = newData.MorningWorkingTime;
            data.AfternoonStartTime = newData.AfternoonStartTime;
            data.AfternoonEndTime = newData.AfternoonEndTime;
            data.AfternoonWorkingTime = newData.AfternoonWorkingTime;
            data.ApplyDate = newData.ApplyDate;
            data.Status = RequestStatus.Pending;
            data.CreationTime = DateTime.Now;

            await WorkScope.GetRepo<HistoryWorkingTime>().UpdateAsync(data);

            await notifySubmitNewMyWorkingTime(requester, newData);

            return newData;
        }

        private async System.Threading.Tasks.Task notifySubmitNewMyWorkingTime(NotifyUserInfoDto requester, ChangeWorkingTimeDto input)
        {
            var receivers = await getReceiverListSubmit(requester.UserId);

            await notifyKomuWhenSubmitRequest(requester, input, receivers);
        }

        private async System.Threading.Tasks.Task notifyKomuWhenSubmitRequest(NotifyUserInfoDto requester, ChangeWorkingTimeDto input, List<ProjectPMDto> receivers)
        {
            var enableNotify = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest);
            if (enableNotify != "true")
            {
                Logger.Info("notifyKomuWhenSubmitRequest() SendKomuRequest=" + enableNotify + ", AbpSessionUserId=" + AbpSession.UserId);
                return;
            }

            var alreadySentToPMIds = new HashSet<long>();
            foreach (var project in receivers)
            {
                if (!project.IsNotifyKomu)
                {
                    Logger.Info($"notifyKomuWhenSubmitRequest() projectId={project.ProjectId}, IsNotifyKomu={project.IsNotifyKomu}, KomuChannelId={project.KomuChannelId}");
                }
                else
                {
                    var komuMessage = $"PM {project.KomuPMsTag(alreadySentToPMIds)}: {requester.KomuAccountInfo} " +
                        $"has sent a request to change working time:" +
                        $"\n ```RequestId: #{input.Id}" +
                        $"\nMorning: {input.MorningStartTime} - {input.MorningEndTime}" +
                        $"\nAfternoon: {input.AfternoonStartTime} - {input.AfternoonEndTime}" +
                        $"\nApply date: {input.ApplyDate.ToString("dd/MM/yyyy")}```";

                    _komuService.NotifyToChannel(komuMessage, project.KomuChannelId);
                    processAlreadySentToPMs(alreadySentToPMIds, project.PMs);
                }
            }
        }

        private void processAlreadySentToPMs(HashSet<long> alreadySentToPMIds, List<NotifyUserInfoDto> PMs)
        {
            foreach (var pm in PMs)
            {
                if (!alreadySentToPMIds.Contains(pm.UserId))
                {
                    alreadySentToPMIds.Add(pm.UserId);
                }
            }
        }

        public async Task<List<ProjectPMDto>> getReceiverListSubmit(long requesterId)
        {
            var qrequesterInProjectIds = WorkScope.GetAll<ProjectUser>()
            .Where(s => s.UserId == requesterId)
            .Where(s => s.Project.Status == ProjectStatus.Active)
            .Where(s => s.Type != ProjectUserType.DeActive)
            .Select(s => s.ProjectId)
            .Distinct();

            var queryPMs = WorkScope.GetAll<ProjectUser>()
            .Where(s => s.Project.Status == ProjectStatus.Active)
            .Where(s => s.Type == ProjectUserType.PM)
            .Select(s => new
            {
                s.User.KomuUserId,
                s.User.FullName,
                s.User.EmailAddress,
                s.UserId,
                s.ProjectId,
                s.Project.KomuChannelId,
                s.Project.IsNotifyToKomu,
                s.Project.IsNoticeKMRequestChangeWorkingTime,
            });

            var result = await (from projectId in qrequesterInProjectIds
                                join pm in queryPMs on projectId equals pm.ProjectId
                                select pm)
                              .GroupBy(s => new { s.ProjectId, s.KomuChannelId, s.IsNoticeKMRequestChangeWorkingTime })
                              .Select(s => new ProjectPMDto
                              {
                                  ProjectId = s.Key.ProjectId,
                                  KomuChannelId = s.Key.KomuChannelId,
                                  IsNotifyKomu = s.Key.IsNoticeKMRequestChangeWorkingTime,
                                  IsNotifyEmail = false,
                                  PMs = s.Select(x => new NotifyUserInfoDto
                                  {
                                      EmailAddress = x.EmailAddress,
                                      FullName = x.FullName,
                                      KomuUserId = x.KomuUserId,
                                      UserId = x.UserId
                                  }).ToList()
                              }).ToListAsync();
            return result;
        }
    }
}
