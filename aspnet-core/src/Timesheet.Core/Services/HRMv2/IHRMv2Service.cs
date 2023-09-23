using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Services.HRMv2.Dto;
using Timesheet.Services.Project.Dto;

namespace Timesheet.Services.HRMv2
{
    public interface IHRMv2Service
    {
        void UpdateAvatarToHrm(UpdateAvatarDto input);
        Task<string> ComplainPayslipMail(InputComplainPayslipMail input);
        Task<string> ConfirmPayslipMail(InputConfirmPayslipMail input);
    }
}
