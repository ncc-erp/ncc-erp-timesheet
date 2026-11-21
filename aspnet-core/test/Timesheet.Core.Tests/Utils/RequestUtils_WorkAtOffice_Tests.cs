using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Core.Tests.Utils
{
    public class RequestUtils_WorkAtOffice_Tests
    {
        [Fact]
        public void ListRequestType_Should_Include_All_WorkAtOffice_DayTypes()
        {
            var list = RequestUtils.RequestsType();

            list.ShouldContain(x =>
                x.MyRequest.Type == RequestType.WorkAtOffice &&
                x.MyRequest.DateType == DayType.Fullday &&
                x.MyRequest.AbsenceTime == null &&
                x.Type == EnumRequest.WorkAtOfficeFull);

            list.ShouldContain(x =>
                x.MyRequest.Type == RequestType.WorkAtOffice &&
                x.MyRequest.DateType == DayType.Morning &&
                x.MyRequest.AbsenceTime == null &&
                x.Type == EnumRequest.WorkAtOfficeMorning);

            list.ShouldContain(x =>
                x.MyRequest.Type == RequestType.WorkAtOffice &&
                x.MyRequest.DateType == DayType.Afternoon &&
                x.MyRequest.AbsenceTime == null &&
                x.Type == EnumRequest.WorkAtOfficeAfternoon);
        }

        [Fact]
        public void DictMayRequest_Should_Allow_Valid_WorkAtOffice_Combinations()
        {
            var dict = RequestUtils.DictMayRequest();

            IEnumerable<EnumRequest> GetAllowed(params EnumRequest[] existing)
            {
                var entry = dict.FirstOrDefault(kv => kv.Key.SequenceEqual(existing));
                return entry.Value ?? Enumerable.Empty<EnumRequest>();
            }

            // OffMorning allows WorkAtOfficeAfternoon, not WorkAtOfficeMorning or Full
            var afterOffMorning = GetAllowed(EnumRequest.OffMorning).ToList();
            afterOffMorning.ShouldContain(EnumRequest.WorkAtOfficeAfternoon);
            afterOffMorning.ShouldNotContain(EnumRequest.WorkAtOfficeMorning);
            afterOffMorning.ShouldNotContain(EnumRequest.WorkAtOfficeFull);

            // OffAfternoon allows WorkAtOfficeMorning, not WorkAtOfficeAfternoon or Full
            var afterOffAfternoon = GetAllowed(EnumRequest.OffAfternoon).ToList();
            afterOffAfternoon.ShouldContain(EnumRequest.WorkAtOfficeMorning);
            afterOffAfternoon.ShouldNotContain(EnumRequest.WorkAtOfficeAfternoon);
            afterOffAfternoon.ShouldNotContain(EnumRequest.WorkAtOfficeFull);

            // WorkAtOfficeFull only allows Tardiness and EarlyLeave
            var afterWorkAtOfficeFull = GetAllowed(EnumRequest.WorkAtOfficeFull).ToList();
            afterWorkAtOfficeFull.ShouldBe(new[] { EnumRequest.Tardiness, EnumRequest.EarlyLeave }, ignoreOrder: true);

            // WorkAtOfficeMorning allows OffAfternoon, EarlyLeave, Tardiness
            var afterWorkAtOfficeMorning = GetAllowed(EnumRequest.WorkAtOfficeMorning).ToList();
            afterWorkAtOfficeMorning.ShouldBe(new[] { EnumRequest.OffAfternoon, EnumRequest.EarlyLeave, EnumRequest.Tardiness }, ignoreOrder: true);

            // WorkAtOfficeAfternoon allows OffMorning, EarlyLeave, Tardiness
            var afterWorkAtOfficeAfternoon = GetAllowed(EnumRequest.WorkAtOfficeAfternoon).ToList();
            afterWorkAtOfficeAfternoon.ShouldBe(new[] { EnumRequest.OffMorning, EnumRequest.EarlyLeave, EnumRequest.Tardiness }, ignoreOrder: true);

            // Tardiness & EarlyLeave allow all WorkAtOffice variants
            var afterTardiness = GetAllowed(EnumRequest.Tardiness).ToList();
            afterTardiness.ShouldContain(EnumRequest.WorkAtOfficeFull);
            afterTardiness.ShouldContain(EnumRequest.WorkAtOfficeMorning);
            afterTardiness.ShouldContain(EnumRequest.WorkAtOfficeAfternoon);

            var afterEarlyLeave = GetAllowed(EnumRequest.EarlyLeave).ToList();
            afterEarlyLeave.ShouldContain(EnumRequest.WorkAtOfficeFull);
            afterEarlyLeave.ShouldContain(EnumRequest.WorkAtOfficeMorning);
            afterEarlyLeave.ShouldContain(EnumRequest.WorkAtOfficeAfternoon);
        }
    }
}
