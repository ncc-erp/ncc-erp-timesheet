using Abp.Configuration;
using Abp.Dependency;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using System.Threading.Tasks;
using static Ncc.Entities.Enum.StatusEnum;
using System.Text;

namespace Timesheet.DomainServices
{
    public class ReviewInternServices : BaseDomainService, IReviewInternServices, ITransientDependency
    {
        public long LastIdReviewIntern()
        {
            return WorkScope.GetAll<ReviewIntern>().
                Where(s => s.IsActive).
                OrderByDescending(s => s.Id).
                Select(s => s.Id).FirstOrDefault();
        }

        public List<NotifyReviewInternDto> GetListPmNotReview(long reviewId)
        {
            var PMsNotReview = WorkScope.GetAll<ReviewDetail>().
                                     Include(s => s.Reviewer).
                                     Where(s => s.ReviewId == reviewId).
                                     Where(s => s.Status == ReviewInternStatus.Draft || s.Status == ReviewInternStatus.Rejected)
                                    .GroupBy(ts => new
                                    {
                                        ts.Reviewer.EmailAddress,
                                        ts.Reviewer.KomuUserId,
                                    })
                                    .Select(s => new NotifyReviewInternDto
                                    {
                                        EmailAddress = s.Key.EmailAddress,
                                        KomuUserId = s.Key.KomuUserId,
                                        InterShips = s.Select(x => new NotifyUserInfoDto
                                        {
                                            FullName = x.InterShip.FullName,
                                            BranchDisplayName = x.InterShip.Branch.DisplayName,
                                            Level = x.InterShip.Level,
                                        }).ToList()
                                    }).ToList();
            return PMsNotReview;
        }
    }
}
