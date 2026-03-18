using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ncc;
using Ncc.Authorization;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.CapabilitySettings.Dto;
using Timesheet.APIs.ReviewDetails.Dto;
using Timesheet.APIs.ReviewInternCapabilities.Dto;
using Timesheet.APIs.ReviewInterns.Dto;
using Timesheet.BackgroundJob;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Services.File;
using Timesheet.Services.HRM;
using Timesheet.Services.HRM.Dto;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using static Timesheet.Services.HRM.Dto.UpdateToHrmDto;

namespace Timesheet.APIs.ReviewDetails
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail)]
    public class ReviewDetailAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ExportFileService _fileService;
        private readonly ProjectService _projectService;
        private readonly HRMService _hRMService;
        private readonly KomuService _komuService;
        private readonly IConfiguration _configuration;
        private readonly UserServices _userServices;

        public ReviewDetailAppService(IBackgroundJobManager backgroundJobManager, ProjectService projectService, HRMService hRMService,
            IHostingEnvironment hostingEnvironment, ExportFileService fileService, IWorkScope workScope, KomuService komuService, IConfiguration configuration,
            UserServices userServices) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _hostingEnvironment = hostingEnvironment;
            _fileService = fileService;
            _projectService = projectService;
            _hRMService = hRMService;
            _komuService = komuService;
            _configuration = configuration;
            _userServices = userServices;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ViewAll)]
        [HttpPost]
        public async Task<PagedResultDto<ReviewDetailDto>> GetAllDetails(GridParam input, long reviewId, long? branchId)
        {

            var levelChange = input.FilterItems?.FirstOrDefault(x => x.PropertyName == "levelChange");
            int valueLevelChange = -1;
            if (levelChange != null)
            {
                valueLevelChange = Convert.ToInt32(levelChange.Value);
                input.FilterItems.Remove(levelChange);
            }
            var reviewsQuery = WorkScope.GetAll<ReviewDetail>().AsNoTracking()
                .Include(x => x.Review)
                .Include(r => r.Reviewer)
                .Include(r => r.InterShip).ThenInclude(i => i.Branch)
                .Include(r => r.InterShip).ThenInclude(i => i.Position)
                .WhereIf(!input.SearchText.IsNullOrEmpty(), x => x.InterShip.UserName != null && x.InterShip.UserName.Contains(input.SearchText))
                .Where(x => x.ReviewId == reviewId && (!branchId.HasValue || x.InterShip.BranchId == branchId))
                .Where(x => levelChange != null && valueLevelChange > -1 ? (valueLevelChange == 2 ? x.NewLevel == x.CurrentLevel : x.NewLevel != x.CurrentLevel) : true);

            var reviewDetails = await reviewsQuery.GetGridResult(reviewsQuery, input);

            var currentReview = reviewDetails.Items
                .Select(x => new { x.Review.Month, x.Review.Year })
                .FirstOrDefault();
            if (currentReview == null)
            {
                return new PagedResultDto<ReviewDetailDto> { Items = new List<ReviewDetailDto>(), TotalCount = 0 };
            }
            var internshipIds = reviewDetails.Items.Select(x => x.InternshipId).ToList();
            var previousReviewData = await WorkScope.GetAll<ReviewDetail>().AsNoTracking()
                                                 .Where(x => internshipIds.Contains(x.InternshipId))
                                                 .Where(x => x.Review.Month <= (currentReview.Month != 1 ? currentReview.Month - 1 : 12)
                                                          && x.Review.Year <= (currentReview.Month != 1 ? currentReview.Year : currentReview.Year - 1))
                                                 .OrderByDescending(x => x.Review.Year).OrderByDescending(x => x.Review.Month)
                                                 .Select(x => new { x.InternshipId, x.RateStar }).ToListAsync();

            var internInMonthAndYear = reviewDetails.Items.Select(s => s.InternshipId).ToHashSet();
            var qMyTimeSheet = (from th in WorkScope.GetAll<MyTimesheet>()
                              .Where(s => s.Status == TimesheetStatus.Approve)
                              .Where(s => !s.ProjectTask.Project.isAllUserBelongTo)
                              .Where(s => s.ProjectTask.Project.Status == ProjectStatus.Active)
                              .Where(s => s.DateAt.Month == currentReview.Month && s.DateAt.Year == currentReview.Year)
                              .Where(s => internInMonthAndYear.Contains(s.UserId))
                                select new
                                {
                                    UserId = th.UserId,
                                    ProjectId = th.ProjectTask.ProjectId,
                                    ProjectName = th.ProjectTask.Project.Name,
                                    WorkingTime = th.WorkingTime
                                }).AsNoTracking();
            var mostWorkingTimeUserProject = qMyTimeSheet.GroupBy(s => new { s.UserId, s.ProjectId, s.ProjectName })
                                                .Select(g => new
                                                {
                                                    UserId = g.Key.UserId,
                                                    ProjectName = g.Key.ProjectName,
                                                    TotalTime = g.Sum(x => x.WorkingTime)
                                                }).AsNoTracking()
                                                .ToList() // Chuyển kết quả vào bộ nhớ để xử lý
                                                .GroupBy(x => x.UserId)
                                                .Select(g => g.OrderByDescending(x => x.TotalTime).FirstOrDefault()) // Lấy project có thời gian làm việc nhiều nhất
                                                .ToDictionary(k => k.UserId, v => v.ProjectName); // UserId là key, ProjectName là value
            var result = reviewDetails.Items.Select(rv => new ReviewDetailDto
            {
                Id = rv.Id,
                InternshipId = rv.InternshipId,
                InternName = rv.InterShip.FullName,
                InternEmail = rv.InterShip.EmailAddress,
                InternAvatar = rv.InterShip.AvatarPath,
                Branch = rv.InterShip.BranchOld,
                ReviewerId = rv.ReviewerId,
                ReviewerName = rv.Reviewer.FullName,
                ReviewerEmail = rv.Reviewer.EmailAddress,
                ReviewerAvatar = rv.Reviewer.AvatarPath,
                CurrentLevel = rv.CurrentLevel,
                NewLevel = rv.NewLevel,
                UserLevel = rv.InterShip.Level,
                Status = rv.Status,
                UpdatedAt = rv.LastModificationTime ?? rv.CreationTime,
                ReviewId = rv.ReviewId,
                Note = rv.Note.IsEmpty() ? "" : rv.Note.Replace("<strong>", "").Replace("</strong>", ""),
                UpdatedId = rv.LastModifierUserId,
                UpdatedName = rv.LastModifierUserId.HasValue ? WorkScope.GetAll<User>().Where(u => u.Id == rv.LastModifierUserId).Select(x => new { FullName = x.Name + " " + x.Surname }).FirstOrDefault().FullName : null,
                Type = (rv.NewLevel >= UserLevel.FresherMinus) ? rv.Type : Usertype.Internship,
                IsFullSalary = rv.IsFullSalary,
                SubLevel = rv.SubLevel,
                RateStar = rv.RateStar,
                PreviousRateStar = previousReviewData.FirstOrDefault(p => p.InternshipId == rv.InternshipId)?.RateStar,
                BranchColor = rv.InterShip.Branch?.Color,
                BranchDisplayName = rv.InterShip.Branch?.DisplayName,
                BranchId = rv.InterShip.Branch?.Id,
                PositionShortName = rv.InterShip.Position?.ShortName,
                PositionId = rv.InterShip.Position?.Id,
                PositionColor = rv.InterShip.Position?.Color,
                Average = rv.RateStar,
                MostLoggedProject = mostWorkingTimeUserProject.ContainsKey(rv.InternshipId) ? mostWorkingTimeUserProject[rv.InternshipId] : null,
                PreviousAverage = previousReviewData.FirstOrDefault(p => p.InternshipId == rv.InternshipId)?.RateStar,
                ReviewInternPrivateNoteDtos = new List<ReviewInternPrivateNoteDto>()// the note unused
            }).OrderByDescending(s => s.InternshipId).ToList();
            return new PagedResultDto<ReviewDetailDto>
            {
                Items = result,
                TotalCount = reviewDetails.TotalCount
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_AddNew)]
        [HttpPost]
        public async System.Threading.Tasks.Task Create(ReviewDetailInputDto input)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(input.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            bool isExist = await WorkScope.GetAll<ReviewDetail>().AnyAsync(x => x.InternshipId == input.InternshipId && x.ReviewId == input.ReviewId);
            if (isExist)
            {
                throw new UserFriendlyException("Already exist Intership Id = " + input.InternshipId + " in the review");
            }
            input.Id = await WorkScope.InsertAndGetIdAsync(ObjectMapper.Map<ReviewDetail>(input));
        }

        //[AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewI)]
        public async Task<object> GetUnReviewIntership(long reviewId)
        {
            var detailCurrentIds = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.ReviewId == reviewId)
                .Select(x => x.InternshipId).ToListAsync();
            var allInternships = await WorkScope.GetAll<User>()
                .Where(x => x.IsActive == true && x.Type == Usertype.Internship)
                .Select(x => new
                {
                    InternshipId = x.Id,
                    InternName = x.FullName,
                    CurrentLevel = x.Level
                }).ToListAsync();

            var addInterships = allInternships.Where(x => !detailCurrentIds.Contains(x.InternshipId)).ToList();

            return addInterships;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var reviewDetail = await WorkScope.GetAsync<ReviewDetail>(input.Id);

            var review = await WorkScope.GetAsync<ReviewIntern>(reviewDetail.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            if (reviewDetail.Status != ReviewInternStatus.Draft)
            {
                throw new UserFriendlyException("Bạn không thể xóa tts này vì tts này đã được review");
            }

            await WorkScope.DeleteAsync(reviewDetail);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_Update)]
        [HttpPost]
        public async System.Threading.Tasks.Task Update(ReviewDetailInputDto input)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(input.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            var reviewDetail = await WorkScope.GetAsync<ReviewDetail>(input.Id);

            if (reviewDetail.Status >= ReviewInternStatus.Approved)
            {
                throw new UserFriendlyException("Bạn không thể sửa vì trạng thái là " + reviewDetail.Status);
            }

            reviewDetail.ReviewerId = input.ReviewerId;
            reviewDetail.CurrentLevel = input.CurrentLevel;

            await WorkScope.UpdateAsync(reviewDetail);
        }

        [HttpGet]
        public async Task<List<ReviewDetailDto>> Get(long Id)
        {
            var currentReview = await WorkScope.GetAll<ReviewDetail>().Include(x => x.Review).Where(x => x.Id == Id).FirstOrDefaultAsync();
            bool isFirst4M = false;
            if (currentReview != null && currentReview.CurrentLevel == UserLevel.Intern_3)
            {
                var previousMonth = currentReview.Review.Month - 1;
                var previousYear = currentReview.Review.Year;
                if (currentReview.Review.Month == 1)
                {
                    previousMonth = 12;
                    previousYear--;
                }
                var previousReviewIs4M = await WorkScope.GetAll<ReviewDetail>()
                                                .Include(x => x.Review)
                                                .Where(x => x.Review.Month <= previousMonth && x.Review.Year <= previousYear)
                                                .Where(x => x.InternshipId == currentReview.InternshipId)
                                                .Where(x => x.CurrentLevel == UserLevel.Intern_3)
                                                .AnyAsync();
                if (!previousReviewIs4M)
                {
                    isFirst4M = true;
                }
            }

            return await (WorkScope.GetAll<ReviewDetail>()
                            .Where(x => x.Id == Id)
                            .Select(x => new ReviewDetailDto
                            {
                                ReviewId = x.ReviewId,
                                InternshipId = x.InternshipId,
                                InternName = x.InterShip.FullName,
                                ReviewerId = x.ReviewerId,
                                CurrentLevel = x.CurrentLevel,
                                IsUpOfficial = x.NewLevel >= UserLevel.FresherMinus,
                                NewLevel = x.NewLevel,
                                Status = x.Status,
                                Branch = x.InterShip.BranchOld,
                                Note = x.Note.Replace("<strong>", "").Replace("</strong>", ""),
                                UpdatedAt = x.CreationTime,
                                InternAvatar = x.InterShip.AvatarPath,
                                InternEmail = x.InterShip.EmailAddress,
                                ReviewerAvatar = x.Reviewer.AvatarPath,
                                ReviewerEmail = x.Reviewer.EmailAddress,
                                Type = x.Type,
                                SubLevel = x.SubLevel,
                                IsFullSalary = x.IsFullSalary,
                                RateStar = x.RateStar,
                                IsFirst4M = isFirst4M,
                                Salary = x.Salary
                            })).ToListAsync();
        }

        public async Task<List<DetailHistoriesDto>> GetHistories(long internshipId, long reviewId)
        {
            var currentReview = await WorkScope.GetAll<ReviewDetail>()
                                               .Where(x => x.ReviewId == reviewId)
                                               .Select(x => new { x.Review.Month, x.Review.Year })
                                               .FirstOrDefaultAsync();

            var Result = WorkScope.GetAll<ReviewDetail>()
                                .Where(x => x.InternshipId == internshipId)
                                .Where(x => (x.Review.Month < currentReview.Month && x.Review.Year == currentReview.Year)
                                || x.Review.Year < currentReview.Year)
                                .Select(r => new DetailHistoriesDto
                                {
                                    FromLevel = r.CurrentLevel,
                                    ToLevel = r.NewLevel.Value,
                                    HistoryMonth = r.Review.Month,
                                    HistoryYear = r.Review.Year,
                                    ReviewerId = r.ReviewerId,
                                    ReviewerName = r.Reviewer.Name,
                                    InternshipId = r.InternshipId,
                                    RateStar = r.RateStar,
                                    Average = r.RateStar
                                }).OrderByDescending(s => s.HistoryYear)
                                .ThenByDescending(s => s.HistoryMonth);
            return await Result.ToListAsync();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_SendEmailForOneIntern)]
        public async System.Threading.Tasks.Task<object> SendEmail(long Id)
        {
            Dictionary<UserLevel, string> levelDetail = CommonUtils.UserLevelDetail();
            Dictionary<Usertype, string> typeDetail = new Dictionary<Usertype, string>()
            {
                { Usertype.Staff, "-Nhân viên <br>"},
                { Usertype.Collaborators, "-Cộng tác viên <br>" },
                { Usertype.ProbationaryStaff, "-Thử việc <br>" },
                { Usertype.Vendor, "-Nhà cung cấp <br>" }
            };
            var levelSettings = await GetLevelSetting();
            var detail = (from rv in WorkScope.GetAll<ReviewDetail>()
                          .Where(x => x.Id == Id)
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
                              InternshipName = rv.InterShip.FullName,
                              ReviewId = rv.ReviewId,
                              Year = rv.Review.Year,
                              Month = rv.Review.Month,
                              Status = rv.Status,
                              Type = rv.Type,
                              SubLevel = rv.SubLevel,
                              IsFullSalary = rv.IsFullSalary,
                              CurrentLevelDetail = rv.CurrentLevel.HasValue ? levelDetail[rv.CurrentLevel.Value] : string.Empty,
                              NewLevelDetail = rv.NewLevel.HasValue ? levelDetail[rv.NewLevel.Value] : string.Empty,
                          }).FirstOrDefault();
            if (detail == null)
            {
                throw new UserFriendlyException("Review detail Id = " + Id + " not exist");
            }
            var review = await WorkScope.GetAsync<ReviewIntern>(detail.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            if (detail.Status != ReviewInternStatus.Approved)
            {
                throw new UserFriendlyException("TTS này chưa được approve review hoặc đã gửi mail");
            }
            if (detail.InterShip.Level != detail.CurrentLevel || detail.InterShip.Level > UserLevel.Intern_3)
            {
                throw new UserFriendlyException($"User {CommonUtils.GetErrorMessageReviewIntern(detail.CurrentLevel, detail.InterShip.Level)}");
            }
            var emailHR = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHR);
            //emailHR = Convert.ToString(emailHR);

            //var emailHRDN = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRDN);
            // emailHRDN = Convert.ToString(emailHRDN);

            //var emailHRHCM = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRHCM);
            //emailHRHCM = Convert.ToString(emailHRHCM);
            //var emailHRVinh = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRVinh);

            var successResult = new object();
            var failResult = new object();
            //int success = 0;
            //int fail = 0;
            StringBuilder content = new StringBuilder("");
            try
            {
                string currentMonth = detail.Month < 10 ? "0" + detail.Month.ToString() : detail.Month.ToString();
                var applyDate = new DateTime(detail.Year, detail.Month, 1).AddMonths(1).ToString("dd/MM/yyyy");
                content.Append($"Thân gửi <span style='font-weight: 600'>{detail.InternshipName}</span>, <br> Dưới đây là phần đánh giá thực tập trong tháng <span style='font-weight: 600'> {currentMonth}/{detail.Year} </span> của bạn.");

                var mailBody = ($@"
                        <hr>
                        <table border-collapse='collapse' border='1' width='100%' style='margin-top:15px'>
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
                            <td style='padding-left: 5px'>{detail.CurrentLevel}<br>
                                <span>{detail.CurrentLevelDetail}</span>
                            </td>
                            <td style='padding-left: 5px'>{detail.NewLevel}<br>
                                <span>{((detail.NewLevel < UserLevel.FresherMinus) ? "" : typeDetail[detail.Type])}{detail.NewLevelDetail}</span>
                            </td>
                            <td style='padding-left: 5px'>{applyDate}</td>
                            <td style='padding-left: 5px; white-space: pre;'>{detail.Note}</td>
                            </tr>
                        </tbody>
                        </table>
                        <hr>");
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
                //else if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.Vinh && !String.IsNullOrEmpty(emailHRVinh))
                //{
                //    targetEmails.Add(emailHRVinh);
                //}

                await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                {
                    TargetEmails = targetEmails,
                    Body = content.ToString(),
                    Subject = emailSubject
                }, BackgroundJobPriority.High
                );

                detail.ReviewDetail.Status = ReviewInternStatus.SentEmail;
                await WorkScope.UpdateAsync(detail.ReviewDetail);
                if (detail.NewLevel != detail.InterShip.Level)
                {
                    if (detail.NewLevel >= UserLevel.FresherMinus)
                    {
                        detail.InterShip.Type = detail.Type;
                    }
                    detail.InterShip.Level = detail.NewLevel;
                    await WorkScope.UpdateAsync<User>(detail.InterShip);
                }
                //success++;
                successResult = new { detail.Id, detail.InternEmail, detail.ReviewerName };
            }
            catch (Exception e)
            {
                failResult = new { detail.Id, detail.InternEmail, detail.ReviewerName };
                Logger.Error("SendEmails() error=>" + e.Message);
                //fail++;
                return failResult;
            }
            //update Star Rating to project
            var updateUsers = new List<UpdateStarRateToProjectDto>()
                {
                    new UpdateStarRateToProjectDto()
                    {
                        UserCode = detail.InterShip.Id.ToString(),
                        EmailAddress = detail.InterShip.EmailAddress,
                        StarRate = detail.ReviewDetail.RateStar ?? 0,
                        Level = detail.InterShip.Level,
                        Type = detail.InterShip.Type
                    }
                };
            var updateSuccess = await _projectService.UpdateStarRateToProject(updateUsers);
            return successResult;
        }

        //TODO: chưa test được do chưa call được api bên hrm
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern)]
        [HttpGet]
        public async Task<string> UpdateLevelHRM(long detailId)
        {
            var detail = await WorkScope.GetAsync<ReviewDetail>(detailId);
            //if (detail == null)
            //{
            //    throw new UserFriendlyException("Review detail Id = " + detailId + " not exist");
            //}
            var review = await WorkScope.GetAsync<ReviewIntern>(detail.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            var creatorUser = WorkScope.GetAll<User>()
                  .Where(x => x.Id == AbpSession.UserId)
                  .Select(x => new
                  {
                      x.FullName,
                      x.NormalizedEmailAddress
                  })
                  .FirstOrDefault();
            var listUserToUpdate = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.Id == detailId && x.Status == ReviewInternStatus.SentEmail)
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

            var updateToHrmDto = new CreateRequestFromTSDto
            {
                CreatorNormalizedEmail = creatorUser.NormalizedEmailAddress,
                RequestName = $"Request tăng lương từ timesheet đợt {review.Month}/{review.Year} bởi {creatorUser.FullName}",
                ListUserToUpdate = listUserToUpdate,
                ExcutedDate = Convert.ToDateTime(review.Year.ToString() + "/" + review.Month.ToString() + "/01").AddMonths(1)
            };
            if (listUserToUpdate == null || listUserToUpdate.Count == 0)
            {
                throw new UserFriendlyException("TTS này chưa được thay đổi level hoặc chưa review xong");
            }

            var rs = await _hRMService.UpdateLevel(updateToHrmDto);
            return rs;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ReviewForOneIntern)]
        [HttpPost]
        public async Task<PMReviewDetailDto> PMReview(PMReviewDetailDto input)
        {
            //var detail = await WorkScope.GetAsync<ReviewDetail>(input.Id);
            var detail = await WorkScope.GetAll<ReviewDetail>().Where(x => x.Id == input.Id).Include(x => x.InterShip).Include(s => s.Review).FirstOrDefaultAsync();
            if (detail == null)
            {
                throw new UserFriendlyException("Review detail Id = " + input.Id + " not exist");
            }

            if (!detail.Review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            var canUpdateSubLevel = IsGranted(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailSubLevel);

            //ktra reviewer
            if (detail.ReviewerId != AbpSession.UserId)
            {
                throw new UserFriendlyException("Bạn không thể review TTS của PM khác");
            }
            if (detail.Status >= ReviewInternStatus.Reviewed && detail.Status != ReviewInternStatus.PmReviewed)
            {
                throw new UserFriendlyException("Bạn không thể sửa vì kết quả review cho tts này đã được gửi mail");
            }

            detail.NewLevel = input.NewLevel;
            detail.Note = input.Note;
            detail.Status = ReviewInternStatus.Reviewed;

            if (input.NewLevel >= UserLevel.FresherMinus)
            {
                var levelSettings = await GetLevelSetting();

                var levelObj = levelSettings.FirstOrDefault(s => s.Id == detail.NewLevel);
                var sublevelObj = levelObj.SubLevels.FirstOrDefault(s => s.Id == input.SubLevel);
                if (sublevelObj == null)
                {
                    sublevelObj = levelObj.SubLevels.OrderBy(s => s.Salary).FirstOrDefault();
                }

                if (sublevelObj == null)
                {
                    throw new UserFriendlyException($"Not found Sublevel setting of {input.SubLevel}");
                }

                var percentSalaryProbationary = input.IsFullSalary.Value ? 100 : Int16.Parse(SettingManager.GetSettingValue(AppSettingNames.PercentSalaryProbationary));
                var salary = sublevelObj.Salary * percentSalaryProbationary / 100;

                detail.Type = input.Type;
                detail.IsFullSalary = input.IsFullSalary;

                if (canUpdateSubLevel)
                {
                    detail.SubLevel = input.SubLevel;
                }

                detail.Salary = salary;
                detail.RateStar = input.RateStar;
            }
            else
            {
                detail.Type = Usertype.Internship;
                detail.IsFullSalary = null;
                var levelSettings = await GetLevelSetting();
                var levelObj = levelSettings.FirstOrDefault(s => s.Id == detail.NewLevel);
                detail.Salary = levelObj.Salary;
                detail.SubLevel = null;
                detail.RateStar = input.RateStar;
            }
            await WorkScope.UpdateAsync(detail);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ApproveForOneIntern)]
        public async System.Threading.Tasks.Task Approve(long Id)
        {
            var detail = await WorkScope.GetAsync<ReviewDetail>(Id);
            if (detail.Status == ReviewInternStatus.Reviewed)
            {
                detail.Status = ReviewInternStatus.Approved;
                await WorkScope.UpdateAsync(detail);
            }
            else
            {
                throw new UserFriendlyException("Review này chưa được review hoặc đã review xong hoặc đã bị reject");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_RejectForOneIntern)]
        public async System.Threading.Tasks.Task Reject(long Id)
        {
            var detail = await WorkScope.GetAll<ReviewDetail>().Where(x => x.Id == Id).FirstOrDefaultAsync();
            if (detail.Status == ReviewInternStatus.Approved || detail.Status == ReviewInternStatus.Reviewed)
            {
                detail.Status = ReviewInternStatus.Rejected;
                await SendMailWhenRejected(new List<long> { Id }, ReviewDetailRejector.CEO);
                await SendDirectMessageWhenRejected(new List<long> { Id }, ReviewDetailRejector.CEO);
                if (detail.Status == ReviewInternStatus.Rejected)
                {
                    detail.NewLevel = detail.CurrentLevel;
                }
                await WorkScope.UpdateAsync(detail);
            }
            else
            {
                throw new UserFriendlyException("Review này chưa được review hoặc đã review xong");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern)]
        public async System.Threading.Tasks.Task<object> RejectSentMail(long Id)
        {
            Dictionary<UserLevel, string> levelDetail = CommonUtils.UserLevelDetail();
            Dictionary<Usertype, string> typeDetail = new Dictionary<Usertype, string>()
            {
                { Usertype.Staff, "- Nhân viên <br>"},
                { Usertype.Collaborators, "- Cộng tác viên <br>" },
                { Usertype.ProbationaryStaff, "-Thử việc <br>" },
                { Usertype.Vendor, "-Nhà cung cấp <br>" }
            };
            var detail = (from rv in WorkScope.GetAll<ReviewDetail>()
                          .Where(x => x.Id == Id)
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
                              InternName = rv.InterShip.FullName,
                              ReviewId = rv.ReviewId,
                              Year = rv.Review.Year,
                              Month = rv.Review.Month,
                              Status = rv.Status,
                              Salary = rv.Salary,
                              IsFullSalary = rv.IsFullSalary,
                              SubLevel = rv.SubLevel,
                              CurrentLevelDetail = rv.CurrentLevel.HasValue ? levelDetail[rv.CurrentLevel.Value] : string.Empty,
                              NewLevelDetail = rv.NewLevel.HasValue ? levelDetail[rv.NewLevel.Value] : string.Empty
                          }).FirstOrDefault();
            if (detail == null)
            {
                throw new UserFriendlyException("Review detail Id = " + Id + " not exist");
            }
            var review = await WorkScope.GetAsync<ReviewIntern>(detail.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            if (detail.Status != ReviewInternStatus.SentEmail)
            {
                throw new UserFriendlyException("TTS này chưa được gửi mail");
            }
            var emailHR = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHR);
            //emailHR = Convert.ToString(emailHR);

            //var emailHRDN = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRDN);
            // emailHRDN = Convert.ToString(emailHRDN);

            //var emailHRHCM = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRHCM);
            //emailHRHCM = Convert.ToString(emailHRHCM);
            //var emailHRVinh = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRVinh);

            var successResult = new object();
            var failResult = new object();
            StringBuilder content = new StringBuilder("");
            //int success = 0;
            //int fail = 0;
            try
            {
                string currentMonth = detail.Month < 10 ? "0" + detail.Month.ToString() : detail.Month.ToString();
                var applyDate = new DateTime(detail.Year, detail.Month, 1).AddMonths(1).ToString("dd/MM/yyyy");
                var emailBody = ($@"
                        <h4>Bảng thông tin kết quả review trước đó do {detail.ReviewerName} thực hiện</h4>
                        <hr>
                        <table border-collapse='collapse' border='1' width='100%' >
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
                            <td style='padding-left: 5px'>{detail.InternName}</td>
                            <td style='padding-left: 5px'>{detail.CurrentLevel}<br>
                                <span>{detail.CurrentLevelDetail}</span>
                            </td>
                            <td style='padding-left: 5px'>{detail.NewLevel}<br>
                                <span>{((detail.NewLevel < UserLevel.FresherMinus) ? "" : typeDetail[detail.ReviewDetail.Type])}{detail.NewLevelDetail}</span>
                            </td>
                            <td style='padding-left: 5px'>{applyDate}</td>
                            <td style='padding-left: 5px; white-space: pre;'>{detail.Note}</td>
                            </tr>
                        </tbody>
                        </table>
                        <hr>");
                content.Append(emailBody);
                content.Append("Mọi thông tin thắc mắc về nội dung đánh giá vui lòng liên hệ trực tiếp với PM để được giải đáp.");
                var emailSubject = $"[NCC] Thông báo huỷ kết quả review mức hỗ trợ đối với TTS {detail.InternName} {detail.Month}/{detail.Year}";

                var targetEmails = new List<string>() { detail.InternEmail, emailHR, detail.ReviewerEmail };

                // if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.DaNang && !String.IsNullOrEmpty(emailHRDN))
                // {
                //     targetEmails.Add(emailHRDN);
                // }
                // else if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.HoChiMinh && !String.IsNullOrEmpty(emailHRHCM))
                // {
                //     targetEmails.Add(emailHRHCM);
                // }
                // else if (detail.InternBranch == Ncc.Entities.Enum.StatusEnum.Branch.Vinh && !String.IsNullOrEmpty(emailHRVinh))
                // {
                //     targetEmails.Add(emailHRVinh);
                // }

                await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                {
                    TargetEmails = targetEmails,
                    Body = content.ToString(),
                    Subject = emailSubject
                }, BackgroundJobPriority.High
                );

                detail.ReviewDetail.Status = ReviewInternStatus.Rejected;
                await WorkScope.UpdateAsync(detail.ReviewDetail);
                if (detail.CurrentLevel != detail.InterShip.Level)
                {
                    if (detail.CurrentLevel < UserLevel.FresherMinus)
                    {
                        detail.InterShip.Type = Usertype.Internship;
                    }
                    detail.InterShip.Level = detail.CurrentLevel;
                    await WorkScope.UpdateAsync<User>(detail.InterShip);
                }
                //update level to HRM
                var isUpdateHRMSuccess = await _hRMService.UpdateLevelAfterRejectEmail(new UpdateLevelHRMDto()
                {
                    UserId = detail.InterShip.Id,
                    EmailAddress = detail.InterShip.EmailAddress,
                    NewLevel = detail.CurrentLevel,
                    Type = detail.InterShip.Type,
                    IsFullSalary = detail.IsFullSalary,
                    Salary = detail.Salary,
                    SubLevel = detail.SubLevel,
                    RequestName = "Request tăng lương từ timesheet đợt " + review.Month.ToString() + "/" + review.Year.ToString(),
                    UpdateToHRMByUserId = AbpSession.UserId.Value,
                    ExcutedDate = Convert.ToDateTime(review.Year.ToString() + "/" + review.Month.ToString() + "/01").AddMonths(1)
                });
                //success++;
                successResult = new { detail.Id, detail.InternEmail, detail.ReviewerName };
            }
            catch (Exception e)
            {
                failResult = new { detail.Id, detail.InternEmail, detail.ReviewerName };
                Logger.Error("SendEmails() error=>" + e.Message);
                //fail++;
                return failResult;
            }
            //update Star Rating to project
            var month = detail.Month;
            var year = detail.Year;
            if (month == 1)
            {
                month = 12;
                year--;
            }
            else
                month--;
            var previousStarRate = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.Review.Month <= month && x.Review.Year <= year && x.InternshipId == detail.InterShip.Id).Select(x => x.RateStar).FirstOrDefaultAsync();
            var updateUsers = new List<UpdateStarRateToProjectDto>()
                {
                    new UpdateStarRateToProjectDto
                    {
                        UserCode = detail.InterShip.Id.ToString(),
                        EmailAddress = detail.InterShip.EmailAddress,
                        StarRate = previousStarRate ?? 0,
                        Level = detail.InterShip.Level,
                        Type = detail.InterShip.Type
                    }
                };
            var updateProjectSuccess = await _projectService.UpdateStarRateToProject(updateUsers);
            return successResult;
        }

        public async Task<List<LevelSettingDto>> GetLevelSetting()
        {
            var levelSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.UserLevelSetting);
            var rs = JsonConvert.DeserializeObject<List<LevelSettingDto>>(levelSetting);
            return rs;
        }

        //TODO: chưa test được do chưa call được api bên hrm
        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> PrintContract(long reviewDetailId)
        {
            var reviewDetail = await WorkScope.GetRepo<ReviewDetail>()
                .GetAllIncluding(r => r.InterShip)
                .Where(r => r.Id == reviewDetailId && r.NewLevel >= UserLevel.FresherMinus)
                .FirstOrDefaultAsync();
            if (reviewDetail == null)
            {
                throw new UserFriendlyException(string.Format($"ReviewDetail id {reviewDetailId} is not exist."));
            }
            else
            {
                var user = await _hRMService.GetUserContract(reviewDetail.InternshipId);
                if (user == null)
                {
                    throw new UserFriendlyException(String.Format("No information about this internship."));
                }

                string templateFile = reviewDetail.Type == Usertype.Staff ? "HDTV" : "HDCTV";
                string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, $"employment-contract-templates\\{templateFile}.docx");

                string fileNameOutput = $"{templateFile}_{reviewDetail.InterShip.FullName.Replace(" ", "-")}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                string saveAsToPath = Path.Combine(_hostingEnvironment.ContentRootPath, $"FileExport\\Contract\\{fileNameOutput}.docx");

                Dictionary<string, string> bookmarks = new Dictionary<string, string>
                            {
                                {"ContractNumber", user.ContractId.ToString() },
                                {"Sex", reviewDetail.InterShip.Sex.Equals(Sex.Male)? "Ông" : "Bà" },
                                {"FullName", reviewDetail.InterShip.FullName },
                                {"DateOfBirth", user.DOB != null ? String.Format("{0:dd-MM-yyyy}", user.DOB) : ""},
                                {"Address", reviewDetail.InterShip.Address },
                                {"IdCard", user.IdCard },
                                {"IssuedOn",  user.IssuedOn != null ? String.Format("{0:dd-MM-yyyy}", user.IssuedOn) : "" },
                                {"IssuedBy", user.IssuedBy },
                                {"StartDate", String.Format("{0:dd-MM-yyyy}", user.StartDate) },
                                {"EndDate", user.EndDate != null? String.Format("{0:dd-MM-yyyy}", user.EndDate) :  String.Format("{0:dd-MM-yyyy}", DateTime.Now.AddDays(60)) },
                                {"AddressCompany", user.AddressCompany },
                                {"Regency", user.Job != null? user.Job.ToString() : "" },
                                {"ContractSalary", string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:#,##0.##}", user.ContractSalary) },
                                {"ContractIssuedAt", user.ContractIssuedAt != null ? String.Format("{0:dd-MM-yyyy}", user.ContractIssuedAt) : "" },
                                {"SignFullName", reviewDetail.InterShip.FullName },
                            };
                var memoryStream = await _fileService.ExportFileWord(fullPath, bookmarks, saveAsToPath);
                var fileStreamResult = new FileStreamResult(memoryStream, "application/msword")
                {
                    FileDownloadName = fileNameOutput
                };
                return fileStreamResult;
            }

            return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest };
        }
        //TODO: chưa test được do chưa call được api bên hrm
        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult> PrintNDA(long reviewDetailId)
        {
            var reviewDetail = await WorkScope.GetRepo<ReviewDetail>()
                .GetAllIncluding(r => r.InterShip)
                .Where(r => r.Id == reviewDetailId && r.NewLevel >= UserLevel.FresherMinus)
                .FirstOrDefaultAsync();
            if (reviewDetail == null)
            {
                throw new UserFriendlyException(string.Format($"ReviewDetail id {reviewDetailId} is not exist."));
            }
            else
            {
                var user = await _hRMService.GetUserContract(reviewDetail.InternshipId);
                if (user == null)
                {
                    throw new UserFriendlyException(String.Format("No information about this internship."));
                }

                string templateFile = "NDA2021-NCCPLUS_V1";
                string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, $"employment-contract-templates\\{templateFile}.docx");

                string fileNameOutput = $"{templateFile}_{reviewDetail.InterShip.FullName.Replace(" ", "-")}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                string saveAsToPath = Path.Combine(_hostingEnvironment.ContentRootPath, $"FileExport\\NDA\\{fileNameOutput}.docx");

                Dictionary<string, string> bookmarks = new Dictionary<string, string>()
                            {
                                {"Day", DateTime.Now.Day.ToString() },
                                {"Month", DateTime.Now.Month.ToString() },
                                {"Year", DateTime.Now.Year.ToString() },
                                {"AddressCompany", user.AddressCompany },
                                {"FullName", reviewDetail.InterShip.FullName },
                                {"Address", reviewDetail.InterShip.Address },
                                {"PhoneNumber", reviewDetail.InterShip.PhoneNumber },
                                {"Regency", user.Job != null? user.Job.ToString() : "" },
                                {"IdCard", user.IdCard },
                                {"IssuedOn",  user.IssuedOn != null ? String.Format("{0:dd-MM-yyyy}", user.IssuedOn) : "" },
                                {"IssuedBy", user.IssuedBy },
                                {"SignFullName", reviewDetail.InterShip.FullName },
                            };
                var memoryStream = await _fileService.ExportFileWord(fullPath, bookmarks, saveAsToPath);
                var fileStreamResult = new FileStreamResult(memoryStream, "application/msword")
                {
                    FileDownloadName = fileNameOutput
                };
                return fileStreamResult;
            }
            return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest };
        }
        [HttpPost]
        [AbpAuthorize(PermissionNames.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern)]
        public async Task<ReviewDetailDto> ConfirmApproveSalary(ReviewDetailDto input)
        {
            var reviewDetail = await WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.Id == input.Id)
                .Include(x => x.InterShip)
                .Include(s => s.Review)
                .FirstOrDefaultAsync();

            if (reviewDetail == null)
            {
                throw new UserFriendlyException("Review detail Id = " + input.Id + " not exist");
            }

            if (!reviewDetail.Review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            reviewDetail.NewLevel = input.NewLevel;
            reviewDetail.Type = input.Type.Value;
            reviewDetail.SubLevel = input.SubLevel;
            reviewDetail.IsFullSalary = input.IsFullSalary;
            reviewDetail.Note = input.Note;
            reviewDetail.Salary = input.Salary;

            await WorkScope.UpdateAsync(reviewDetail);
            return input;
        }

        [HttpGet]
        public async Task<List<NewReviewDetailDto>> GetInternCapability(long Id)
        {
            var currentReview = await WorkScope.GetAll<ReviewDetail>().Include(x => x.Review).Where(x => x.Id == Id).FirstOrDefaultAsync();
            var userInfo = WorkScope.GetAll<User>()
               .Select(s => new
               {
                   s.Id,
                   s.Type,
                   s.PositionId,
               })
               .Where(s => currentReview.InternshipId == s.Id)
               .FirstOrDefault();

            bool isFirst4M = false;
            if (currentReview != null && currentReview.CurrentLevel == UserLevel.Intern_3)
            {
                var previousMonth = currentReview.Review.Month - 1;
                var previousYear = currentReview.Review.Year;
                if (currentReview.Review.Month == 1)
                {
                    previousMonth = 12;
                    previousYear--;
                }
                var previousReviewIs4M = await WorkScope.GetAll<ReviewDetail>()
                                                .Include(x => x.Review)
                                                .Where(x => x.Review.Month <= previousMonth && x.Review.Year <= previousYear)
                                                .Where(x => x.InternshipId == currentReview.InternshipId)
                                                .Where(x => x.CurrentLevel == UserLevel.Intern_3)
                                                .AnyAsync();
                if (!previousReviewIs4M)
                {
                    isFirst4M = true;
                }
            }
            var dictGuideline = WorkScope.GetAll<CapabilitySetting>()
                                         .Where(s => s.PositionId == userInfo.PositionId)
                                         .Where(s => s.UserType == userInfo.Type)
                                         .Select(s => new
                                         {
                                             s.CapabilityId,
                                             s.GuildeLine
                                         })
                                         .ToDictionary(s => s.CapabilityId, s => s.GuildeLine);

            var interListCapa = WorkScope.GetAll<ReviewInternCapability>().Where(x => x.ReviewDetailId == Id).Select(x => new ReviewInterCapabilityDto
            {
                ReviewDetailId = x.ReviewDetailId,
                Id = x.Id,
                Confficent = x.Coefficient,
                CapabilityName = x.Capability.Name,
                CapabilityType = x.Capability.Type,
                GuideLine = dictGuideline.ContainsKey(x.CapabilityId) ? dictGuideline[x.CapabilityId] : null,
                Note = x.Note,
                Point = x.Point,
            }).ToList();
            return await (WorkScope.GetAll<ReviewDetail>()
                            .Where(x => x.Id == Id)
                            .Select(x => new NewReviewDetailDto
                            {
                                Id = x.Id,
                                ReviewId = x.ReviewId,
                                InternshipId = x.InternshipId,
                                InternName = x.InterShip.FullName,
                                ReviewerId = x.ReviewerId,
                                CurrentLevel = x.CurrentLevel,
                                NewLevel = x.NewLevel,
                                IsUpOfficial = x.NewLevel >= UserLevel.FresherMinus,
                                Status = x.Status,
                                Branch = x.InterShip.BranchOld, 
                                Note = x.Note.Replace("<strong>", "").Replace("</strong>", ""),
                                UpdatedAt = x.CreationTime,
                                InternAvatar = x.InterShip.AvatarPath,
                                InternEmail = x.InterShip.EmailAddress,
                                ReviewerAvatar = x.Reviewer.AvatarPath,
                                ReviewerEmail = x.Reviewer.EmailAddress,
                                Type = x.Type,
                                SubLevel = x.SubLevel,
                                IsFullSalary = x.IsFullSalary,
                                Average = x.RateStar,
                                RateStar = x.RateStar,
                                IsFirst4M = isFirst4M,
                                PositionId = x.InterShip.PositionId,
                                PositionShortName = x.InterShip.Position.ShortName,
                                ReviewInternCapabilities = interListCapa
                                //Salary = x.Salary
                            })).ToListAsync();
        }
        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_Delete)]
        public async System.Threading.Tasks.Task DeleteInternCapability(EntityDto<long> input)
        {
            var reviewDetail = await WorkScope.GetAsync<ReviewDetail>(input.Id);
            var review = await WorkScope.GetAsync<ReviewIntern>(reviewDetail.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            if (reviewDetail.Status > ReviewInternStatus.Draft && reviewDetail.Type == Usertype.Internship)
            {
                throw new UserFriendlyException("Không thể xoá internship có trạng thái khác draft hoặc rejected");
            }
            {
                await WorkScope.DeleteAsync(reviewDetail);
                var reviewInter = await WorkScope.GetAll<ReviewInternCapability>().Where(s => s.ReviewDetailId == input.Id).ToListAsync();
                await WorkScope.DeleteRangeAsync(reviewInter);
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern)]
        public async Task<NewPMReviewDetailDto> PMReviewInternCapability(NewPMReviewDetailDto input)
        {
            //var detail = await WorkScope.GetAsync<ReviewDetail>(input.Id);
            var detail = await WorkScope.GetAll<ReviewDetail>().Where(x => x.Id == input.Id).Include(x => x.InterShip).Include(s => s.Review).FirstOrDefaultAsync();
            if (detail == null)
            {
                throw new UserFriendlyException("Review detail Id = " + input.Id + " not exist");
            }
            if (!detail.Review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            var canUpdateSubLevel = IsGranted(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailSubLevel);
            //ktra reviewer
            if (detail.ReviewerId != AbpSession.UserId)
            {
                throw new UserFriendlyException("Bạn không thể review TTS của PM khác");
            }
            if (detail.Status >= ReviewInternStatus.Reviewed && detail.Status != ReviewInternStatus.ReOpen && detail.Status != ReviewInternStatus.PmReviewed)
            {
                throw new UserFriendlyException("Bạn không thể sửa vì kết quả review cho tts này đã được gửi mail");
            }

            detail.NewLevel = input.NewLevel;
            //detail.Note = input.Note;
            detail.Status = ReviewInternStatus.PmReviewed;
            if (input.NewLevel >= UserLevel.FresherMinus)
            {
                var levelSettings = await GetLevelSetting();
                var levelObj = levelSettings.FirstOrDefault(s => s.Id == detail.NewLevel);
                var sublevelObj = levelObj.SubLevels.FirstOrDefault(s => s.Id == input.SubLevel);
                if (sublevelObj == null)
                {
                    sublevelObj = levelObj.SubLevels.OrderBy(s => s.Salary).FirstOrDefault();
                }
                if (sublevelObj == null)
                {
                    throw new UserFriendlyException($"Not found Sublevel setting of {input.SubLevel}");
                }
                var percentSalaryProbationary = input.IsFullSalary.Value ? 100 : Int16.Parse(SettingManager.GetSettingValue(AppSettingNames.PercentSalaryProbationary));
                var salary = sublevelObj.Salary * percentSalaryProbationary / 100;
                detail.Type = input.Type;
                detail.IsFullSalary = input.IsFullSalary;
                if (canUpdateSubLevel)
                {
                    detail.SubLevel = input.SubLevel;
                }
                detail.Salary = salary;
                //detail.RateStar = input.RateStar;
            }
            else
            {
                detail.Type = Usertype.Internship;
                detail.IsFullSalary = null;
                var levelSettings = await GetLevelSetting();
                var levelObj = levelSettings.FirstOrDefault(s => s.Id == detail.NewLevel);
                detail.Salary = levelObj.Salary;
                detail.SubLevel = null;
                //detail.RateStar = input.RateStar;
            }
            var currentReviewInternCapability = await WorkScope.GetAll<ReviewInternCapability>().Include(x => x.Capability).
                Where(s => s.ReviewDetailId == input.Id)
                .OrderBy(s => s.Capability.Type)
                .ToListAsync();
            var updateReviewInternCapability = (from c in currentReviewInternCapability
                                                join i in input.reviewInternCapabilities on c.Id equals i.Id
                                                select new
                                                {
                                                    ReviewInternCapability = c,
                                                    Dto = i
                                                }).ToList();

            var userInfo = WorkScope.GetAll<User>()
               .Select(s => new
               {
                   s.Id,
                   s.Type,
                   s.PositionId,
                   s.Position.Name
               })
               .Where(s => detail.InternshipId == s.Id)
               .FirstOrDefault();

            foreach (var item in updateReviewInternCapability)
            {
                item.ReviewInternCapability.Point = item.Dto.Point;
                item.ReviewInternCapability.Note = item.Dto.Note;
                await WorkScope.UpdateAsync<ReviewInternCapability>(item.ReviewInternCapability);
            }
            float totalPoint = 0;
            float totalCoefficient = 0;
            StringBuilder sbNote = new StringBuilder();
            sbNote.AppendLine("<strong>Điểm trung bình: {{Điểm trung bình}}/5</strong>");
            foreach (var item in currentReviewInternCapability)
            {
                if (item.Capability.Type == CapabilityType.Note)
                {
                    sbNote.AppendLine($"{item.Capability.Name}:");
                    sbNote.AppendLine($"    {item.Note.Replace("\n", "\n    ")}");
                }
                if (item.Capability.Type == CapabilityType.Point)
                {
                    sbNote.AppendLine($"{item.Capability.Name} (x{item.Coefficient}): {item.Point}/5");
                }
                if (item.Capability.Type == CapabilityType.Point)
                {
                    var itemPoint = item.Point * item.Coefficient;
                    totalPoint += itemPoint;
                    totalCoefficient += item.Coefficient;
                }
            }
            detail.RateStar = (float)Math.Round((totalPoint / totalCoefficient), 2);
            sbNote.Replace("{{Điểm trung bình}}", detail.RateStar.ToString());
            detail.Note = sbNote.ToString();

            //add PM comment
            if (input.PrivateNote != null)
            {
                var reviewInternComment = new ReviewInternPrivateNote
                {
                    ReviewDetailId = input.Id,
                    NoteByUserId = AbpSession.UserId.Value,
                    PrivateNote = input.PrivateNote.Trim()
                };
                await WorkScope.InsertAsync(reviewInternComment);
            }
            await WorkScope.UpdateAsync(detail);

            List<long> listId = new List<long> { detail.Id };
            bool check = GetReviewInternStatusSummary(detail.ReviewId, ReviewInternStatus.PmReviewed, listId).hasAllSameStatus;
            if(check)
            {
                string headPmEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);
                int notifyHeadPmReviewInternOnDate = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMReviewInternOnDate));
                await SendMailToNotifyTransition(headPmEmail, ReviewInternStatus.PmReviewed, detail.ReviewId, notifyHeadPmReviewInternOnDate);
                await SendDirectMessageToNotifyTransition(headPmEmail, ReviewInternStatus.PmReviewed, detail.ReviewId, notifyHeadPmReviewInternOnDate);
            }

            return input;
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_AddNew)]
        public async System.Threading.Tasks.Task CreateInternCapability(ReviewDetailInputDto input)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(input.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }
            bool isExist = await WorkScope.GetAll<ReviewDetail>().AnyAsync(x => x.InternshipId == input.InternshipId && x.ReviewId == input.ReviewId);
            if (isExist)
            {
                throw new UserFriendlyException("Already exist Intership Id = " + input.InternshipId + " in the review");
            }

            var userInfo = await WorkScope.GetAll<User>()
                .Select(s => new
                {
                    s.Id,
                    s.Type,
                    s.PositionId,
                    PositionName = s.Position.Name
                })
                .Where(s => input.InternshipId == s.Id)
                .FirstOrDefaultAsync();

            if (userInfo.PositionId == null)
            {
                throw new UserFriendlyException("Không thể add employee vì employee không có Position ");
            }

            var capabilities = await WorkScope.GetAll<CapabilitySetting>()
                                         .Where(s => s.UserType == userInfo.Type)
                                         .Where(s => s.PositionId == userInfo.PositionId)
                                         .Select(s => new { s.CapabilityId, s.Coefficient, CapabilityType = s.Capability.Type })
                                         .ToListAsync();

            if (capabilities.FirstOrDefault() == null)
            {
                throw new UserFriendlyException($"Không thể add employee vì Position: {userInfo.PositionName} không có Capability setting");
            }

            var hasNote = !capabilities.Where(s => s.CapabilityType == CapabilityType.Note).Any();
            var hasPoint = !capabilities.Where(s => s.CapabilityType == CapabilityType.Point).Any();

            if (hasPoint || hasNote)
            {
                throw new UserFriendlyException($"Không thể add employee vì Position: {userInfo.PositionName} không có Capability Point hoặc Note ");
            }

            input.Id = await WorkScope.InsertAndGetIdAsync(ObjectMapper.Map<ReviewDetail>(input));

            var reviewInternCapability = new List<ReviewInternCapability>();
            capabilities.ForEach(s =>
            {
                reviewInternCapability.Add(new ReviewInternCapability
                {
                    CapabilityId = s.CapabilityId,
                    Coefficient = s.Coefficient,
                    Point = 0,
                    Note = "",
                    ReviewDetailId = input.Id
                });
            });
            await WorkScope.InsertRangeAsync(reviewInternCapability);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ChangeReviewer)]
        [HttpPost]
        public async System.Threading.Tasks.Task UpdateReviewer(ReviewDetailInputDto input)
        {
            var review = await WorkScope.GetAsync<ReviewIntern>(input.ReviewId);
            if (!review.IsActive)
            {
                throw new UserFriendlyException("Review Phase not Active");
            }

            var reviewDetail = await WorkScope.GetAsync<ReviewDetail>(input.Id);

            if (reviewDetail.Status >= ReviewInternStatus.Approved)
            {
                throw new UserFriendlyException("Bạn không thể sửa vì trạng thái là " + reviewDetail.Status);
            }

            reviewDetail.ReviewerId = input.ReviewerId;

            await WorkScope.UpdateAsync(reviewDetail);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_VerifyPmReviewedForOneIntern)]
        [HttpPost]
        public async System.Threading.Tasks.Task HeadPmVerify(HeadPmVerifyDto input)
        {
            var detail = await WorkScope.GetAsync<ReviewDetail>(input.ReviewDetailId);

            if (detail.Status != ReviewInternStatus.PmReviewed)
            {
                throw new UserFriendlyException("Review này chưa được PM review hoặc đã review xong");
            }

            detail.Status = input.Status;

            if (input.Status == ReviewInternStatus.Rejected)
            {
                await SendMailWhenRejected(new List<long> { input.ReviewDetailId }, ReviewDetailRejector.HeadPM);
                await SendDirectMessageWhenRejected(new List<long> { input.ReviewDetailId }, ReviewDetailRejector.HeadPM);
                detail.NewLevel = detail.CurrentLevel;
            }

            await WorkScope.UpdateAsync(detail);
            if (input.Status == ReviewInternStatus.Reviewed)
            {
                List<long> listId = new List<long> { detail.Id };
                await CheckSendMailToPresident(detail.ReviewId, listId);
            }
        }

        public async Task SendMailWhenRejected(List<long> reviewDetailIds, ReviewDetailRejector rejector)
        {
            var data = await GetReviewDataByDetailIdsAsync(reviewDetailIds);
            int date = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMReviewInternOnDate));
            var dateNow = DateTimeUtils.GetNow();
            Dictionary<ReviewInternStatus, string> statusDictionary = CommonUtils.ReviewInternStatusString();
            var deadlineDate = dateNow.AddDays(1);
            if (data == null)
            {
                return;
            }

            var hrEmails = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHrEmail)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(email => email.Trim())
                .ToList();

            string rejectorDisplay = rejector == ReviewDetailRejector.HeadPM ? "Head PM" : "CEO";

            foreach (var reviewer in data.Reviewers)
            {
                StringBuilder content = new StringBuilder();
                try
                {
                    content.Append($"<span style='font-weight: 600'>{rejectorDisplay}</span> đã chuyển trạng thái <span style='font-weight: 600'> {statusDictionary[ReviewInternStatus.Rejected]} </span> cho bản ghi đánh giá thực tập sinh trong đợt đánh giá tháng <span style='font-weight: 600'> {data.MonthReviewIntern}/{data.YearReviewIntern}. </span> ");
                    content.Append($"<br>Thông tin thực tập sinh bao gồm: <br>");
                    var tableHtml = $@"<table border-collapse='collapse' border='1' width='80%' style='margin-top: 15px'>
                                            <thead> 
                                                <tr>
                                                    <th width='20%'><span style='font-weight: 600'>Intern name</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Current level</span></th> 
                                                    <th width='20%'><span style='font-weight: 600'>Level after PM reviewed</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Status</span></th>
                                                </tr>
                                            </thead> 
                                            <tbody>";
                    foreach (var intern in reviewer.Interns)
                    {
                        tableHtml += $@"
                                    <tr> 
                                        <td style='padding-left: 5px; text-align: center'>{intern.InternFullName}</td> 
                                        <td style='padding-left: 5px; text-align: center'>{intern.InternCurrentLevel}</td>
                                        <td style='padding-left: 5px; text-align: center'>{intern.InternNewLevel}</td> 
                                        <td style='padding-left: 5px; text-align: center'>{intern.ReviewStatus}</td>
                                    </tr>";
                    }
                    tableHtml += @"
                        </tbody> 
                    </table>";
                    content.Append(tableHtml);
                    content.Append("<br>");
                    content.Append($"Vui lòng trao đổi với <span style='font-weight: 600'> Head PM </span>về thực tập sinh trên và thực hiện đánh giá lại trên <span style='font-weight: 600'> Timesheet </span> trước ngày <span style='font-weight: 600'>{deadlineDate:dd/MM/yyyy}. </span>");
                    content.Append("<br>");

                    var emailSubject = $"[NCC] [Review Intern {data.MonthReviewIntern}/{data.YearReviewIntern}] Thông báo cập nhật đánh giá thực tập sinh theo phản hồi từ {rejectorDisplay}";
                    var targetEmails = new List<string> { reviewer.ReviewerEmail };

                    await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = targetEmails,
                        Body = content.ToString(),
                        Subject = emailSubject
                    }, BackgroundJobPriority.High);
                }
                catch (Exception e)
                {
                    Logger.Error("SendMailToNotifyNewReviewDetail() error => " + e.Message);
                }
            }

            if (hrEmails.Any())
            {
                var allReviewsForHR = new List<ReviewDetailForHRDto>();

                foreach (var reviewer in data.Reviewers)
                {
                    foreach (var intern in reviewer.Interns)
                    {
                        allReviewsForHR.Add(new ReviewDetailForHRDto
                        {
                            ReviewerFullName = reviewer.ReviewerFullName,
                            InternFullName = intern.InternFullName,
                            InternCurrentLevel = intern.InternCurrentLevel,
                            InternNewLevel = intern.InternNewLevel,
                            ReviewStatus = intern.ReviewStatus
                        });
                    }
                }

                StringBuilder hrContent = new StringBuilder();
                try
                {
                    hrContent.Append($"<span style='font-weight: 600'>{rejectorDisplay}</span> đã chuyển trạng thái <span style='font-weight: 600'> {statusDictionary[ReviewInternStatus.Rejected]} </span> cho bản ghi đánh giá thực tập sinh trong đợt đánh giá tháng <span style='font-weight: 600'> {data.MonthReviewIntern}/{data.YearReviewIntern}. </span> ");
                    hrContent.Append($"<br>Thông tin thực tập sinh bao gồm: <br>");
                    var hrTableHtml = $@"<table border-collapse='collapse' border='1' width='80%' style='margin-top: 15px'>
                                            <thead>
                                                <tr>
                                                    <th width='20%'><span style='font-weight: 600'>Intern name</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Reviewer name</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Current level</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Level after PM reviewed</span></th>
                                                    <th width='20%'><span style='font-weight: 600'>Status</span></th>
                                                </tr>
                                            </thead>
                                            <tbody>";
                    foreach (var item in allReviewsForHR)
                    {
                        hrTableHtml += $@"
                            <tr>
                                <td style='padding-left: 5px; text-align: center'>{item.InternFullName}</td>
                                <td style='padding-left: 5px; text-align: center'>{item.ReviewerFullName}</td>
                                <td style='padding-left: 5px; text-align: center'>{item.InternCurrentLevel}</td>
                                <td style='padding-left: 5px; text-align: center'>{item.InternNewLevel}</td>
                                <td style='padding-left: 5px; text-align: center'>{item.ReviewStatus}</td>
                            </tr>";
                    }
                    hrTableHtml += @"
                        </tbody>
                    </table>";
                    hrContent.Append(hrTableHtml);
                    hrContent.Append("<br>");
                    var hrEmailSubject = $"[NCC] [Review Intern {data.MonthReviewIntern}/{data.YearReviewIntern}] Thông báo cập nhật đánh giá thực tập sinh theo phản hồi từ {rejectorDisplay}";
                    await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = hrEmails,
                        Body = hrContent.ToString(),
                        Subject = hrEmailSubject
                    }, BackgroundJobPriority.High);
                }
                catch (Exception e)
                {
                    Logger.Error("SendMailWhenHeadPMReject() error => " + e.Message);
                }
            }
        }
        
        public async Task SendDirectMessageWhenRejected(List<long> reviewDetailIds, ReviewDetailRejector rejector)
        {
            var data = await GetReviewDataByDetailIdsAsync(reviewDetailIds);
            int date = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMReviewInternOnDate));
            var dateNow = DateTimeUtils.GetNow();
            Dictionary<ReviewInternStatus, string> statusDictionary = CommonUtils.ReviewInternStatusString();
            var deadlineDate = dateNow.AddDays(1);
            if (data == null)
            {
                return;
            }

            string rejectorDisplay = rejector == ReviewDetailRejector.HeadPM ? "Head PM" : "CEO";

            const int BATCH_SIZE = 5;
            const int MESSAGE_DELAY_MS = 1000;

            foreach (var reviewer in data.Reviewers)
            {
                try
                {
                    StringBuilder reviewerHeaderMessage = new StringBuilder();
                    reviewerHeaderMessage.AppendLine($"**[NCC] [Review Intern {data.MonthReviewIntern}/{data.YearReviewIntern}] Thông báo đánh giá thực tập sinh**");
                    reviewerHeaderMessage.AppendLine();
                    reviewerHeaderMessage.AppendLine($"**{rejectorDisplay}**đã chuyển trạng thái**{statusDictionary[ReviewInternStatus.Rejected]}**cho bản ghi đánh giá thực tập sinh trong đợt đánh giá tháng**{data.MonthReviewIntern}/{data.YearReviewIntern}**");
                    reviewerHeaderMessage.AppendLine("Thông tin thực tập sinh bao gồm:");
                    reviewerHeaderMessage.AppendLine("");
                    _komuService.SendSimpleNotificationToUser(reviewerHeaderMessage.ToString(), reviewer.ReviewerUserName);
                    await Task.Delay(MESSAGE_DELAY_MS);

                    var chunks = CommonUtils.SplitIntoChunks(reviewer.Interns.Cast<dynamic>().ToList(), BATCH_SIZE);
                    int idx = 1;
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];
                        bool isLastChunk = i == chunks.Count - 1;
                        StringBuilder reviewerChunkMessage = new StringBuilder();
                        foreach (var item in chunk)
                        {
                            reviewerChunkMessage.AppendLine($"{idx}. Intern name:**{item.InternUserName}**");
                            reviewerChunkMessage.AppendLine($"- Current level:**{item.InternCurrentLevel}**");
                            reviewerChunkMessage.AppendLine($"- Level after PM reviewed:**{item.InternNewLevel}**");
                            reviewerChunkMessage.AppendLine($"- Status:**{item.ReviewStatus}**");
                            reviewerChunkMessage.AppendLine("");
                            idx++;
                        }
                        if (isLastChunk)
                        {
                            reviewerChunkMessage.AppendLine($"Vui lòng trao đổi với**Head PM**về thực tập sinh trên và thực hiện đánh giá lại trên**Timesheet**trước ngày**{deadlineDate:dd/MM/yyyy}**");
                        }
                        _komuService.SendSimpleNotificationToUser(reviewerChunkMessage.ToString(), reviewer.ReviewerUserName);
                        await Task.Delay(MESSAGE_DELAY_MS);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("SendKomuMessageWhenHeadPMReject() error for reviewer => " + e.Message);
                }
            }
        }

        public async Task<ReviewDataDto> GetReviewDataByDetailIdsAsync(List<long> reviewDetailIds)
        {
            if (!reviewDetailIds.Any())
            {
                return null;
            }

            var reviewDetails = await WorkScope.GetAll<ReviewDetail>()
                .Where(d => reviewDetailIds.Contains(d.Id))
                .ToListAsync();

            if (!reviewDetails.Any())
            {
                return null;
            }

            var reviewId = reviewDetails.First().ReviewId;
            var reviewIntern = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            var monthReviewIntern = reviewIntern.Month;
            var yearReviewIntern = reviewIntern.Year;

            var internshipIds = reviewDetails.Select(rd => rd.InternshipId).ToList();

            var reviewerIds = reviewDetails.Select(rd => rd.ReviewerId.Value).Distinct().ToList();

            var userIds = internshipIds.Concat(reviewerIds).Distinct().ToList();

            var users = await WorkScope.GetAll<User>()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var userDict = users.ToDictionary(u => u.Id);

            var reviewersDto = reviewDetails
                .GroupBy(rd => rd.ReviewerId.Value)
                .Select(g => new ReviewerAssignmentDto
                {
                    ReviewerId = g.Key,
                    ReviewerUserName = userDict[g.Key].UserName,
                    ReviewerFullName = userDict[g.Key].FullName,
                    ReviewerEmail = userDict[g.Key].EmailAddress,
                    Interns = g.Select(rd => new InternInfoForReviewDto
                    {
                        InternId = rd.Id,
                        InternFullName = userDict[rd.InternshipId].FullName,
                        InternUserName = userDict[rd.InternshipId].UserName,
                        InternCurrentLevel = rd.CurrentLevel,
                        InternNewLevel = rd.NewLevel,
                        ReviewStatus = rd.Status
                    }).ToList()
                })
                .ToList();

            return new ReviewDataDto
            {
                MonthReviewIntern = reviewIntern.Month,
                YearReviewIntern = reviewIntern.Year,
                Reviewers = reviewersDto
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_CreatePMNote)]
        [HttpPost]
        public async System.Threading.Tasks.Task CreatePMNote(CreatePrivateNoteDto input)
        {
            if (input.Status == ReviewInternStatus.Reviewed)
            {
                var detail = await WorkScope.GetAsync<ReviewDetail>(input.ReviewDetailId);
                if (detail.Status == ReviewInternStatus.Reviewed)
                {
                    detail.Status = input.Status;
                    if (input.PrivateNote != null)
                    {
                        var reviewInternComment = new ReviewInternPrivateNote
                        {
                            ReviewDetailId = input.ReviewDetailId,
                            NoteByUserId = AbpSession.UserId.Value,
                            PrivateNote = input.PrivateNote.Trim(),
                            ReviewInternNoteType = ReviewInternNoteType.PmNote
                        };
                        await WorkScope.InsertAsync(reviewInternComment);
                    }
                    await WorkScope.UpdateAsync(detail);
                }
                else
                {
                    throw new UserFriendlyException("Review này chưa được PM review hoặc đã review xong");
                }
            }
            else
            {
                throw new UserFriendlyException("Trạng thái chỉ được là HR Approved hoặc Reopen.");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_CreateInterviewNote)]
        [HttpPost]
        public async System.Threading.Tasks.Task CreateInterviewNote(CreatePrivateNoteDto input)
        {
            if (input.Status == ReviewInternStatus.Reviewed)
            {
                var detail = await WorkScope.GetAsync<ReviewDetail>(input.ReviewDetailId);
                if (detail.Status == ReviewInternStatus.Reviewed)
                {
                    detail.Status = input.Status;
                    if (input.PrivateNote != null)
                    {
                        var reviewInternComment = new ReviewInternPrivateNote
                        {
                            ReviewDetailId = input.ReviewDetailId,
                            NoteByUserId = AbpSession.UserId.Value,
                            PrivateNote = input.PrivateNote.Trim(),
                            ReviewInternNoteType = ReviewInternNoteType.InterviewerNote
                        };
                        await WorkScope.InsertAsync(reviewInternComment);
                    }
                    await WorkScope.UpdateAsync(detail);
                }
                else
                {
                    throw new UserFriendlyException("This review has not yet been PM Reviewed");
                }
            }
            else
            {
                throw new UserFriendlyException("Status can only be Reviewed or Rejected.");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_AcceptPMReviewForAllIntern)]
        [HttpPost]
        public async Task HeadPmVerifyOrRejectAll(List<HeadPmVerifyDto> input)
        {
            var listReviewDetail = new List<ReviewDetail>();
            var rejectedIds = new List<long>();

            var listId = new List<long>();
            foreach (var inputItem in input)
            {
                var detail = await WorkScope.GetAsync<ReviewDetail>(inputItem.ReviewDetailId);

                if (detail.Status != ReviewInternStatus.PmReviewed)
                {
                    throw new UserFriendlyException("Review này chưa được PM review hoặc đã review xong");
                }

                if (inputItem.Status == ReviewInternStatus.Rejected)
                {
                    rejectedIds.Add(detail.Id);
                }

                detail.Status = inputItem.Status;
                listReviewDetail.Add(detail);
                if (inputItem.Status == ReviewInternStatus.Reviewed)
                {
                    listId.Add(detail.Id);
                }

            }

            if (rejectedIds.Any())
            {
                await SendMailWhenRejected(rejectedIds, ReviewDetailRejector.HeadPM);
                await SendDirectMessageWhenRejected(rejectedIds, ReviewDetailRejector.HeadPM);
            }

            foreach (var rejectedId in rejectedIds)
            {
                var detail = listReviewDetail.FirstOrDefault(x => x.Id == rejectedId);
                if (detail != null)
                {
                    detail.NewLevel = detail.CurrentLevel;
                }
            }
            await WorkScope.UpdateRangeAsync(listReviewDetail);
            if (listId.Any())
            {
                await CheckSendMailToPresident(listReviewDetail.FirstOrDefault().ReviewId, listId);
            }
            Logger.Info("send mail to GD ");
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.ReviewIntern_ReviewDetail_ViewAll)]
        [HttpGet]
        public async Task<List<InternshipMaxLevelMonthsDto>> CountMonthLevelMax()
        {
            long userId = AbpSession.UserId.Value;
            var listInternshipMaxLevelMonths = await WorkScope.GetAll<ReviewDetail>()
                        .Where(x => x.IsDeleted == false && x.NewLevel != null && x.NewLevel.Value < UserLevel.FresherMinus && x.Status != ReviewInternStatus.Draft)
                        .GroupBy(x => x.InternshipId)
                        .Select(gp => new InternshipMaxLevelMonthsDto
                        {
                            internshipId = gp.Key,
                            maxLevel = gp.Max(y => y.NewLevel).Value,
                            countMonthLevelMax = gp.Count(rd => rd.NewLevel == gp.Max(y => y.NewLevel))
                        })
                        .ToListAsync();
            return listInternshipMaxLevelMonths;
        }

        public async Task CheckSendMailToPresident(long reviewId, List<long> listId)
        {
            bool check = GetReviewInternStatusSummary(reviewId, ReviewInternStatus.Reviewed, listId).hasAllSameStatus;
            if (!check) return;

            string presidentEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentEmail);
            int dateSendMailToPresident = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentReviewInternOnDate));
            await SendMailToNotifyTransition(presidentEmail, ReviewInternStatus.Reviewed, reviewId, dateSendMailToPresident);
            await SendDirectMessageToNotifyTransition(presidentEmail, ReviewInternStatus.Reviewed, reviewId, dateSendMailToPresident);
        }

        public (bool hasAllSameStatus, int totalPendingInterns) GetReviewInternStatusSummary(long reviewId, ReviewInternStatus status, List<long> listId = null)
        {
            var query = WorkScope.GetAll<ReviewDetail>()
                .Where(x => x.ReviewId == reviewId)
                .WhereIf(listId != null && listId.Any(), x => !listId.Contains(x.Id));
            var result = query.GroupBy(x => x.Status)
                      .Select(g => new
                      {
                          Status = g.Key,
                          Count = g.Count()
                      })
                      .ToList();

            var totalPendingInterns = result.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
            bool hasAllSameStatus = false;
            if (status == ReviewInternStatus.Reviewed)
            {
                hasAllSameStatus = result.Count == 0 || (result.Count == 1 && result.First().Status == status);
            }
            else if (status == ReviewInternStatus.PmReviewed)
            {
                hasAllSameStatus = result.Count == 0 || result.All(g => g.Status == ReviewInternStatus.Reviewed || g.Status == ReviewInternStatus.PmReviewed);
            }
            return (hasAllSameStatus, totalPendingInterns);
        }

        public async Task SendMailToNotifyTransition(string email, ReviewInternStatus status, long reviewId, int date)
        {
            string name = _userServices.GetUserByEmail(email).Name;
            ReviewIntern reviewIntern = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            int monthReviewIntern = reviewIntern.Month;
            int yearReviewIntern = reviewIntern.Year;
            var dateNow = DateTimeUtils.GetNow();
            var deadlineDate = dateNow.AddDays(2);
            StringBuilder content = new StringBuilder("");
            Dictionary<ReviewInternStatus, string> statusDictionary = CommonUtils.ReviewInternStatusString();
            string statusAchieved, statusExpected;
            try
            {
                string reviewer = status == ReviewInternStatus.Reviewed ? "Head PM" : "PM";
                if (status == ReviewInternStatus.Reviewed)
                {
                    statusAchieved = statusDictionary[ReviewInternStatus.Reviewed];
                    statusExpected = statusDictionary[ReviewInternStatus.Approved];
                }
                else
                {
                    statusAchieved = statusDictionary[ReviewInternStatus.PmReviewed];
                    statusExpected = statusDictionary[ReviewInternStatus.Reviewed];
                }
                content.Append($"Giai đoạn <span style='font-weight: 600'> {reviewer} review </span> cho đợt đánh giá intern tháng <span style='font-weight: 600'> {reviewIntern.Month}/{reviewIntern.Year} </span> đã hoàn tất.");
                content.Append("<br>");
                content.Append($"Các bản ghi đã được chuyển sang trạng thái <span style='font-weight: 600'> {statusAchieved} </span> trên hệ thống <span style='font-weight: 600'> Timesheet. </span>");
                content.Append("<br>");
                content.Append($"Vui lòng kiểm tra và hoàn tất việc đánh giá trước ngày <span style='font-weight: 600'> {deadlineDate:dd/MM/yyyy}. </span>");
                content.Append("<br>");
                var emailSubject = $"[NCC] [Review Intern {monthReviewIntern}/{yearReviewIntern}] Thông báo yêu cầu đánh giá thực tập sinh";
                var targetEmails = new List<string> { email };
                targetEmails.AddRange(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHrEmail)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(mail => mail.Trim())
                    .ToList());

                await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                {
                    TargetEmails = targetEmails,
                    Body = content.ToString(),
                    Subject = emailSubject
                }, BackgroundJobPriority.High
                );
            }
            catch (Exception e)
            {
                Logger.Error("SendEmails() error=>" + e.Message);
            }
        }

        public async Task SendDirectMessageToNotifyTransition(string email, ReviewInternStatus status, long reviewId, int date)
        {
            var user = _userServices.GetUserByEmail(email);
            ReviewIntern reviewIntern = await WorkScope.GetAsync<ReviewIntern>(reviewId);
            int monthReviewIntern = reviewIntern.Month;
            int yearReviewIntern = reviewIntern.Year;
            var dateNow = DateTimeUtils.GetNow();
            var deadlineDate = dateNow.AddDays(2);
            Dictionary<ReviewInternStatus, string> statusDictionary = CommonUtils.ReviewInternStatusString();
            string statusAchieved, statusExpected;
            try
            {
                string headPmMail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);
                var headPM = _userServices.GetUserByEmail(headPmMail);
                string nameHeadPm = headPM.UserName;
                string reviewer = status == ReviewInternStatus.Reviewed ? "Head PM" : "PM";

                StringBuilder content = new StringBuilder("");
                content.AppendLine($"**[NCC] [Review Intern {monthReviewIntern}/{yearReviewIntern}] Thông báo đánh giá thực tập sinh**");
                content.AppendLine();
                if (status == ReviewInternStatus.Reviewed)
                {
                    statusAchieved = statusDictionary[ReviewInternStatus.Reviewed];
                    statusExpected = statusDictionary[ReviewInternStatus.Approved];
                }
                else
                {
                    statusAchieved = statusDictionary[ReviewInternStatus.PmReviewed];
                    statusExpected = statusDictionary[ReviewInternStatus.Reviewed];
                }
                content.AppendLine($"Giai đoạn**{reviewer} review**cho đợt đánh giá intern tháng**{monthReviewIntern}/{yearReviewIntern}**đã hoàn tất");
                content.AppendLine($"Các bản ghi đã được chuyển sang trạng thái**{statusAchieved}**trên hệ thống**Timesheet**");
                content.AppendLine($"Vui lòng kiểm tra và hoàn tất việc đánh giá trước ngày**{deadlineDate:dd/MM/yyyy}**");

                _komuService.SendSimpleNotificationToUser(content.ToString(), user.UserName);
            }
            catch (Exception e)
            {
                Logger.Error("SendDirectMessage() error=>" + e.Message);
            }
        }

        public async Task SendMailForReReview(string headPmEmail, long reviewerId, long internId, long reviewId, int date)
        {
            try
            {
                var headPmUser = _userServices.GetUserByEmail(headPmEmail);
                var reviewerUser = await WorkScope.GetAsync<User>(reviewerId);
                var internUser = await WorkScope.GetAsync<User>(internId);
                var reviewIntern = await WorkScope.GetAsync<ReviewIntern>(reviewId);
                var dateNow = DateTimeUtils.GetNow();

                StringBuilder content = new StringBuilder();
                content.Append($"<span style='font-weight: 600'>Kính gửi anh {headPmUser.FullName}</span>,<br>");
                content.Append($"Reviewer <span style='font-weight: 600'> {reviewerUser.FullName} </span> đã đánh giá lại thực tập sinh <span style='font-weight: 600'> {internUser.FullName} </span> cho đợt đánh giá intern tháng {reviewIntern.Month}/{reviewIntern.Year} trên Timesheet.");
                content.Append("<br>");
                content.Append($"Kính mong anh xem xét và thực hiện chuyển trạng thái sang  <span style='font-weight: 600'> reviewed </span> trước  <span style='font-weight: 600'> ngày {date + 1}/{dateNow.Month}/{dateNow.Year}. </span>");
                content.Append("<br>");
                content.Append("Trân trọng cảm ơn anh!");

                var emailSubject = $"[NCC] [Review Intern {reviewIntern.Month}/{reviewIntern.Year}] Thông báo reviewer đã đánh giá lại thực tập sinh";

                var targetEmails = new List<string> { headPmEmail };
                targetEmails.AddRange(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHrEmail)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                {
                    TargetEmails = targetEmails,
                    Body = content.ToString(),
                    Subject = emailSubject
                }, BackgroundJobPriority.High);
            }
            catch (Exception e)
            {
                Logger.Error("SendMailForReReview() error => " + e.Message);
            }
        }

        public async Task SendDirectMessageForReReview(string headPmEmail, long reviewerId, long internId, long reviewId, int date)
        {
            try
            {
                var headPmUser = _userServices.GetUserByEmail(headPmEmail);
                var reviewerUser = await WorkScope.GetAsync<User>(reviewerId);
                var internUser = await WorkScope.GetAsync<User>(internId);
                var reviewIntern = await WorkScope.GetAsync<ReviewIntern>(reviewId);
                var dateNow = DateTimeUtils.GetNow();

                StringBuilder content = new StringBuilder();
                content.AppendLine($"Kính gửi anh**{headPmUser.UserName}**");
                content.AppendLine($"Reviewer**{reviewerUser.UserName}**đã đánh giá lại thực tập sinh**{internUser.UserName}**cho đợt đánh giá intern tháng**{reviewIntern.Month}/{reviewIntern.Year}**trên Timesheet.");
                content.AppendLine($"Kính mong anh xem xét và thực hiện chuyển trạng thái sang**Reviewed**trước ngày**{date + 1}/{dateNow.Month}/{dateNow.Year}**");
                content.AppendLine("Trân trọng cảm ơn anh!");

                _komuService.SendSimpleNotificationToUser(content.ToString(), headPmUser.UserName);
            }
            catch (Exception e)
            {
                Logger.Error("SendDirectMessageForReReview() error => " + e.Message);
            }
        }
    }
}