using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.APIs.InternInfo.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.InternInfo
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_InternsInfo)]
    public class InternsInfoAppService : AppServiceBase
    {
        public InternsInfoAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_InternsInfo_View)]
        [HttpPost]
        public InternsInfoOutputDto GetAll(InputInternInfoDto input)
        {
            var output = new InternsInfoOutputDto();

            var userIds = GetFilterUserIds(input);

            if (userIds.Count() == 0)
                return null;

            var qReviewDetail = WorkScope.GetAll<ReviewDetail>()
                                           .Select(s => new ReviewDetailFilterDto
                                           {
                                               UserId = s.InterShip.Id,
                                               Month = s.Review.Month,
                                               Year = s.Review.Year,
                                               NextLevel = s.NewLevel,
                                               RateStar = s.RateStar.Value,
                                               ReviewerName = s.Reviewer.FullName,
                                               Note = s.Note
                                           })
                                           .Where(s => userIds.Contains(s.UserId));

            var dictReviewDetailByUserId = qReviewDetail.Select(s => new
            {
                MonthInfo = new MonthInfo
                {
                    Month = s.Month,
                    NewLevel = s.NextLevel,
                    RateStar = s.RateStar,
                    ReviewerName = s.ReviewerName,
                    Year = s.Year,
                    Note = s.Note
                },
                s.UserId,
            })
            .GroupBy(s => s.UserId)
            .ToDictionary(s => s.Key, s => s.Select(m => m.MonthInfo).ToList());

            var qListInternInfo = WorkScope.GetAll<User>().Include(s => s.Manager)
                                       .Where(s => userIds.Contains(s.Id))
                                       .Select(s => new InternsInfoDto
                                       {
                                           MyInfo = new InternsInfoUserInfo
                                           {
                                               Id = s.Id,
                                               AvatarPath = s.AvatarPath,
                                               Branch = s.BranchOld,
                                               BranchDisplayName = s.Branch.DisplayName,
                                               BranchColor = s.Branch.Color,
                                               EmailAddress = s.EmailAddress,
                                               FullName = s.FullName,
                                               Level = s.Level,
                                               Type = s.Type,
                                           },
                                           BasicTrannerFullName = s.Manager.FullName,
                                           ReviewDetails = dictReviewDetailByUserId.ContainsKey(s.Id) ? dictReviewDetailByUserId[s.Id] : null,
                                           StartDate = s.StartDateAt,
                                           EndDate = !s.IsActive ? s.EndDateAt : null,
                                           BeginLevel = s.BeginLevel,
                                       });

            var result = qListInternInfo.GetGridResult(qListInternInfo, input);
            var listInternInfo = result.Result.Items.ToList();

            userIds = listInternInfo.Select(s => s.MyInfo.Id).ToList();

            var lastReviewMonth = !qReviewDetail.IsEmpty() ? qReviewDetail.Where(s => userIds.Contains(s.UserId)).OrderBy(s => s.GetDateAt).LastOrDefault() : default;
            var lastOutMonth = listInternInfo.Where(s => s.EndDate.HasValue).OrderBy(s => s.EndDate).LastOrDefault();
            var userWoringDate = listInternInfo.Where(s => s.StartDate.HasValue).OrderBy(s => s.StartDate);

            var firstWoringDate = userWoringDate.FirstOrDefault().StartDate.Value;
            var lastWoringDate = userWoringDate.LastOrDefault().StartDate.Value;
            var lastReviewDate = lastReviewMonth != default ? lastReviewMonth.GetDateAt : lastWoringDate;
            var lastOutDate = lastOutMonth != default ? lastOutMonth.EndDate.Value : input.EndDate;

            var startDate = GetStartDate(firstWoringDate);
            var endDate = GetEndDate(lastReviewDate, lastOutDate, lastWoringDate, input.DateFilterType);

            output.ListMonth = new List<string>();

            var listMonth = new List<DateTime>();

            var date = DateTimeUtils.FirstDayOfMonth(startDate);

            while (date <= endDate)
            {
                output.ListMonth.Add(date.ToString("MM-yyyy"));
                listMonth.Add(date);
                date = date.AddMonths(1);
            }

            foreach (var intern in listInternInfo)
            {
                if (intern.ReviewDetails == null)
                {
                    intern.ReviewDetails = new List<MonthInfo>();
                }
                foreach (var dateAt in listMonth)
                {
                    if (!intern.ReviewDetails.Any(s => s.Year == dateAt.Year && s.Month == dateAt.Month))
                    {
                        intern.ReviewDetails.Add(new MonthInfo
                        {
                            Year = dateAt.Year,
                            Month = dateAt.Month,
                        });
                    }
                }

                intern.ReviewDetails = intern.ReviewDetails.OrderBy(s => s.Year).ThenBy(s => s.Month).ToList();
                intern.ReviewDetails.ForEach(reviewDetails =>
                {
                    int month = reviewDetails.Month;
                    int year = reviewDetails.Year;

                    bool isStart = IsTheSameMonthYear(intern.StartDate, month, year);
                    bool isStop = IsTheSameMonthYear(intern.EndDate, month, year);

                    reviewDetails.IsStart = isStart;
                    reviewDetails.IsStop = isStop;

                    reviewDetails.Display = CommonUtils.GetReviewDetailDisplay(isStart, isStop, intern.BeginLevel, reviewDetails.NewLevel, intern.MyInfo.Level);
                });
            }

            result.Result.Items = listInternInfo;

            output.ListInternInfo = result.Result;

            return output;
        }

        private bool IsTheSameMonthYear(DateTime? date, int month, int years)
        {
            return date.HasValue && month == date.Value.Month && years == date.Value.Year;
        }

        private List<long> GetFilterUserIds(InputInternInfoDto input)
        {
            if (input.StartDate > input.EndDate)
            {
                return new List<long>();
            }
            var basicTrainerIds = input.BasicTrainerIds.EmptyIfNull().ToList();
            var branchIds = input.BranchIds.EmptyIfNull().ToList();

            var qFilterUser = WorkScope.GetAll<User>()
                                   .Select(s => new
                                   {
                                       BeginLevel = s.BeginLevel,
                                       EmailAddress = s.EmailAddress.ToLower(),
                                       BasicTrainerId = s.ManagerId,
                                       BranchId = s.BranchId,
                                       StartDateAt = s.StartDateAt,
                                       Id = s.Id,
                                       EndDateAt = !s.IsActive ? s.EndDateAt : null,
                                   })
                                   .Where(s => s.BeginLevel.HasValue && s.BeginLevel < UserLevel.FresherMinus);

            if (!input.SearchText.IsNullOrEmpty())
                qFilterUser = qFilterUser.Where(s => s.EmailAddress.Contains(input.SearchText.ToLower()));

            if(input.IsFilterBranch)
                qFilterUser = qFilterUser.Where(s => s.BranchId.HasValue && branchIds.Contains(s.BranchId.Value));

            if (input.IsFilterTrainer)
                qFilterUser = qFilterUser.Where(s => basicTrainerIds.Contains(s.BasicTrainerId.Value) || (!s.BasicTrainerId.HasValue && input.BasicTrainerIds.Contains(-1)));

            if (input.DateFilterType == DateFilterType.OnBoardDate)
            {
                qFilterUser = qFilterUser.Where(s => s.StartDateAt.HasValue && s.StartDateAt.Value >= input.StartDate.Date && s.StartDateAt.Value <= input.EndDate.Date);
            }
            else if (input.DateFilterType == DateFilterType.OutDate)
            {
                qFilterUser = qFilterUser.Where(s => s.EndDateAt.HasValue && s.EndDateAt.Value >= input.StartDate.Date && s.EndDateAt.Value <= input.EndDate.Date);
            }
            var userIds = qFilterUser.Select(s => s.Id).AsEnumerable();

            if (input.DateFilterType == DateFilterType.BeStaffDate)
            {
                var beStaffUserIds = WorkScope.GetAll<ReviewDetail>()
                                           .Select(s => new ReviewDetailFilterDto
                                           {
                                               UserId = s.InternshipId,
                                               Month = s.Review.Month,
                                               Year = s.Review.Year,
                                               NextLevel = s.NewLevel,
                                           })
                                           .Where(s => s.NextLevel.HasValue && s.NextLevel.Value >= UserLevel.FresherMinus)
                                           .Where(s => input.StartDate <= s.GetDateAt && input.EndDate >= s.GetDateAt)
                                           .Select(s => s.UserId)
                                           .AsEnumerable();
                userIds = userIds.Intersect(beStaffUserIds);
            }

            return userIds.ToList();
        }

        private DateTime GetStartDate(DateTime FirstStartWorkingDate)
        {
            return FirstStartWorkingDate;
        }

        private DateTime GetEndDate(DateTime LastReview, DateTime LastOutDate, DateTime LastWorkingDate, DateFilterType dateFilterType)
        {
            if (dateFilterType == DateFilterType.OutDate)
            {
                return LastOutDate;
            }
            return DateTimeUtils.Max(LastWorkingDate, LastReview);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_InternsInfo_View)]
        [HttpGet]
        public List<GetAllBasicTranerDto> GetAllBasicTraner()
        {
            return WorkScope.GetAll<User>()
                            .Include(s => s.Manager)
                            .Where(s => s.Manager != null)
                            .Select(s => s.Manager)
                            .Distinct()
                            .AsEnumerable()
                            .Select(s => new GetAllBasicTranerDto
                            {
                                EmailAddress = s.EmailAddress,
                                FullName = s.FullName,
                                Id = s.Id
                            })
                            .ToList();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_InternsInfo_View)]
        [HttpGet]
        public List<GetAllBranchDto> GetAllBranch()
        {
            return WorkScope.GetAll<Entities.Branch>()
                            .Select(s => new GetAllBranchDto
                            {
                                BranchId = s.Id,
                                BranchDisplayName = s.DisplayName,
                            })
                            .ToList();
        }
    }
}