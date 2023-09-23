using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetAbsenceDayRequestDto : Entity<long>
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string DayOffName { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public Ncc.Entities.Enum.StatusEnum.Branch? Branch { get; set; }
        public Usertype? Type { get; set; } // Staff, Intern, CTV
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public UserLevel? Level { get; set; }
        public Sex? Sex { get; set; }
        public IEnumerable<DetailDto> Details { get; set; }
    }

    public class DetailDto : Entity<long>
    {
        public string DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }
        public RequestType Type { get; set; }
    }
}
