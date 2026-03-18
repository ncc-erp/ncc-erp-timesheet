using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.DomainServices
{
    public interface IWhitelistSystemServices : IDomainService
    {
        Task<WhitelistSystem> Add(AddWhitelistTypeDto input);
        Task<WhitelistSystem> Update(UpdateWhitelistTypeDto input);
        List<WhitelistTypeDto> GetWhitelistTypes();
        Task<List<GetWhitelistSystemDto>> GetAll();
        Task<bool> Delete(long id);
    }
}
