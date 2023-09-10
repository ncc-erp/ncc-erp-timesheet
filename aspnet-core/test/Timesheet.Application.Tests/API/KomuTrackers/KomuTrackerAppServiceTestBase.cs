using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.KomuTrackers;
using Timesheet.APIs.KomuTrackers.Dto;
using Timesheet.Paging;

namespace Timesheet.Application.Tests.API.KomuTrackers
{
    public class KomuTrackerAppServiceTestBase :  TimesheetApplicationTestBase
    {
        public KomuTrackerAppService InstanceKomuTrackerAppService() 
        {
            var workScope = Resolve<IWorkScope>();
            var komuTrackerAppService = new KomuTrackerAppService(workScope);
            komuTrackerAppService.ObjectMapper = Resolve<IObjectMapper>();
            komuTrackerAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();

            return komuTrackerAppService;
        }

        public GridParam GridParam() 
        {
            return new GridParam
            {
                MaxResultCount = 10,
                SkipCount = 0
            };
        }

        private KomuTrackerDto KomuTrackerDto()
        {
            return new KomuTrackerDto
            {
                EmailAddress = "a.tranvan@ncc.asia",
                ComputerName = "A'PC",
                WorkingMinute = 450,
            };
        }

        public KomuTrackerDto KomuTrackerDto1()
        {
            return new KomuTrackerDto
            {
                EmailAddress = "toai.nguyencong@ncc.asia",
                ComputerName = "A'PC",
                WorkingMinute = 4500,
            };
        }

        public SaveKomuTrackerDto SaveKomuTrackerDto()
        {
            var komuTrackerDto = KomuTrackerDto();
            return new SaveKomuTrackerDto
            {
                DateAt = new DateTime(2022, 1, 1),
                ListKomuTracker = new List<KomuTrackerDto> { komuTrackerDto }
            };
        }

        public SaveKomuTrackerDto KomuTrackerUpdateDto()
        {
            var komuTrackerDto = KomuTrackerDto1();
            return new SaveKomuTrackerDto
            {
                DateAt = new DateTime(2022, 1, 1),
                ListKomuTracker = new List<KomuTrackerDto> { komuTrackerDto }
            };
        }

    }
}
