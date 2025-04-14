using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Web.Http.Results;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Komu;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class ApproveRequestOffServices : DomainService, IApproveRequestOffServices, ITransientDependency
    {
        private readonly IRepository<AbsenceDayDetail, long> _absenceDayDetailRepository;
        private readonly IRepository<ProjectUser, long> _projectUserRepository;
        private readonly KomuService _komuService;
        private readonly IRepository<User, long> _userRepository;  // Inject User repository
        public ApproveRequestOffServices(IRepository<AbsenceDayDetail, long> absenceDayDetailRepository, IRepository<ProjectUser, long> projectUserRepository, KomuService komuService, IRepository<User, long> userRepository)
        {
            _absenceDayDetailRepository = absenceDayDetailRepository;
            _projectUserRepository = projectUserRepository;
            _komuService = komuService;
            _userRepository = userRepository;
        }


        private bool IsWithinWorkingHours(TimeSpan currentTime, User user)
        {
            // Giá trị mặc định
            TimeSpan defaultMorningStart = new TimeSpan(8, 30, 0);  // 8:30 AM
            TimeSpan defaultMorningEnd = new TimeSpan(12, 0, 0);    // 12:00 PM
            TimeSpan defaultAfternoonStart = new TimeSpan(13, 0, 0); // 1:00 PM
            TimeSpan defaultAfternoonEnd = new TimeSpan(17, 30, 0);  // 5:30 PM

            TimeSpan morningStart = string.IsNullOrEmpty(user?.MorningStartAt)
                ? defaultMorningStart
                : TimeSpan.Parse(user.MorningStartAt);

            TimeSpan morningEnd = string.IsNullOrEmpty(user?.MorningEndAt)
                ? defaultMorningEnd
                : TimeSpan.Parse(user.MorningEndAt);

            TimeSpan afternoonStart = string.IsNullOrEmpty(user?.AfternoonStartAt)  
                ? defaultAfternoonStart
                : TimeSpan.Parse(user.AfternoonStartAt);

            TimeSpan afternoonEnd = string.IsNullOrEmpty(user?.AfternoonEndAt)
                ? defaultAfternoonEnd
                : TimeSpan.Parse(user.AfternoonEndAt);

            // Kiểm tra currentTime có nằm trong khoảng giờ làm việc không
            return (currentTime >= morningStart && currentTime <= morningEnd) ||
                   (currentTime >= afternoonStart && currentTime <= afternoonEnd);
        }



        private bool IsWithinWorkingHours(TimeSpan currentTime, PmInfoRequestOffDto pm)
        {
            TimeSpan defaultMorningStart = new TimeSpan(8, 30, 0);   
            TimeSpan defaultMorningEnd = new TimeSpan(12, 0, 0);     
            TimeSpan defaultAfternoonStart = new TimeSpan(13, 0, 0); 
            TimeSpan defaultAfternoonEnd = new TimeSpan(17, 30, 0); 

            TimeSpan morningStart = string.IsNullOrEmpty(pm?.MorningStartAt)
                ? defaultMorningStart
                : TimeSpan.Parse(pm.MorningStartAt);

            TimeSpan morningEnd = string.IsNullOrEmpty(pm?.MorningEndAt)
                ? defaultMorningEnd
                : TimeSpan.Parse(pm.MorningEndAt);

            TimeSpan afternoonStart = string.IsNullOrEmpty(pm?.AfternoonStartAt)
                ? defaultAfternoonStart
                : TimeSpan.Parse(pm.AfternoonStartAt);

            TimeSpan afternoonEnd = string.IsNullOrEmpty(pm?.AfternoonEndAt)
                ? defaultAfternoonEnd
                : TimeSpan.Parse(pm.AfternoonEndAt);

            // Kiểm tra xem currentTime có nằm trong khoảng giờ làm việc không
            return (currentTime >= morningStart && currentTime <= morningEnd) ||
                   (currentTime >= afternoonStart && currentTime <= afternoonEnd);
        }




        public List<RequestAddDto> GetListPmNotApproveRequestOff()
        {
            double timePeriodWithPendingRequest = Double.Parse(SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveRequestOffNotifyTimePeriodWithPendingRequest));
            var dateNow = DateTimeUtils.GetNow();
            // lấy ra ngày thứ 30 trước Now hiện tại
            var startDate = dateNow.AddDays(timePeriodWithPendingRequest);

            var allRequestOff = _absenceDayDetailRepository.GetAll()
                .Where(s => s.Request.Status == RequestStatus.Pending)
                .Where(s => s.DateAt >= startDate.Date) // thay đổi business: lấy tất cả request đang pending
                .Select(s => new RequestAddDto
                {
                    DateAt = s.DateAt,
                    DateType = s.DateType,
                    FullName = s.Request.User.FullName,
                    RequestType = s.Request.Type,
                    UserId = s.Request.UserId,
                    UserName = s.Request.User.UserName,
                })
                .OrderBy(s => s.DateAt).ToList();

            return allRequestOff;
        }

        public void NotifyRequestOffPending(List<RequestAddDto> listRequestOff, string notifyToChannels)
        {
            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            var listInfo = listRequestOff.GroupBy(s => new { s.UserId, s.FullName })
                .ToDictionary(s => s.Key,
                s => s.Select(x => new RequestOffDto
                {
                    DateAt = x.DateAt,
                    DayType = x.DateType,
                    RequestType = x.RequestType
                }).ToList());

            List<NotifyApproveRequestOffDto> result = new List<NotifyApproveRequestOffDto>();

            var userIds = listInfo.Select(s => s.Key.UserId).Distinct().ToList();
            var currentTime = DateTime.Now.TimeOfDay;

            // Filter list project has request based on list userIds
            var allProjectUsers = _projectUserRepository.GetAll()
                .Where(s => !s.IsDeleted)
                .Where(s => s.Type != ProjectUserType.DeActive)
                .Where(s => s.Project.Status == ProjectStatus.Active);

            var projectIds = allProjectUsers.Where(s => userIds.Contains(s.UserId)).Where(s => !s.Project.isAllUserBelongTo).Select(s => s.ProjectId).Distinct();

            // Lấy danh sách PM và thông tin working time của họ
            var listPM = allProjectUsers
                .Where(s => s.Type == ProjectUserType.PM)
                .Where(s => projectIds.Contains(s.ProjectId))
                .Select(s => new PmInfoRequestOffDto
                {
                    UserId = s.UserId,
                    EmailAddress = s.User.EmailAddress,
                    KomuUserId = s.User.KomuUserId,
                    MorningStartAt = s.User.MorningStartAt,
                    MorningEndAt = s.User.MorningEndAt,
                    AfternoonStartAt = s.User.AfternoonStartAt,
                    AfternoonEndAt = s.User.AfternoonEndAt
                })
                .Distinct()
                .ToList();

            listPM.ForEach(pm =>
            {
                // Check if current time is within PM's working hours
                if (!IsWithinWorkingHours(currentTime, pm))
                {
                    return; // Skip this PM if not within their working hours
                }

                //Filter list projects by PM
                var projectIdsByPM = allProjectUsers
                .Where(item => item.UserId.Equals(pm.UserId))
                .Where(s => s.Type == ProjectUserType.PM)
                .Where(s => projectIds.Contains(s.ProjectId))
                .Select(item => item.ProjectId).Distinct().ToList();
                //Logger.Debug("Get list project by PM[" + pm.EmailAddress + "]" + JsonConvert.SerializeObject(allProjectUsers.Where(item => item.UserId.Equals(pm.UserId)).Where(s => s.Type == ProjectUserType.PM).Where(s => projectIds.Contains(s.ProjectId)).Select(item => new { item.ProjectId, item.Project.Name }).ToList()));
                //Filter list userIds by list project by PM
                var projectUserIds = allProjectUsers
                .Where(item => projectIdsByPM
                .Contains(item.ProjectId))
                .Select(item => item.UserId).Distinct().ToList();
                //Logger.Debug("Get list users by list: " + JsonConvert.SerializeObject(allProjectUsers.Where(item => projectIdsByPM.Contains(item.ProjectId)).Select(item => new { item.UserId, item.ProjectId, item.User.FullName }).Distinct().ToList()));

                var row = new NotifyApproveRequestOffDto();
                row.EmailAddress = pm.EmailAddress;
                row.KomuUserId = pm.KomuUserId;
                row.Users = listInfo.Where(item => projectUserIds
                .Contains(item.Key.UserId)).Select(item => new UserInfoApproveRequestOffDto
                {
                    FullName = item.Key.FullName,
                    RequestOffDtos = item.Value
                }).ToList();

                result.Add(row);
            });

            var sb = new StringBuilder();
            foreach (var item in result)
            {
                if (item.CountUsers == 1)
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please approve request off/remote/onsite for **{item.CountUsers}** user");
                }
                else
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please approve request off/remote/onsite for **{item.CountUsers}** users");
                }
                sb.AppendLine($"```");

                var currentUser = "";
                var currentFullName = "";

                foreach (var user in item.Users)
                {
                    if (currentFullName != user.FullName)
                    {
                        if (currentUser != "")
                        {
                            sb.AppendLine("");
                        }

                        currentFullName = user.FullName;
                        currentUser = $"{currentFullName}:\n";
                        sb.Append(currentUser);
                    }
                    else
                    {
                        sb.Append(" ");
                    }

                    user.RequestOffDtos.ForEach(s =>
                    {
                        sb.Append(" ");
                        sb.AppendLine($"({s.RequestType} {s.DayType} - {s.DateAt.ToString("dd/MM/yyyy")})");
                    });
                }

                sb.AppendLine($"```");

                if (countListChannel > 0)
                {
                    for (var i = 0; i < countListChannel; i++)
                    {
                        _komuService.NotifyToChannel(sb.ToString(), arrListChannel[i].Trim());
                    }
                }
                sb.Clear();
            }
        }

        public void SendMessageToUserRequestOffPending(List<RequestAddDto> listRequestOff)
        {
            var listInfo = listRequestOff.GroupBy(s => s.UserId)
                .ToDictionary(s => s.Key,
                s => s.Select(x => new RequestOffDto
                {
                    DateAt = x.DateAt,
                    DayType = x.DateType,
                    RequestType = x.RequestType,
                    UserName = x.UserName,
                }).ToList());

            var currentTime = DateTime.Now.TimeOfDay;

            foreach (var userGroup in listInfo)
            {
                var userId = userGroup.Key;
                var user = _userRepository.FirstOrDefault(u => u.Id == userId);

                if (!IsWithinWorkingHours(currentTime, user))
                {
                    continue; 
                }

                var userRequests = userGroup.Value;
                var userName = userRequests.FirstOrDefault()?.UserName;
                if (string.IsNullOrEmpty(userName)) continue;

                var sb = new StringBuilder();
                var requestCount = userRequests.Count;

                sb.AppendLine($"You have **{requestCount}** pending request{(requestCount > 1 ? "s" : "")}, please confirm with PM to approve it:");

                foreach (var request in userRequests)
                {
                    sb.Append(" ");
                    sb.AppendLine($"({request.RequestType} {request.DayType} - {request.DateAt:dd/MM/yyyy})");
                }

                _komuService.SendMessageToUser(sb.ToString(), userName.Trim());
            }
        }
    }
}

