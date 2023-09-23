using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.DomainServices.Dto;

namespace Timesheet.DomainServices
{
    public interface ISendMessageToUserServices : IDomainService
    {
        List<SendMessageToPunishUserDto> GetListUserPunish();
    }
}
