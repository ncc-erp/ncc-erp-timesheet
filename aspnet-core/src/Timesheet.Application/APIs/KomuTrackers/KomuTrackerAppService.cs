using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.KomuTrackers.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Ncc.Configuration;
using Timesheet.Uitls;
using Ncc.Authorization.Users;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text;
using Ncc.IoC;
using Timesheet.NCCAuthen;

namespace Timesheet.APIs.KomuTrackers
{
    public class KomuTrackerAppService : AppServiceBase
    {
        public KomuTrackerAppService(IWorkScope workScope) : base(workScope)
        {
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_KomuTracker_View)]
        [HttpPost]
        public async Task<GridResult<GetKomuTrackerUserDto>> GetAllPagging(GridParam input, DateTime? dateAt, int? branchId, string emailAddress)
        {
            var query = from km in WorkScope.GetAll<KomuTracker>()
                        where (!dateAt.HasValue || km.DateAt.Date == dateAt.Value)
                        where (String.IsNullOrEmpty(emailAddress) || km.EmailAddress == emailAddress)
                        join u in WorkScope.GetAll<User>()
                        on km.EmailAddress equals u.EmailAddress into tt
                        where (!branchId.HasValue || tt.FirstOrDefault().BranchId == branchId)
                        select new GetKomuTrackerUserDto
                        {
                            EmailAddress = km.EmailAddress,
                            DateAt = km.DateAt,
                            WorkingMinute = km.WorkingMinute,
                            ComputerName = km.ComputerName,
                            FullName = tt.FirstOrDefault().FullName,
                            UserType = tt.FirstOrDefault().Type,
                            Branch = tt.FirstOrDefault().BranchOld,
                            BranchColor = tt.FirstOrDefault().Branch.Color,
                            BranchDisplayName = tt.FirstOrDefault().Branch.DisplayName,
                            AvatarPath = tt.FirstOrDefault().AvatarPath,
                        };

            return await query.GetGridResult(query, input);
        }

        [HttpPost]
        [NccAuthentication]
        public async Task<string> Save(SaveKomuTrackerDto input)
        {
            if (input.ListKomuTracker.IsEmpty())
            {
                return "ListKomuTracker empty";
            }

            var listKomuTrackerDB = await WorkScope.GetAll<KomuTracker>()
                .Where(s => s.DateAt.Date == input.DateAt.Date)
                .ToListAsync();

            var dbEmails = listKomuTrackerDB.Select(s => s.EmailAddress).ToHashSet();
            var inputEmails = input.ListKomuTracker.Select(s => s.EmailAddress).ToHashSet();

            var listToInsert = input.ListKomuTracker.Where(s => !dbEmails.Contains(s.EmailAddress));
            var listToUpdate = listKomuTrackerDB.Where(s => inputEmails.Contains(s.EmailAddress));

            var sb = new StringBuilder();
            sb.AppendLine($"Inserted: {listToInsert.Count()} rows");
            sb.AppendLine($"Updated: {listToUpdate.Count()} rows");

            sb.AppendLine($"Inserted email list:");
            if (listToInsert.Any())
            {
                foreach (var dto in listToInsert)
                {
                    var item = ObjectMapper.Map<KomuTracker>(dto);
                    item.DateAt = input.DateAt.Date;
                    await WorkScope.InsertAsync(item);
                    sb.AppendLine(dto.EmailAddress);
                }
            }

            sb.AppendLine($"Updated email list:");
            if (listToUpdate.Any())
            {
                foreach (var entity in listToUpdate)
                {
                    var dto = input.ListKomuTracker.Where(s => s.EmailAddress == entity.EmailAddress).FirstOrDefault();
                    ObjectMapper.Map(dto, entity);
                    sb.AppendLine(dto.EmailAddress);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            } 

            return sb.ToString();
        }
    }
}