using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using Timesheet.Entities;
using Ncc.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Timesheet.APIs.KomuTrackers.Dto
{
    [AutoMap(typeof(KomuTracker))]
    public class KomuTrackerDto
    {
        public string EmailAddress { get; set; }
        public string ComputerName { get; set; }
        public float WorkingMinute { get; set; }
    }

    public class SaveKomuTrackerDto
    {
        public DateTime DateAt { get; set; }
        public List<KomuTrackerDto> ListKomuTracker { get; set; }
    }
}
