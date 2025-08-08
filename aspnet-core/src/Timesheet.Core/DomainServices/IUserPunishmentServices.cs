using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Project.Dto;

namespace Timesheet.DomainServices
{
    public interface IUserPunishmentServices : IDomainService
    {
        //Task<List<UserPunishments>> AddUserPunishmentByDay(DateTime selectedDate);
        Task<List<PMReportItemDto>> ApplyPMReportPunishmentsAsync();
    }
}
