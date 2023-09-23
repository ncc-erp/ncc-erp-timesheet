using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Services.Komu.Dto;

namespace Timesheet.Services.Komu
{
    public interface IKomuService
    {
        Task NotifyToChannel(string komuMessage, string channelId);
        Task<ulong?> GetKomuUserId(string userName);
    }
}
 