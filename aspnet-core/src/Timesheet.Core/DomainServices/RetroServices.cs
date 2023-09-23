using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class RetroServices : BaseDomainService, IRetroServices
    {
        public List<NotifyRetroDto> GetListPmNotRetro(long retroId)
        {
            var listProjectInRetro = WorkScope.GetAll<RetroResult>()
                .Where(s => s.RetroId == retroId)
                .Select(s => s.ProjectId)
                .Distinct()
                .ToList();

            var listProjectNotInRetro = WorkScope.GetAll<Project>()
                .Where(s => !listProjectInRetro.Contains(s.Id))
                .Where(s => s.Status == ProjectStatus.Active)
                .Select(s => s.Id)
                .ToList();

            return WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Where(s => listProjectNotInRetro.Contains(s.ProjectId))
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Select(s => new ProjectUserInfoDto
                {
                    UserId = s.UserId,
                    ProjectId = s.ProjectId,
                    EmailAddress = s.User.EmailAddress,
                    KomuUserId = s.User.KomuUserId,
                    ProjectName = s.Project.Name,
                    ProjectCode = s.Project.Code
                })
                .GroupBy(s => new
                {
                    s.UserId,
                    s.EmailAddress,
                    s.KomuUserId,
                })
                .Select(s => new NotifyRetroDto
                {
                    EmailAddress = s.Key.EmailAddress,
                    KomuUserId = s.Key.KomuUserId,
                    Projects = s.Select(x => new NotifyProjectRetroInfoDto
                    {
                        Name = x.ProjectName,
                        Code = x.ProjectCode,
                    }).ToList()
                })
                .ToList();
        }

        public long LastIdRetro()
        {
            return WorkScope.GetAll<Retro>()
                .Where(s => s.Status == RetroStatus.Public)
                .OrderByDescending(s => s.Id)
                .Select(s => s.Id).FirstOrDefault();
        }
    }
}
