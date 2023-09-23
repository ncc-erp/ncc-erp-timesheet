using Abp.Configuration;
using Abp.Dependency;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using System.Collections.Generic;
using System.Linq;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.DomainServices
{
    public class SendMessageRequestPendingTeamBuildingToHRServices : BaseDomainService, ISendMessageRequestPendingTeamBuildingToHRServices, ITransientDependency
    {
        public List<SendMessageRequestPendingTeamBuildingToHRDto> GetListRequestPendingTeamBuilding()
        {
            var listEmailHR = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREmail);

            string[] arrListEmailHR = listEmailHR.Split(',');
            int countListEmailHR = arrListEmailHR.Count();

            var dEmailAddressToKomuId = WorkScope.GetAll<User>()
                .Where(s => arrListEmailHR.Contains(s.EmailAddress) || arrListEmailHR.Contains(s.UserName))
                .ToDictionary(s => s.EmailAddress, s => s.KomuUserId);

            var listRequest = WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.Status == Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending)
                .Select(s => new RequestTeamBuildingDto
                {
                    Title = s.TitleRequest,
                    RequesterEmail = s.Requester.EmailAddress,
                    Note = s.Note,
                }).ToList();

            var listResult = new List<SendMessageRequestPendingTeamBuildingToHRDto>();

            if(countListEmailHR > 0)
            {
                for (var i = 0; i < countListEmailHR; i++)
                {
                    listResult.Add(new SendMessageRequestPendingTeamBuildingToHRDto
                    {
                        EmailAddress = arrListEmailHR[i],
                        KomuUserId = dEmailAddressToKomuId[arrListEmailHR[i]],
                        Requests = listRequest
                    });
                }
            }

            return listResult;
        }
    }
}
