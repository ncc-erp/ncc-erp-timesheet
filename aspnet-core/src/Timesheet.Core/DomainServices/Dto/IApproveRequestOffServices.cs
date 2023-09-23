using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public interface IApproveRequestOffServices : IDomainService
    {
        List<RequestAddDto> GetListPmNotApproveRequestOff();

        void NotifyRequestOffPending(List<RequestAddDto> listRequestOff, string notifyToChannels);
        void SendMessageToUserRequestOffPending(List<RequestAddDto> listRequestOff);
    }
}
