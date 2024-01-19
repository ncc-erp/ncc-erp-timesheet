using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Uitls
{
    public class RequestUtils
    {
        public static IEnumerable<RequestTypeDto> RequestsType()
        {
            return new List<RequestTypeDto>()
            {
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Off,      DateType = DayType.Fullday,     AbsenceTime = null},                    Type = EnumRequest.OffFull},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Off,      DateType = DayType.Morning,     AbsenceTime = null },                   Type = EnumRequest.OffMorning},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Off,      DateType = DayType.Afternoon,   AbsenceTime = null },                   Type = EnumRequest.OffAfternoon},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Remote,   DateType = DayType.Fullday,     AbsenceTime = null },                   Type = EnumRequest.RemoteFull},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Remote,   DateType = DayType.Morning,     AbsenceTime = null },                   Type = EnumRequest.RemoteMorning},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Remote,   DateType = DayType.Afternoon,   AbsenceTime = null },                   Type = EnumRequest.RemoteAfternoon},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Off,      DateType = DayType.Custom,      AbsenceTime = OnDayType.DiMuon },   Type = EnumRequest.Tardiness},
                new RequestTypeDto{ MyRequest = new MyRequest{ Type = RequestType.Off,      DateType = DayType.Custom,      AbsenceTime = OnDayType.VeSom },     Type = EnumRequest.EarlyLeave},
            };
        }
        public static Dictionary<IEnumerable<EnumRequest>, IEnumerable<EnumRequest>> DictMayRequest()
        {
            return new Dictionary<IEnumerable<EnumRequest>, IEnumerable<EnumRequest>>()
            {
                { new List<EnumRequest>() { EnumRequest.OffFull }
                , new List<EnumRequest>()},    

                { new List<EnumRequest>() { EnumRequest.OffMorning }
                , new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.Tardiness, EnumRequest.EarlyLeave}},    

                { new List<EnumRequest>() { EnumRequest.OffAfternoon }
                , new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.Tardiness, EnumRequest.EarlyLeave}},   
                
                { new List<EnumRequest>() { EnumRequest.RemoteFull }
                , new List<EnumRequest>() { EnumRequest.Tardiness, EnumRequest.EarlyLeave}},    

                { new List<EnumRequest>() { EnumRequest.RemoteMorning }
                , new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.Tardiness, EnumRequest.EarlyLeave}},  

                { new List<EnumRequest>() { EnumRequest.RemoteAfternoon }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.Tardiness, EnumRequest.EarlyLeave}},  

                { new List<EnumRequest>() { EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.OffAfternoon, EnumRequest.RemoteFull, EnumRequest.RemoteMorning, EnumRequest.RemoteAfternoon, EnumRequest.EarlyLeave} },   

                { new List<EnumRequest>() { EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.OffAfternoon, EnumRequest.RemoteFull, EnumRequest.RemoteMorning, EnumRequest.RemoteAfternoon, EnumRequest.Tardiness} },    


                { new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.RemoteAfternoon }
                , new List<EnumRequest>() { EnumRequest.Tardiness, EnumRequest.EarlyLeave}},    

                { new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.EarlyLeave}},   

                { new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.Tardiness}},   

                { new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.RemoteMorning }
                , new List<EnumRequest>() { EnumRequest.Tardiness, EnumRequest.EarlyLeave}},    

                { new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.EarlyLeave}},   

                { new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.Tardiness}},    

                { new List<EnumRequest>() { EnumRequest.RemoteFull, EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.EarlyLeave}},    

                { new List<EnumRequest>() { EnumRequest.RemoteFull, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.Tardiness}},   

                { new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.EarlyLeave}},   

                { new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.Tardiness}},  

                { new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.Tardiness }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.EarlyLeave}},   

                { new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.Tardiness}},   

                { new List<EnumRequest>() { EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                , new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.OffAfternoon, EnumRequest.RemoteMorning, EnumRequest.RemoteAfternoon, EnumRequest.RemoteFull}},  
                

                //{ new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.RemoteAfternoon, EnumRequest.Tardiness }
                //, new List<EnumRequest>() { EnumRequest.EarlyLeave}},   

                //{ new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.RemoteAfternoon, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.Tardiness}},   

                //{ new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.RemoteAfternoon}},    

                //{ new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.RemoteMorning, EnumRequest.Tardiness }
                //, new List<EnumRequest>() { EnumRequest.EarlyLeave}},    

                //{ new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.RemoteMorning, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.Tardiness}},    

                //{ new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.RemoteMorning}},    

                //{ new List<EnumRequest>() { EnumRequest.RemoteFull, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() {  }},    

                //{ new List<EnumRequest>() { EnumRequest.RemoteMorning, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.OffAfternoon}},   

                //{ new List<EnumRequest>() { EnumRequest.RemoteAfternoon, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { EnumRequest.OffMorning}},    


                //{ new List<EnumRequest>() { EnumRequest.OffMorning, EnumRequest.RemoteAfternoon, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { }},    

                //{ new List<EnumRequest>() { EnumRequest.OffAfternoon, EnumRequest.RemoteMorning, EnumRequest.Tardiness, EnumRequest.EarlyLeave }
                //, new List<EnumRequest>() { }},   
            };

        }

        public static IEnumerable<EnumRequest> ConvertRequestsToListInt(IEnumerable<MyRequest> requests)
        {
            return requests.Select(s => RequestsType().Where(d => d.MyRequest == s).Select(d => d.Type).FirstOrDefault()).OrderBy(s => s).ToList();
        }
        public static EnumRequest ConvertRequestToInt(MyRequest request)
        {
            return RequestsType().Where(d => d.MyRequest == request).Select(d => d.Type).FirstOrDefault();
        }

        public static bool checkValidate(IEnumerable<MyRequest> dbRequests, MyRequest newRequest)
        {
            var key = ConvertRequestsToListInt(dbRequests);
            var value = ConvertRequestToInt(newRequest);

            var mayRequest = DictMayRequest().Where(s => Enumerable.SequenceEqual(s.Key, key)).Select(s => s.Value).FirstOrDefault();
            
            return mayRequest != default && mayRequest.Contains(value);
        }
    }
    public class MyRequest
    {
        public RequestType Type { get; set; }  // kieu nghi la off, remote
        //public RequestDetail Detail { get; set; }
        public DayType DateType { get; set; }            // full sang chieu custom
        public OnDayType? AbsenceTime { get; set; }   // dau, giua, cuoi h

        public static bool operator == (MyRequest a, MyRequest b)
        {
            if (a.Type == b.Type && a.DateType == b.DateType && a.AbsenceTime == b.AbsenceTime)
                return true;
            return false;
        }
        public static bool operator != (MyRequest a, MyRequest b)
        {
            if (a.Type != b.Type || a.DateType != b.DateType || a.AbsenceTime != b.AbsenceTime)
                return true;
            return false;
        }

    }

    //public class RequestDetail
    //{
    //    public DayType DateType { get; set; }            // full sang chieu custom
    //    public OnDayType? AbsenceTime { get; set; }   // dau, giua, cuoi h
    //}
    public class RequestTypeDto
    {
        public MyRequest MyRequest { get; set; }
        public EnumRequest Type { get; set; }
    }
}
