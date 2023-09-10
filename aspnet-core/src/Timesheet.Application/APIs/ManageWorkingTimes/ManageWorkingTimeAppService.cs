using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Timesheet.APIs.ManageWorkingTimes.Dto;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.APIs.MyWorkingTimes.Dto;
using Timesheet.BackgroundJob;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ManageWorkingTimes
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.ManageWorkingTime, Ncc.Authorization.PermissionNames.ManageWorkingTime_ViewAll)]
    public class ManageWorkingTimeAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly KomuService _komuService;


        public ManageWorkingTimeAppService(IBackgroundJobManager backgroundJobManager, IWorkScope workScope, KomuService komuService) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _komuService = komuService;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ManageWorkingTime_ViewDetail, Ncc.Authorization.PermissionNames.ManageWorkingTime_ViewAll)]
        [HttpPost]
        public async Task<List<UserHistoryWorkingTime>> GetAll(FilterDto input)
        {
            var searchText = Regex.Replace(input.userName.Trim().ToLower(), @"\s+", " ");
            var projectMember = await WorkScope.GetAll<ProjectUser>()
                .Where(s => input.projectIds.Contains(s.ProjectId))
                .Where(s => s.Type != ProjectUserType.DeActive)
                .Where(s => s.User.IsActive)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            var qUsers =  WorkScope.GetAll<User>()
                .Select(x => new
                {
                    x.Id,
                    x.EmailAddress
                });

            var result = WorkScope.GetAll<HistoryWorkingTime>()
                .Where(h => projectMember.Contains(h.UserId))
                .Select(h => new UserHistoryWorkingTime
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    FullName = h.User.FullName,
                    UserName = h.User.UserName,
                    AvatarPath = h.User.AvatarPath,
                    BranchDisplayName = h.User.Branch.DisplayName,
                    BranchColor = h.User.Branch.Color,
                    Type = h.User.Type,
                    ReqestTime = h.CreationTime,
                    MorningStartTime = h.MorningStartTime,
                    MorningEndTime = h.MorningEndTime,
                    MorningWorkingTime = h.MorningWorkingTime,
                    AfternoonStartTime = h.AfternoonStartTime,
                    AfternoonEndTime = h.AfternoonEndTime,
                    AfternoonWorkingTime = h.AfternoonWorkingTime,
                    ApplyDate = h.ApplyDate,
                    Status = h.Status,
                    LastModificationTime = h.LastModificationTime,
                    LastModifierUser = qUsers.Where(x => x.Id == h.LastModifierUserId).Select(x => x.EmailAddress).FirstOrDefault()
                })
                .Where(s => string.IsNullOrWhiteSpace(searchText)
                || s.UserName.ToLower().Contains(searchText)
                || s.FullName.ToLower().Contains(searchText))
                .WhereIf(input.status.HasValue && input.status > 0, s => s.Status == input.status)
                .OrderByDescending(s => s.ReqestTime)
                .ToList();

            return result;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ManageWorkingTime_Approval)]
        [HttpPost]
        public async System.Threading.Tasks.Task ApproveWorkingTime(long id)
        {

            var itemExt = await WorkScope.GetAll<HistoryWorkingTime>()
                .Where(s => s.Id == id)
                .Select(s => new { Entity = s, s.User.FullName })
                .FirstOrDefaultAsync();

            var item = itemExt.Entity;

            var projectIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM && s.UserId == AbpSession.UserId)
                .Select(s => s.ProjectId).ToListAsync();

            var isValid = await WorkScope.GetAll<ProjectUser>()
                .AnyAsync(s => s.UserId == item.UserId && projectIds.Contains(s.ProjectId));

            if (!isValid)
            {
                throw new UserFriendlyException($"You are not PM of user {itemExt.FullName}");
            }

            item.Status = RequestStatus.Approved;
            await WorkScope.UpdateAsync<HistoryWorkingTime>(item);

            if (item.ApplyDate.Date <= DateTimeUtils.GetNow().Date)
            {
                await ChangeWorkingTime(item);
            }
            else
            {
                // delete background job
                //var getDeleted = await
                //    (from b in WorkScope.GetAll<BackgroundJobInfo>()
                //     from hd in WorkScope.GetAll<HistoryWorkingTime>().Where(x => x.UserId == item.UserId && x.ApplyDate == item.ApplyDate && x.Id != id)
                //     where b.JobArgs.Contains($"\"Id\":{hd.Id}") && b.JobType.Contains("Timesheet.BackgroundJob.WorkingTimeBackgroundJob")
                //     select new { Id = b.Id }).ToListAsync();
                var today = DateTimeUtils.GetNow().Date;

                var deleteBgrdjs = await WorkScope.GetAll<BackgroundJobInfo>()
                    .Where(s => s.JobType.StartsWith("Timesheet.BackgroundJob.WorkingTimeBackgroundJob") && s.JobArgs.Contains($"\"UserId\":{item.UserId}"))
                    .Select(s => s).ToListAsync();
                var hwtIds = new LinkedList<long>();
                foreach (var bgrd in deleteBgrdjs)
                {
                    await _backgroundJobManager.DeleteAsync(bgrd.Id.ToString());
                    try
                    {
                        HistoryWorkingTime h = JsonConvert.DeserializeObject<HistoryWorkingTime>(bgrd.JobArgs);
                        hwtIds.AddLast(h.Id);
                    }
                    catch (Exception e)
                    {

                    }

                }

                var approvedHWTs = await WorkScope.GetAll<HistoryWorkingTime>()
                    .Where(s => (s.UserId == item.UserId && s.Id != id && s.Status == RequestStatus.Approved && s.ApplyDate.Date > today) || hwtIds.Contains(s.Id))
                    .ToListAsync();
                foreach (var hwt in approvedHWTs)
                {
                    hwt.Status = RequestStatus.Rejected;
                    await WorkScope.UpdateAsync(hwt);
                }

                // add background job
                await _backgroundJobManager.EnqueueAsync<WorkingTimeBackgroundJob, WorkingTimeBackgroundJobArgs>(new WorkingTimeBackgroundJobArgs
                {
                    Target = item
                }, BackgroundJobPriority.High, TimeSpan.FromHours((item.ApplyDate.Date.AddHours(20) - DateTimeUtils.GetNow()).TotalHours));
            }

            await notifyKomuWhenApproveChangeMyWorkingTime(item, true);
        }

        private async System.Threading.Tasks.Task ChangeWorkingTime(HistoryWorkingTime workingTime)
        {
            var user = await WorkScope.GetAsync<User>(workingTime.UserId);
            user.MorningStartAt = workingTime.MorningStartTime;
            user.MorningEndAt = workingTime.MorningEndTime;
            user.MorningWorking = workingTime.MorningWorkingTime;
            user.AfternoonStartAt = workingTime.AfternoonStartTime;
            user.AfternoonEndAt = workingTime.AfternoonEndTime;
            user.AfternoonWorking = workingTime.AfternoonWorkingTime;
            user.isWorkingTimeDefault = false;

            await WorkScope.UpdateAsync<User>(user);

            ChangeRequestOffOfUserAfterApplydate(user.Id, workingTime.MorningWorkingTime, workingTime.AfternoonWorkingTime);
        }

        private void ChangeRequestOffOfUserAfterApplydate(long userId, double morningWokingTime, double afternoonWokingTime)
        {
            var absenceDayDetails = WorkScope.GetAll<AbsenceDayDetail>()
                                                    .Where(s => s.Request.UserId == userId)
                                                    .Where(s => s.DateAt.Date >= DateTimeUtils.GetNow().Date)
                                                    .Where(s => s.Request.Type == RequestType.Off)
                                                    .Where(s => s.DateType == DayType.Morning || s.DateType == DayType.Afternoon)
                                                    .ToList();

            foreach (var item in absenceDayDetails)
            {
                if (item.DateType == DayType.Morning)
                    item.Hour = morningWokingTime;
                else
                    item.Hour = afternoonWokingTime;
            }

            CurrentUnitOfWork.SaveChanges();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ManageWorkingTime_Approval)]
        [HttpPost]
        public async System.Threading.Tasks.Task RejectWorkingTime(long id)
        {
            var itemExt = await WorkScope.GetAll<HistoryWorkingTime>()
                .Where(s => s.Id == id)
                .Select(s => new { Entity = s, s.User.FullName })
                .FirstOrDefaultAsync();

            var item = itemExt.Entity;

            if (item.Status == RequestStatus.Approved && item.ApplyDate.Date <= DateTime.Now.Date)
            {
                throw new UserFriendlyException("This working time is approved");
            }

            var projectIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM && s.UserId == AbpSession.UserId)
                .Select(s => s.ProjectId).ToListAsync();

            var isValid = await WorkScope.GetAll<ProjectUser>()
                .AnyAsync(s => s.UserId == item.UserId && projectIds.Contains(s.ProjectId));

            if (!isValid)
            {
                throw new UserFriendlyException($"You are not PM of user {itemExt.FullName}");
            }

            item.Status = RequestStatus.Rejected;
            await WorkScope.UpdateAsync<HistoryWorkingTime>(item);
            // delete background job
            var getDeleted = await (from b in WorkScope.GetAll<BackgroundJobInfo>()
                                    where b.JobArgs.Contains($"\"Id\":{id}") && b.JobType.Contains("Timesheet.BackgroundJob.WorkingTimeBackgroundJob")
                                    select new { Id = b.Id }).FirstOrDefaultAsync();
            if (getDeleted != null)
            {
                await _backgroundJobManager.DeleteAsync($"{getDeleted.Id}");
            }

            await notifyKomuWhenApproveChangeMyWorkingTime(item, false);
        }

        private async System.Threading.Tasks.Task notifyKomuWhenApproveChangeMyWorkingTime(HistoryWorkingTime request, bool isApprove)
        {
            var enableNotify = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest);
            if (enableNotify != "true")
            {
                Logger.Info("notifyKomuWhenApproveOrRejectRequest() SendKomuRequest=" + enableNotify + ", AbpSessionUserId=" + AbpSession.UserId);
                return;
            }

            var approver = await getNotifyUserInfoDto(AbpSession.UserId.Value);
            var receivers = await getReceiverListApproveChangeWorkingTime(request.UserId);
            var requester = await getNotifyUserInfoDto(request.UserId);

            var historyWorkingTime = await WorkScope.GetAll<HistoryWorkingTime>()
                .Where(s => s.Id == request.Id)
                .Select(s => new HistoryWorkingTimeDto
                {
                    Id = s.Id,
                    MorningStartTime = s.MorningStartTime,
                    MorningEndTime = s.MorningEndTime,
                    AfternoonStartTime = s.AfternoonStartTime,
                    AfternoonEndTime = s.AfternoonEndTime,
                    ApplyDate = s.ApplyDate
                }).FirstOrDefaultAsync();

            var alreadySentToPMIds = new HashSet<long>();
            foreach (var project in receivers)
            {
                if (!project.IsNotifyKomu)
                {
                    Logger.Info($"notifyKomuWhenApproveRequest() projectId={project.ProjectId}: IsNotifyKomu={project.IsNotifyKomu}, KomuChannelId={project.KomuChannelId}");
                }
                else
                {
                    var pmsTag = project.KomuPMsTag(alreadySentToPMIds);
                    pmsTag = string.IsNullOrEmpty(pmsTag) ? "" : $"PM {pmsTag}:";

                    var komuMessage = $"{pmsTag} **{approver.FullName}** " +
                        $"has **{(isApprove ? "approved" : "rejected")}** the request to change working time: " +
                        $"\n```RequestId: #{historyWorkingTime.Id}" +
                        $"\nName: {requester.FullName}" +
                        $"\nMorning: {historyWorkingTime.MorningStartTime} - {historyWorkingTime.MorningEndTime}" +
                        $"\nAfternoon: {historyWorkingTime.AfternoonStartTime} - {historyWorkingTime.AfternoonEndTime}" +
                        $"\nApply date: {historyWorkingTime.ApplyDate.ToString("dd/MM/yyyy")}```";

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

        private async Task<NotifyUserInfoDto> getNotifyUserInfoDto(long userId)
        {
            return await WorkScope.GetAll<User>().Where(s => s.Id == userId)
                .Select(user => new NotifyUserInfoDto
                {
                    Branch = user.BranchOld,
                    EmailAddress = user.EmailAddress,
                    FullName = user.FullName,
                    KomuUserId = user.KomuUserId,
                    Type = user.Type,
                    UserId = user.Id,
                    BranchDisplayName = user.Branch.DisplayName,
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ProjectPMDto>> getReceiverListApproveChangeWorkingTime(long requesterId)
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
                s.Project.IsNoticeKMApproveChangeWorkingTime,
            });

            var result = await (from projectId in qrequesterInProjectIds
                                join pm in queryPMs on projectId equals pm.ProjectId
                                select pm)
                              .GroupBy(s => new { s.ProjectId, s.KomuChannelId, s.IsNoticeKMApproveChangeWorkingTime })
                              .Select(s => new ProjectPMDto
                              {
                                  ProjectId = s.Key.ProjectId,
                                  KomuChannelId = s.Key.KomuChannelId,
                                  IsNotifyKomu = s.Key.IsNoticeKMApproveChangeWorkingTime,
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
