using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using ClosedXML.Excel;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Timesheet.APIs.CapabilitySettings.Dto;
using Timesheet.APIs.ReviewDetails;
using Timesheet.APIs.ReviewDetails.Dto;
using Timesheet.APIs.ReviewInterns.Dto;
using Timesheet.APIs.Timesheets.LevelSettings;
using Timesheet.BackgroundJob;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Services.HRM;
using Timesheet.Services.HRM.Dto;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using static Timesheet.Services.HRM.Dto.UpdateToHrmDto;
using static Timesheet.Services.HRM.Dto.UpdateToHrmv2Dto;

namespace Timesheet.APIs.ReviewInterns
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern)]
    public class ReviewInternAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly HRMv2Service _hrmv2Service;
        private readonly HRMService _hrmService;
        private readonly ProjectService _projectService;
        private readonly ReviewDetailAppService _reviewDetailAppService;


        public ReviewInternAppService(IBackgroundJobManager backgroundJobManager, HRMv2Service hrmv2Service, IWorkScope workScope,
            ProjectService projectService, HRMService hRMService, ReviewDetailAppService reviewDetailAppService) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _hrmService = hRMService;
            _hrmv2Service = hrmv2Service;
            _projectService = projectService;
            _reviewDetailAppService = reviewDetailAppService;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_AddNewReview)]
        [HttpPost]
        public async Task<ReviewInternDto> Create(ReviewInternDto input)
        {
            var IsAlreadyExist = await WorkScope.GetAll<ReviewIntern>()
                .AnyAsync(x => x.Month == input.Month && x.Year == input.Year);

            if (IsAlreadyExist)
            {
                throw new UserFriendlyException("Review Intern already exist row at Year = " + input.Year + "  month = " + input.Month);
            }

            var reviewIntern = ObjectMapper.Map<ReviewIntern>(input);

            input.Id = await WorkScope.InsertAndGetIdAsync(reviewIntern);

            var qmtsOfIntern = WorkScope.GetAll<MyTimesheet>()
              .Where(s => s.DateAt.Year == input.Year && s.DateAt.Month == input.Month)
              .Where(s => s.Status >= TimesheetStatus.Pending)
              .Where(s => s.User.Type == Usertype.Internship && s.User.IsActive && !s.User.IsStopWork)
              .Where(s => !s.ProjectTask.Project.isAllUserBelongTo);


            var details = await qmtsOfIntern
                .Where(x => x.DateAt.Day <= 20)
                .GroupBy(x => x.UserId)
                .Select(x => new
                {
                    InternshipId = x.Key,
                    ReviewerId = AbpSession.UserId.Value,
                    CurrentLevel = x.Select(mts => mts.User.Level).FirstOrDefault(),
                }).ToListAsync();


            var qmts = qmtsOfIntern
                .Select(s => new
                {
                    s.UserId,
                    s.Id,
                    s.WorkingTime,
                    s.ProjectTask.ProjectId
                });


            var qprojectPms = from s in (WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new { s.ProjectId, PMId = s.UserId }))
                              group s by s.ProjectId into g
                              select new { ProjectId = g.Key, PmId = g.FirstOrDefault().PMId };

            var mapUserPmId = await (from s in (from mts in qmts
                                                join pu in qprojectPms on mts.ProjectId equals pu.ProjectId
                                                select new { mts.UserId, mts.WorkingTime, pu.PmId })
                                     group s by s.UserId into g
                                     select new
                                     {
                                         UserId = g.Key,
                                         PM = g.GroupBy(s => s.PmId, (key, lst) => new { PmId = key, WorkingTime = lst.Sum(s => s.WorkingTime) })
                                         .OrderByDescending(s => s.WorkingTime)
                                         .FirstOrDefault()
                                     }).ToDictionaryAsync(s => s.UserId, s => s.PM.PmId);

            foreach (var x in details)
            {
                var reviewDetails = new ReviewDetail
                {
                    InternshipId = x.InternshipId,
                    ReviewerId = mapUserPmId.ContainsKey(x.InternshipId) ? (long?)mapUserPmId[x.InternshipId] : null,
                    CurrentLevel = x.CurrentLevel,
                    NewLevel = x.CurrentLevel,
                    Status = ReviewInternStatus.Draft,
                    ReviewId = input.Id,
                    Type = Usertype.Internship
                };
                await WorkScope.InsertAsync(reviewDetails);
            }

            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ViewAll)]
        public async Task<List<ReviewInternDto>> GetAll(int? year)
        {
            var Result = WorkScope.GetAll<ReviewIntern>()
                .Where(x => !year.HasValue || x.Year == year)
                .Select(x => new ReviewInternDto
                {
                    Month = x.Month,
                    Year = x.Year,
                    Id = x.Id,
                    IsActive = x.IsActive
                }).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

            return await Result.ToListAsync();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            //var isExist = await WorkScope.GetAll<ReviewIntern>().AnyAsync(x=>x.Id == input.Id);
            //if (!isExist)
            //{
            //    throw new UserFriendlyException("Review has Id = " + input.Id + " not exist");
            //}

            bool hasReviewedDetail = await WorkScope.GetAll<ReviewDetail>().AnyAsync(x => x.ReviewId == input.Id && x.Status != ReviewInternStatus.Draft);

            if (hasReviewedDetail)
            {
                throw new UserFriendlyException("Bạn không thể xóa đợt review tts này vì đã có tts được review");
            }

            var detailIds = await WorkScope.GetAll<ReviewDetail>().Where(x => x.ReviewId == input.Id).Select(s => s.Id).ToListAsync();

            foreach (var Id in detailIds)
            {
                await WorkScope.DeleteAsync<ReviewDetail>(Id);
            }

            await WorkScope.DeleteAsync<ReviewIntern>(input.Id);
        }
        //TODO: test case SendEmailsIntern
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsIntern, PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsOffical)]
        public async System.Threading.Tasks.Task<object> SendEmailsIntern(long reviewId, bool isCheckToOffical)
        {
            Dictionary<UserLevel, string> levelDetail = CommonUtils.UserLevelDetail();
            Dictionary<Usertype, string> typeDetail = new Dictionary<Usertype, string>()
            {
                { Usertype.Staff, "- Thử việc <br>"},
                { Usertype.Collaborators, "- Cộng tác viên<br>" }
            };
            var levelSettings = await GetLevelSetting();
            var isExist = await WorkScope.GetAll<ReviewIntern>().AnyAsync(x => x.Id == reviewId);
            if (!isExist)
            {
                throw new UserFriendlyException("Review has Id = " + reviewId + " not exist");
            }

            var review = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            var details = await (from rv in WorkScope.GetAll<ReviewDetail>()
                                    .Where(x => x.ReviewId == reviewId && (x.Status == ReviewInternStatus.Approved || x.Status == ReviewInternStatus.Rejected))
                                    .Where(x => (isCheckToOffical && x.NewLevel > UserLevel.Intern_3) || (!isCheckToOffical && x.NewLevel <= UserLevel.Intern_3))
                                 select new
                                 {
                                     ReviewDetail = rv,
                                     InterShip = rv.InterShip,
                                     InternEmail = rv.InterShip.EmailAddress,
                                     InternBranch = rv.InterShip.BranchOld,
                                     ReviewerEmail = rv.Reviewer.EmailAddress,
                                     ReviewerName = rv.Reviewer.FullName,
                                     Id = rv.Id,
                                     Note = rv.Note,
                                     CurrentLevel = rv.CurrentLevel,
                                     NewLevel = rv.NewLevel,
                                     SubLevel = rv.SubLevel,
                                     IsFullSalary = rv.IsFullSalary,
                                     InternshipName = rv.InterShip.FullName,
                                     Type = rv.Type,
                                     Month = rv.Review.Month,
                                     Year = rv.Review.Year,
                                     CurrentLevelInfo = rv.CurrentLevel.HasValue ? levelDetail[rv.CurrentLevel.Value] : string.Empty,
                                     NewLevelInfo = rv.NewLevel.HasValue ? levelDetail[rv.NewLevel.Value] : string.Empty,
                                     Position = rv.InterShip.Type
                                 }).ToListAsync();

            if (details.Count() == 0)
            {
                throw new UserFriendlyException("Chưa có TTS mới nào được approve hoặc reject");
            }

            List<Object> failedList = new List<object>();
            List<Object> successList = new List<object>();
            var updateUsers = new List<UpdateStarRateToProjectDto>();

            var userErrorInfo = details
                .Where(s => s.InterShip.Level != s.CurrentLevel || s.InterShip.Level > UserLevel.Intern_3)
                .Select(s => s.Id)
                .ToList();

            var emailHR = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHR);
            emailHR = Convert.ToString(emailHR);

            //var emailHRDN = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRDN);
            //emailHRDN = Convert.ToString(emailHRDN);

            //var emailHRHCM = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRHCM);
            //emailHRHCM = Convert.ToString(emailHRHCM);

            foreach (var detail in details)
            {
                try
                {
                    if (userErrorInfo.Contains(detail.Id))
                    {
                        throw new Exception(CommonUtils.GetErrorMessageReviewIntern(detail.CurrentLevel, detail.InterShip.Level));
                    }
                    StringBuilder content = new StringBuilder();
                    string currentMonth = detail.Month < 10 ? "0" + detail.Month.ToString() : detail.Month.ToString();
                    var applyDate = new DateTime(detail.Year, detail.Month, 1).AddMonths(1).ToString("dd/MM/yyyy");
                    content.Append($"Thân gửi <span style='font-weight: 600'>{detail.InternshipName}</span>, <br> Dưới đây là phần đánh giá thực tập trong tháng <span style='font-weight: 600'> {currentMonth}/{detail.Year} </span> của bạn.");

                    var mailBody = $@"<table border-collapse='collapse' border='1' width='100%' style='margin-top: 15px'>
                                <thead>
                                    <tr>
                                        <th width='15%'>Reviewer Name</th>
                                        <th width='15%'>Intern Name</th>
                                        <th width='20%'>Old Level</th>
                                        <th width='20%'>New Level</th>
                                        <th width='10%'>Apply Date</th>
                                        <th width='20%'>Note</th>
                                    </tr>
                                </thead>
                            <tbody>
                                <tr>
                                <td style='padding-left: 5px'>{detail.ReviewerName}</td>
                                <td style='padding-left: 5px'>{detail.InternshipName}</td>
                                <td style='padding-left: 5px'>{detail.CurrentLevel} <br/> {detail.CurrentLevelInfo}</td>
                                <td style='padding-left: 5px'>{detail.NewLevel}<br/> {((detail.NewLevel < UserLevel.FresherMinus) ? "" : typeDetail[detail.Type])}{detail.NewLevelInfo}</td>
                                <td style='padding-left: 5px'>{applyDate}</td>
                                <td style='padding-left: 5px; white-space: pre;'>{detail.Note}</td>
                                </tr>
                            </tbody>
                            </table>
                            <hr>";
                    content.Append(mailBody);
                    content.Append("Mọi thông tin thắc mắc về nội dung đánh giá vui lòng liên hệ trực tiếp với PM để được giải đáp.");

                    var emailSubject = $"[NCC] Thông báo kết quả review mức hỗ trợ đối với TTS {detail.InternshipName} {detail.Month}/{detail.Year}";

                    var targetEmails = new List<string>() { detail.InternEmail, emailHR, detail.ReviewerEmail };

                    //if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.DaNang && !String.IsNullOrEmpty(emailHRDN))
                    //{
                    //    targetEmails.Add(emailHRDN);
                    //}
                    //else if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.HoChiMinh && !String.IsNullOrEmpty(emailHRHCM))
                    //{
                    //    targetEmails.Add(emailHRHCM);
                    //}

                    string result = await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = targetEmails,
                        Body = content.ToString(),
                        Subject = emailSubject
                    }, BackgroundJobPriority.High, new TimeSpan(TimeSpan.TicksPerMinute));

                    detail.ReviewDetail.Status = ReviewInternStatus.SentEmail;
                    await WorkScope.UpdateAsync(detail.ReviewDetail);
                    if (detail.NewLevel != detail.InterShip.Level)
                    {
                        //detail.InterShip.Type = detail.Type;
                        if (detail.NewLevel >= UserLevel.FresherMinus)
                        {
                            detail.InterShip.Type = detail.Type;
                        }
                        detail.InterShip.Level = detail.NewLevel;
                        await WorkScope.UpdateAsync<User>(detail.InterShip);
                    }

                    successList.Add(new { detail.Id, detail.InternEmail, detail.ReviewerName });
                    updateUsers.Add(new UpdateStarRateToProjectDto
                    {
                        UserCode = detail.InterShip.Id.ToString(),
                        EmailAddress = detail.InterShip.EmailAddress,
                        StarRate = detail.ReviewDetail.RateStar ?? 0,
                        Level = detail.InterShip.Level,
                        Type = detail.InterShip.Type
                    });
                }
                catch (Exception e)
                {
                    failedList.Add(new { detail.Id, detail.InternEmail, detail.ReviewerName, e.Message });
                    Logger.Error("SendEmails() error=>" + e.Message);
                }
            }
            var updateSuccess = await _projectService.UpdateStarRateToProject(updateUsers);
            return new
            {
                //Details = details.ToList(),
                SuccessList = successList,
                FailedList = failedList
            };

        }


        public async Task<List<LevelSettingDto>> GetLevelSetting()
        {
            var levelSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.UserLevelSetting);
            var rs = JsonConvert.DeserializeObject<List<LevelSettingDto>>(levelSetting);
            return rs;
        }

        //TODO: chưa test được do chưa gọi được api bên hrm
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_UpdateAllToHRMs)]
        [HttpGet]
        public async Task<string> UpdateLevelHRMs(long reviewId)
        {
            var isExist = await WorkScope.GetAll<ReviewIntern>().AnyAsync(x => x.Id == reviewId);
            if (!isExist)
            {
                throw new UserFriendlyException("Review has Id = " + reviewId + " not exist");
            }

            var review = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            var creatorUser = WorkScope.GetAll<User>()
                  .Where(x => x.Id == AbpSession.UserId)
                  .Select(x => new {
                      x.NormalizedEmailAddress,
                      x.FullName
                  })
                  .FirstOrDefault();
            var listUserToUpdate = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.ReviewId == reviewId && x.Status == ReviewInternStatus.SentEmail)
                .Select(s => new UserToUpdateDto
                {
                    UserId = s.InternshipId,
                    EmailAddress = s.InterShip.EmailAddress,
                    NormalizedEmailAddress = s.InterShip.NormalizedEmailAddress,
                    NewLevel = s.NewLevel,
                    Type = s.InterShip.Type,
                    IsFullSalary = s.IsFullSalary,
                    Salary = s.Salary,
                    SubLevel = s.SubLevel,
                    UpdateToHRMByUserId = AbpSession.UserId.Value,

                }).ToListAsync();

            if (listUserToUpdate == null || listUserToUpdate.Count == 0)
            {
                throw new UserFriendlyException("There is no internship change level");
            }
            var updateToHrmDto = new CreateRequestFromTSDto
            {
                CreatorNormalizedEmail = creatorUser.NormalizedEmailAddress,
                RequestName = $"Request tăng lương từ timesheet đợt {review.Month}/{review.Year} bởi {creatorUser.FullName}",
                ListUserToUpdate = listUserToUpdate,
                ExcutedDate = Convert.ToDateTime(review.Year.ToString() + "/" + review.Month.ToString() + "/01").AddMonths(1)
            };
            var rs = await _hrmService.UpdateLevel(updateToHrmDto);
            return rs;
        }

        //TODO: chưa test được do chưa gọi được api bên hrmv2
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_UpdateAllToHRMs)]
        [HttpGet]
        public void UpdateLevelHRMv2ForAll(long reviewId)
        {
            var review = WorkScope.GetAll<ReviewIntern>()
                .Where(x => x.Id == reviewId)
                .FirstOrDefault();

            if (review == default)
            {
                throw new UserFriendlyException("Review has Id = " + reviewId + " not exist");
            }

            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            var listUserToUpdate = WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.ReviewId == reviewId && x.Status == ReviewInternStatus.SentEmail)
                .Where(s => s.NewLevel != s.CurrentLevel)
                .Select(s => new UserToUpdateHrmv2Dto
                {
                    NormalizedEmailAddress = s.InterShip.NormalizedEmailAddress,
                    NewLevel = s.NewLevel ?? UserLevel.Intern_0,
                    UserType = s.Type, // sửa lại vì gửi sai userType cũ sang HRM-V2
                    IsFullSalary = s.IsFullSalary ?? false,
                    Salary = s.Salary ?? 0,
                }).ToList();

            if (listUserToUpdate == null || listUserToUpdate.Count == 0)
            {
                throw new UserFriendlyException("There are no internship change level");
            }

            UpdateLevelToHrmv2(listUserToUpdate, review.Year, review.Month);
        }

        //TODO: chưa test được do chưa gọi được api bên hrmv2
        [HttpGet]
        public void UpdateLevelHRMv2ForOne(long reviewDetailId)
        {
            var reviewDetail = WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.Id == reviewDetailId)
                .Select(s => new
                {
                    Email = s.InterShip.NormalizedEmailAddress,
                    NewLevel = s.NewLevel,
                    UserType = s.Type, // sửa lại vì gửi sai userType cũ sang HRM-V2
                    IsFullSalary = s.IsFullSalary,
                    Salary = s.Salary,
                    ReviewYear = s.Review.Year,
                    ReviewMonth = s.Review.Month,
                })
                .FirstOrDefault();

            if (reviewDetail == default)
            {
                throw new UserFriendlyException($"Can't find review detail with id {reviewDetailId}");
            }

            var dto = new UserToUpdateHrmv2Dto
            {
                NormalizedEmailAddress = reviewDetail.Email,
                NewLevel = reviewDetail.NewLevel ?? UserLevel.Intern_0,
                UserType = reviewDetail.UserType, // sửa lại vì gửi sai userType cũ sang HRM-V2
                IsFullSalary = reviewDetail.IsFullSalary ?? false,
                Salary = reviewDetail.Salary ?? 0,
            };
            var userToUpdate = new List<UserToUpdateHrmv2Dto> { dto };

            UpdateLevelToHrmv2(userToUpdate, reviewDetail.ReviewYear, reviewDetail.ReviewMonth);
        }

        //TODO: chưa test được do chưa gọi được api bên hrmv2
        public void UpdateLevelToHrmv2(List<UserToUpdateHrmv2Dto> listUserToUpdate, int year, int month)
        {
            var creatorUser = WorkScope.GetAll<User>()
               .Where(x => x.Id == AbpSession.UserId)
               .Select(x => x.FullName)
               .FirstOrDefault();

            var updateToHrmDto = new InputCreateRequestHrmv2Dto
            {
                RequestName = $"Review intern từ timesheet đợt {month}/{year}",
                ListUserToUpdate = listUserToUpdate,
                CreatedBy = creatorUser,
                Applydate = new DateTime(year, month, 1).AddMonths(1)
            };

            _hrmv2Service.CreateRequestHrmv2(updateToHrmDto);
        }

        //TODO: chưa test được do chưa gọi được api bên project
        [AbpAuthorize(PermissionNames.ReviewIntern_ReviewDetail_UpdateStarToProject)]
        [HttpGet]
        public async Task<List<UpdateStarRateToProjectDto>> UpdateStarProject(long reviewId)
        {
            var isExist = await WorkScope.GetAll<ReviewIntern>().AnyAsync(x => x.Id == reviewId && x.IsActive);
            if (!isExist)
            {
                throw new UserFriendlyException("Review has Id = " + reviewId + " not exist");
            }

            var updateUsers = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.ReviewId == reviewId && x.Status == ReviewInternStatus.SentEmail)
                .Select(s => new UpdateStarRateToProjectDto
                {
                    UserCode = s.InterShip.Id.ToString(),
                    EmailAddress = s.InterShip.EmailAddress,
                    StarRate = s.RateStar ?? 0,
                    Level = s.InterShip.Level,
                    Type = s.InterShip.Type

                }).ToListAsync();

            if (updateUsers == null || updateUsers.Count == 0)
            {
                throw new UserFriendlyException("No one has been rated yet!");
            }

            var updateSuccess = await _projectService.UpdateStarRateToProject(updateUsers);
            if (updateSuccess == null)
            {
                throw new UserFriendlyException("Unable to update Star Rating for Project");
            }

            return updateUsers;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_Active)]
        public async System.Threading.Tasks.Task Active(long Id)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(Id);
            review.IsActive = true;
            await WorkScope.UpdateAsync(review);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_DeActive)]
        public async System.Threading.Tasks.Task DeActive(long Id)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(Id);
            review.IsActive = false;
            await WorkScope.UpdateAsync(review);
        }


        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ApproveAll)]
        public async System.Threading.Tasks.Task ApproveAll(long reviewId, GridParam input, long? branchId)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            input.MaxResultCount = int.MaxValue;
            var t =  await _reviewDetailAppService.GetAllDetails(input, reviewId, branchId);
            var reviewDetailReviewedIds = t.Items.Where(s => s.Status == ReviewInternStatus.Reviewed).Select(s => s.Id).ToList();
            var details = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => reviewDetailReviewedIds.Contains(x.Id))
                .ToListAsync();

            foreach (var detail in details)
            {
                detail.Status = ReviewInternStatus.Approved;
                await WorkScope.UpdateAsync(detail);
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ViewAllReport)]
        public async Task<ReportInternOutput> GetReport(int? year, int? month, string keyword, string level, bool isCurrentInternOnly)
        {
            if (!year.HasValue)
            {
                year = DateTimeUtils.GetNow().Year;
            }
            if (!month.HasValue)
            {
                month = DateTimeUtils.GetNow().Month;
            }

            var output = new ReportInternOutput { };
            DateTime toDate = new DateTime(year.Value, month.Value, 1);
            DateTime fromDate = toDate.AddMonths(-11);

            output.listMonth = new List<string>();

            var date = fromDate;
            while (date <= toDate)
            {
                output.listMonth.Add(date.ToString("MM-yyyy"));
                date = date.AddMonths(1);
            }

            var queryReviewLevel = WorkScope.GetAll<ReviewDetail>()
                            .Where(s => s.Review.Year == year)
                            .Where(s => s.Review.Month == month)
                            .Where(s => s.Status >= ReviewInternStatus.Reviewed);

            if (level == "level-up")
            {
                queryReviewLevel = queryReviewLevel.Where(s => s.NewLevel > s.CurrentLevel);
            }
            else if (level == "level-not-change")
            {
                queryReviewLevel = queryReviewLevel.Where(s => s.NewLevel <= s.CurrentLevel);
            }
            var internIds = await queryReviewLevel.Select(s => s.InternshipId).ToListAsync();

            var reviewInterns = await WorkScope.GetAll<ReviewDetail>()
           .Where(s => new DateTime(s.Review.Year, s.Review.Month, 2) >= fromDate.Date)
           .Where(s => new DateTime(s.Review.Year, s.Review.Month, 1).Date <= toDate)
           .Where(s => s.Status >= ReviewInternStatus.Reviewed)
           .Where(s => String.IsNullOrEmpty(keyword) || s.InterShip.EmailAddress.Contains(keyword) || s.InterShip.FullName.Contains(keyword))
           .Where(s => string.IsNullOrEmpty(level) || internIds.Contains(s.InternshipId))
           .Where(s => s.InterShip.IsActive)
           .Where(s => !isCurrentInternOnly || s.InterShip.Type == Usertype.Internship)
           .GroupBy(s => new
           {
               s.InterShip.EmailAddress,
               FullName = s.InterShip.Name + " " + s.InterShip.Surname,
               s.InterShip.BranchOld,
               AvatarPath = s.InterShip.AvatarPath != null ? s.InterShip.AvatarPath : "",
               s.InterShip.Type,
               s.InterShip.Level,
               s.InterShip.Branch.Color,
               s.InterShip.Branch.DisplayName,
           })
           .Select(s => new ReportInternForMonth
           {
               EmailAddress = s.Key.EmailAddress,
               InternName = s.Key.FullName,
               Branch = s.Key.BranchOld,
               AvatarPath = s.Key.AvatarPath,
               Type = s.Key.Type,
               Level = s.Key.Level,
               BranchColor = s.Key.Color,
               BranchDisplayName = s.Key.DisplayName,
               ReviewDetailForMonths = s.Select(x => new ReviewResult
               {
                   NewLevel = x.NewLevel,
                   CurrentLevel = x.CurrentLevel,
                   ReviewerName = x.Reviewer.Name + " " + x.Reviewer.Surname,
                   WarningType = InternWarningType.Normal,
                   Year = x.Review.Year,
                   Month = x.Review.Month,
                   HasReview = true,
               }).OrderBy(x => x.Year)
               .ThenBy(x => x.Month)
               .ToList()
           }).ToListAsync();

            foreach (var intern in reviewInterns)
            {
                if (intern.ReviewDetailForMonths == null)
                {
                    intern.ReviewDetailForMonths = new List<ReviewResult>();
                }

                date = fromDate;
                while (date <= toDate)
                {
                    if (!intern.ReviewDetailForMonths.Any(s => s.Year == date.Year && s.Month == date.Month))
                    {
                        intern.ReviewDetailForMonths.Add(new ReviewResult
                        {
                            Year = date.Year,
                            Month = date.Month,
                            HasReview = false
                        });
                    }
                    date = date.AddMonths(1);

                }
                intern.ReviewDetailForMonths = intern.ReviewDetailForMonths.OrderBy(s => s.Year).ThenBy(s => s.Month).ToList();
                for (int i = 0; i < intern.ReviewDetailForMonths.Count; i++)
                {
                    intern.ReviewDetailForMonths[i].IndexColumnExcel = i;
                }
                processWarningStatusInterns(intern.ReviewDetailForMonths);

            }

            output.listInternLevel = reviewInterns;

            return output;
        }

        private class LastInternLevel
        {
            public UserLevel Level { get; set; }
            public int Index { get; set; }
        }

        private LastInternLevel findLastInternLevel(List<ReviewResult> reviewDetailForMonths, UserLevel level)
        {
            for (int i = reviewDetailForMonths.Count - 1; i >= 0; i--)
            {
                var item = reviewDetailForMonths[i];
                if (item.HasReview && item.NewLevel.HasValue && item.NewLevel.Value == level)
                {
                    return new LastInternLevel { Index = i, Level = item.NewLevel.Value };
                }
            }

            return null;
        }

        private List<ReviewResult> getListSameLevel(List<ReviewResult> reviewDetailForMonths, LastInternLevel lastInternLevel)
        {
            var result = new List<ReviewResult>();
            for (int i = lastInternLevel.Index; i >= 0; i--)
            {
                var item = reviewDetailForMonths[i];
                if (item.HasReview && item.NewLevel.HasValue && item.NewLevel.Value == lastInternLevel.Level)
                {
                    result.Add(item);
                }

            }

            return result;
        }

        private List<ReviewResult> getListLessThanOrEqualLevel(List<ReviewResult> reviewDetailForMonths, LastInternLevel lastInternLevel)
        {
            var result = new List<ReviewResult>();
            for (int i = lastInternLevel.Index; i >= 0; i--)
            {
                var item = reviewDetailForMonths[i];
                if (item.HasReview && item.NewLevel.HasValue && item.NewLevel.Value <= lastInternLevel.Level)
                {
                    result.Add(item);
                }

            }

            return result;
        }

        private List<ReviewResult> getListStaff(List<ReviewResult> reviewDetailForMonths)
        {
            var result = new List<ReviewResult>();
            for (int i = 0; i < reviewDetailForMonths.Count; i++)
            {
                var item = reviewDetailForMonths[i];
                if (item.HasReview && item.NewLevel.HasValue && item.NewLevel.Value > UserLevel.Intern_3)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private void updateWarningStatus(List<ReviewResult> listSameLevel, InternWarningType warningType)
        {
            foreach (var item in listSameLevel)
            {
                item.WarningType = warningType;
            }
        }

        // Orange Warning:
        //    <=1 tháng dưới 2M
        //    2 tháng liên tiếp (n, n+1) 2M mà tháng n-1 CÓ review
        //    3 tháng liên tiếp (n, n+1, n+2) 2M mà tháng n-1 KHÔNG có review (mới vào đã được 2M rồi)
        //    2 tháng liên tiếp 4M
        //Red Warning:
        //    >=2 tháng liên tiếp < 2M
        //    >= 3 tháng liên tiếp (n, n+1, n+2) 2M mà tháng n-1 CÓ review
        //    >= 4 tháng liên tiếp (n, n+1, n+2, n+3) 2M mà tháng n-1 KHÔNG có review
        //    >= 3 tháng liên tiếp 4M            
        private void processWarningStatusInterns(List<ReviewResult> reviewDetailForMonths)
        {
            //Staff
            var listStaff = getListStaff(reviewDetailForMonths);
            if (listStaff.Count > 0)
            {
                updateWarningStatus(listStaff, InternWarningType.Staff);
            }
            //4M
            var lastInternLevel = findLastInternLevel(reviewDetailForMonths, UserLevel.Intern_3);
            List<ReviewResult> listSameLevel = null;
            if (lastInternLevel != null)
            {
                listSameLevel = getListSameLevel(reviewDetailForMonths, lastInternLevel);
                if (listSameLevel.Count >= 3)
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Red);
                }
                else if (listSameLevel.Count == 2)
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Orange);
                }
            }

            //2M
            lastInternLevel = findLastInternLevel(reviewDetailForMonths, UserLevel.Intern_2);
            if (lastInternLevel != null)
            {
                listSameLevel = getListSameLevel(reviewDetailForMonths, lastInternLevel);
                //Red
                int index = lastInternLevel.Index - 4;

                if (listSameLevel.Count >= 4 && !hasReview(reviewDetailForMonths, index))
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Red);
                }

                index = lastInternLevel.Index - 3;
                if (listSameLevel.Count >= 3 && hasReview(reviewDetailForMonths, index))
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Red);
                }

                //Orange
                if (listSameLevel.Count == 3 && !hasReview(reviewDetailForMonths, index))
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Orange);
                }

                index = lastInternLevel.Index - 2;
                if (listSameLevel.Count == 2 && hasReview(reviewDetailForMonths, index))
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Orange);
                }
            }


            //Intern 0,1

            var lastLevel = findLastInternLevel(reviewDetailForMonths, UserLevel.Intern_1);

            if (lastLevel == null)
            {
                lastLevel = findLastInternLevel(reviewDetailForMonths, UserLevel.Intern_0);
            }

            if (lastLevel != null)
            {
                listSameLevel = getListLessThanOrEqualLevel(reviewDetailForMonths, lastLevel);
                if (listSameLevel.Count >= 2)
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Red);
                }
                else if (listSameLevel.Count == 1)
                {
                    updateWarningStatus(listSameLevel, InternWarningType.Orange);
                }
            }
        }

        private bool hasReview(List<ReviewResult> reviewDetailForMonths, int index)
        {
            return index >= 0 && reviewDetailForMonths[index].HasReview;
        }
        //TODO: Test case ExportReportIntern
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ExportReport)]
        public async Task<Byte[]> ExportReportIntern(int year, int month, string keyword, string level, bool isCurrentInternOnly)
        {
            var reportInternForMonths = await GetReport(year, month, keyword, level, isCurrentInternOnly);
            try
            {
                int currentRow = 2;
                using (var workBook = new XLWorkbook())
                {
                    workBook.Author = "NCCSoftHR";
                    //Title
                    var ws = workBook.Worksheets.Add("Report Intern");
                    ws.Cell("A1").Value = $"REPORT REVIEW INTERN {month} - {year}";
                    var title = ws.Range("A1:N1").Merge();
                    title.Style.Font.SetBold().Font.FontSize = 16;
                    title.Style.Fill.BackgroundColor = XLColor.Yellow;
                    title.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    title.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    //Header
                    ws.Column(1).Width = 5;
                    ws.Column(2).Width = 30;
                    ws.Cell(currentRow, 1).Value = "STT";
                    ws.Cell(currentRow, 2).Value = "Thực tập sinh";
                    for (int i = 3; i <= 14; i++)
                    {
                        ws.Cell(currentRow, i).Value = $"Tháng {reportInternForMonths.listMonth[i - 3]}";
                        ws.Column(i).Width = 15;
                    }
                    var header = ws.Range("A2:P2");
                    header.Style.Font.SetBold().Font.FontSize = 12;
                    header.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    header.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    //Content
                    int indexColumn = 0;
                    foreach (var reportInternForMonth in reportInternForMonths.listInternLevel)
                    {
                        currentRow++;
                        ws.Cell(currentRow, 1).Value = currentRow - 2;
                        ws.Cell(currentRow, 2).Value = reportInternForMonth.InternName;
                        for (int i = 3; i <= 14; i++)
                        {
                            foreach (var reviewDetail in reportInternForMonth.ReviewDetailForMonths)
                            {
                                if (reviewDetail.Month == i - 2)
                                {
                                    indexColumn = reviewDetail.IndexColumnExcel + 3;
                                    ws.Cell(currentRow, indexColumn).Value = reviewDetail.NewLevel != null ? reviewDetail.NewLevel.ToString() : " ";
                                    if (reviewDetail.NewLevel != null)
                                    {
                                        ws.Cell(currentRow, indexColumn).Comment.AddText(String.Format($"- Người review: {reviewDetail.ReviewerName}"));
                                        ws.Cell(currentRow, indexColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFFFFF"));
                                    }
                                    if (reviewDetail.NewLevel == null)
                                    {
                                        ws.Cell(currentRow, indexColumn).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                                    }
                                    if (reviewDetail.WarningType == InternWarningType.Orange)
                                    {
                                        ws.Cell(currentRow, indexColumn).Comment.AddNewLine().AddText(String.Format($"- Cảnh báo đối với thực tập sinh {reportInternForMonth.InternName}"));
                                        ws.Cell(currentRow, indexColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFC760"));
                                    }
                                    else if (reviewDetail.WarningType == InternWarningType.Red)
                                    {
                                        var monthWarning = (reviewDetail.NewLevel == UserLevel.Intern_0 || reviewDetail.NewLevel == UserLevel.Intern_1) ? 2 : 3;
                                        ws.Cell(currentRow, indexColumn).Comment.AddNewLine().AddText(String.Format($"- Thực tập sinh này đã giữ nguyên level {reviewDetail.NewLevel} {monthWarning} tháng"));
                                        ws.Cell(currentRow, indexColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DC3545"));
                                    }
                                    else if (reviewDetail.WarningType == InternWarningType.Staff)
                                    {
                                        ws.Cell(currentRow, indexColumn).Comment.AddText(String.Format($"- Người review: {reviewDetail.ReviewerName}"));
                                        ws.Cell(currentRow, indexColumn).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#2b982b"));
                                    }
                                    break;
                                }
                            }
                            if (String.IsNullOrEmpty(ws.Cell(currentRow, i).Value.ToString()))
                            {
                                ws.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                        }

                    }
                    ws.Cells().Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    using (var memoryStream = new MemoryStream())
                    {
                        workBook.SaveAs(memoryStream);
                        byte[] file = memoryStream.ToArray();
                        return file;
                    }
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(String.Format($"Error: {e.Message}"));
            }
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_AddNewReviewByCapability)]
        public async Task<List<string>> CreateInternCapability(ReviewInternDto input)
        {
            var fails = new List<string>();
            var IsAlreadyExist = await WorkScope.GetAll<ReviewIntern>()
                .AnyAsync(x => x.Month == input.Month && x.Year == input.Year);
            if (IsAlreadyExist)
            {
                throw new UserFriendlyException("Review Intern already exist row at Year = " + input.Year + "  month = " + input.Month);
            }

            var qmtsOfIntern = WorkScope.GetAll<MyTimesheet>()
              .Where(s => s.DateAt.Year == input.Year && s.DateAt.Month == input.Month)
              .Where(s => s.Status >= TimesheetStatus.Pending)
              .Where(s => s.User.Type == Usertype.Internship && s.User.IsActive && !s.User.IsStopWork)
              .Where(s => !s.ProjectTask.Project.isAllUserBelongTo);

            var details = await qmtsOfIntern
                .Where(x => x.DateAt.Day <= 20)
                .GroupBy(x => new { UserId = x.UserId, PositionId = x.User.PositionId, PositionName = x.User.Position.Name })
                .Select(x => new
                {
                    InternshipId = x.Key.UserId,
                    ReviewerId = AbpSession.UserId.Value,
                    PositionId = x.Key.PositionId,
                    PositionName = x.Key.PositionName,
                    CurrentLevel = x.Select(mts => mts.User.Level).FirstOrDefault(),
                }).ToListAsync();
            var userIds = details.Select(x => x.InternshipId).ToList();

            var listDonthavePosition = WorkScope.GetAll<User>()
                .Where(s => !s.PositionId.HasValue)
                .Where(s => userIds.Contains(s.Id))
                .Select(s => s.EmailAddress)
                .ToList();

            if (!listDonthavePosition.IsNullOrEmpty())
            {
                var message = string.Join("<br/>", listDonthavePosition);
                fails.Add($"<b>Không thể tạo đợt Review vì những user không có Position: </b><div class=\"text-left\" style=\"overflow-y: auto; max-height: 300px; padding: 0 55px; margin-top: 20px;\">{message}</div>");
                return fails;
            }
            var dicPositionIdToListCapabitly = WorkScope.GetAll<CapabilitySetting>()
                                                        .GroupBy(x => new { position = x.PositionId, userType = x.UserType })
                                                        .Select(s => new
                                                        {
                                                            s.Key,
                                                            setting = s.Select(x => new CreateUpdateCapabilitySettingDto
                                                            {
                                                                PositionId = x.PositionId,
                                                                UserType = x.UserType,
                                                                CapabilityId = x.CapabilityId,
                                                                CapabilityType = x.Capability.Type,
                                                                Coefficient = x.Coefficient,
                                                                GuildeLine = x.GuildeLine,
                                                            }).ToList()

                                                        }).ToDictionary(x => x.Key, x => x.setting);

            var listPositionId = details.Select(s => new { s.PositionId, s.PositionName }).Distinct();

            var listPositionUserType = listPositionId.Select(s => new { position = s.PositionId, userType = Usertype.Internship }).ToList();

            var isHaventCapability = listPositionUserType.Where(s => !dicPositionIdToListCapabitly.ContainsKey(s)).Select(s => s.position).ToList();

            if (!isHaventCapability.IsEmpty())
            {
                var message = string.Join("<br/>", listPositionId.Where(s => isHaventCapability.Contains(s.PositionId)).Select(s => s.PositionName).ToList());
                fails.Add($"<b>Không thể tạo đợt review vì những Position sau không có Capability setting:</b><div class=\"text-left\" style=\"overflow-y: auto; max-height: 300px; padding: 20px 55px 0 100px;\">{message}</div>");
                return fails;
            }

            var dicPossitionIdToName = WorkScope.GetAll<Position>().ToDictionary(s => s.Id, s => s.Name);

            var listCapabilitySetting = new List<long>();

            foreach (var item in dicPositionIdToListCapabitly)
            {
                var hasNote = !item.Value.Where(s => s.CapabilityType == CapabilityType.Note).Any();
                var hasPoint = !item.Value.Where(s => s.CapabilityType == CapabilityType.Point).Any();

                if (hasNote || hasPoint)
                {
                    listCapabilitySetting.Add(item.Key.position.Value);
                }
            }
            if (!listCapabilitySetting.IsEmpty())
            {
                var message = string.Join("<br/>", dicPossitionIdToName.Where(s => listCapabilitySetting.Contains(s.Key)).Select(s => s.Value).ToList());
                fails.Add($"<b>Không thể tạo đợt review vì những Position sau không có Capability Point hoặc Note:</b><div class=\"text-left\" style=\"overflow-y: auto; max-height: 300px; padding: 20px 55px 0 100px;\">{message}</div>");
                return fails;
            }

            var reviewIntern = ObjectMapper.Map<ReviewIntern>(input);
            input.Id = await WorkScope.InsertAndGetIdAsync(reviewIntern);

            var qmts = qmtsOfIntern
                .Select(s => new
                {
                    s.UserId,
                    s.Id,
                    s.WorkingTime,
                    s.ProjectTask.ProjectId
                });

            var qprojectPms = from s in (WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new { s.ProjectId, PMId = s.UserId }))
                              group s by s.ProjectId into g
                              select new { ProjectId = g.Key, PmId = g.FirstOrDefault().PMId };

            var mapUserPmId = await (from s in (from mts in qmts
                                                join pu in qprojectPms on mts.ProjectId equals pu.ProjectId
                                                select new { mts.UserId, mts.WorkingTime, pu.PmId })
                                     group s by s.UserId into g
                                     select new
                                     {
                                         UserId = g.Key,
                                         PM = g.GroupBy(s => s.PmId, (key, lst) => new { PmId = key, WorkingTime = lst.Sum(s => s.WorkingTime) })
                                         .OrderByDescending(s => s.WorkingTime)
                                         .FirstOrDefault()
                                     }).ToDictionaryAsync(s => s.UserId, s => s.PM.PmId);

            foreach (var x in details)
            {
                var reviewDetail = new ReviewDetail
                {
                    InternshipId = x.InternshipId,
                    ReviewerId = mapUserPmId.ContainsKey(x.InternshipId) ? (long?)mapUserPmId[x.InternshipId] : null,
                    CurrentLevel = x.CurrentLevel,
                    NewLevel = x.CurrentLevel,
                    Status = ReviewInternStatus.Draft,
                    ReviewId = input.Id,
                    Type = Usertype.Internship,
                };

                var reviewDetailId = await WorkScope.InsertAndGetIdAsync(reviewDetail);

                var listCapa = dicPositionIdToListCapabitly
                    .ContainsKey(new { position = x.PositionId, userType = reviewDetail.Type })
                    ? dicPositionIdToListCapabitly[new { position = x.PositionId, userType = reviewDetail.Type }]
                    : new List<CreateUpdateCapabilitySettingDto>();

                foreach (var item in listCapa)
                {

                    var reviewInternCapability = new ReviewInternCapability
                    {
                        CapabilityId = item.CapabilityId.Value,
                        Coefficient = item.Coefficient,
                        Point = 0,
                        Note = "",
                        ReviewDetailId = reviewDetailId
                    };

                    await WorkScope.InsertAsync(reviewInternCapability);
                }
            }
            return fails;
        }
        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_Delete)]
        public async System.Threading.Tasks.Task DeleteInternCapability(EntityDto<long> input)
        {
            bool hasReviewedDetail = await WorkScope.GetAll<ReviewDetail>().AnyAsync(x => x.ReviewId == input.Id && x.Status > ReviewInternStatus.Draft);
            if (hasReviewedDetail)
            {
                throw new UserFriendlyException("Bạn không thể xóa đợt review tts này vì đã có tts được review");
            }

            var reviewIntern = WorkScope.GetAll<ReviewIntern>()
             .Where(x => x.Id == input.Id).FirstOrDefault();

            reviewIntern.IsDeleted = true;

            var details = await WorkScope.GetAll<ReviewDetail>().Where(x => x.ReviewId == input.Id).ToListAsync();

            var reviewDetail = await WorkScope.GetAll<ReviewInternCapability>().Where(x => x.ReviewDetail.ReviewId == input.Id).ToListAsync();

            foreach (var detaild in details)
            {
                detaild.IsDeleted = true;
            }
            foreach (var review in reviewDetail)
            {
                review.IsDeleted = true;
            }
            CurrentUnitOfWork.SaveChanges();
        }
    }
}