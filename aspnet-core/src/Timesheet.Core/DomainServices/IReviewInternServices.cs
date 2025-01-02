using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.DomainServices
{
    public interface IReviewInternServices : IDomainService
    {
        long LastIdReviewIntern();
        List<NotifyReviewInternDto> GetListPmNotReview(long reviewId);
    }
}
