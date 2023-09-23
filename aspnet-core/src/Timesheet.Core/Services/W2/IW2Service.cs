using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Services.W2.Dto;

namespace Timesheet.Services.W2
{
    public interface IW2Service
    {
        WorkFromHomeRequestDto GetWfhRequest(string email, string date);
    }
}
