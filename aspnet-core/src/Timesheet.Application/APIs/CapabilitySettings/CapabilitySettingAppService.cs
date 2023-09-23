using Ncc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.CapabilitySettings.Dto;
using Timesheet.Entities;
using System.Linq;
using static Ncc.Entities.Enum.StatusEnum;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Ncc.Authorization;
using Timesheet.Paging;
using Timesheet.Extension;
using System.Web.Http;
using HttpPutAttribute = Microsoft.AspNetCore.Mvc.HttpPutAttribute;
using HttpDeleteAttribute = Microsoft.AspNetCore.Mvc.HttpDeleteAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using DocumentFormat.OpenXml.VariantTypes;
using Abp.Domain.Uow;
using Amazon.S3.Model.Internal.MarshallTransformations;
using DocumentFormat.OpenXml.Office2010.Excel;
using Ncc.IoC;
using Abp.Linq.Extensions;
using Abp.Domain.Repositories;

namespace Timesheet.APIs.CapabilitySettings
{
    public class CapabilitySettingAppService : AppServiceBase
    {
        public CapabilitySettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_View)]
        public async Task<GridResult<GetPagingCapabilitySettingDto>> GetAllPaging(ParamCapability input)//<CapabilitySettingDto>> GetAllPaging(GridParam param)
        {
            var query = IQGetAllCapabilitySettingsGroupBy(input.Type, input.Param.SearchText);
            return await query.GetGridResult(query, input.Param);
        }
        [HttpGet]
        public async Task<List<GetPagingCapabilitySettingDto>> GetAllCapabilitySettings(Usertype? userType, long? PositionId)//(string capabilityName, Usertype? userType, long? PositionId)
        {
            var capabilitySettings = await
                IQGetAllCapabilitySettingsGroupBy(null, "")//(capabilityName)
                .Where(q => userType.HasValue ? q.UserType == userType.Value : true)
                .Where(q => PositionId.HasValue ? q.PositionId == PositionId.Value : true)
                .ToListAsync();
            return capabilitySettings;
        }
        public async Task<List<CapabilitySettingDto>> GetCapabilitiesByUserTypeAndPositionId(Usertype userType, long PositionId)
        {
            var query = from cs in WorkScope.GetAll<CapabilitySetting>()
                        where cs.UserType == userType && cs.PositionId == PositionId
                        orderby cs.CapabilityId
                        select new CapabilitySettingDto
                        {
                            CapabilityId = cs.CapabilityId,
                            CapabilityName = cs.Capability.Name,
                            Id = cs.Id,
                            PositionId = cs.PositionId,
                            PositionName = cs.Position.Name,
                            UserType = cs.UserType,
                            Type = cs.Capability.Type,
                            Coefficient = cs.Coefficient,
                            GuildeLine = cs.GuildeLine,
                        };
            return await query.ToListAsync();
        }
        public async Task<List<CapabilitySettingDto>> GetRemainCapabilitiesByUserTypeAndPositionId(Usertype userType, long PositionId)
        {
            var capabilitiesExists = await (from cs in WorkScope.GetAll<CapabilitySetting>()
                                            where cs.UserType == userType && cs.PositionId == PositionId
                                            select cs.CapabilityId)
                                                .Distinct().ToListAsync();
            var qremainCapabilities = from c in WorkScope.GetAll<Capability>()
                                      where !capabilitiesExists.Contains(c.Id)
                                      orderby c.Id
                                      select new CapabilitySettingDto
                                      {
                                          CapabilityId = c.Id,
                                          CapabilityName = c.Name,
                                          Type = c.Type,
                                          Coefficient = 1,
                                      };
            return await qremainCapabilities.ToListAsync();
        }

        private async Task<List<CapabilitySetting>> GetListCapabilitySettings(Usertype userType, long PositionId, bool isIncludingSoftDelete = false)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var query = WorkScope.GetAll<CapabilitySetting>()
                            .Where(s => s.UserType == userType)
                            .Where(s => s.PositionId == PositionId)
                            //Không lấy thằng đã bị xoá thì lấy những thằng chưa bị xoá
                            .WhereIf(!isIncludingSoftDelete, s => !s.IsDeleted)
                            .OrderBy(s => s.CapabilityId);
                return await query.ToListAsync();
            }
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_Clone)]
        public async Task<ResponseCapabilitySettingCloneDto> CapabilitySettingClone(CapabilitySettingCloneDto input)
        {
            var listFromCapabilitySettings = await GetListCapabilitySettings(input.FromUserType, input.FromPositionId);

            if (listFromCapabilitySettings.IsEmpty())
            {
                throw new UserFriendlyException("No Capabilites Exist");
            }

            var positionName = await WorkScope.GetAll<CapabilitySetting>().Where(s => s.PositionId == input.ToPositionId).Select(s => s.Position.Name).FirstOrDefaultAsync();

            var listToCapabilitySettings = await GetListCapabilitySettings(input.ToUserType, input.ToPositionId, true);

            if(listToCapabilitySettings.Where(s => !s.IsDeleted).Any())
            {
                throw new UserFriendlyException($"Không thể Clone vì đã tồn tại Capabilities ở CapabilitySetting: {input.ToUserType} {positionName}");
            }

            var deleteCapabilitySettingInListTo = listToCapabilitySettings.Where(s => s.IsDeleted = true).ToList();

            foreach(var item in deleteCapabilitySettingInListTo)
            {
                await WorkScope.Repository<CapabilitySetting>().HardDeleteAsync(item);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            await WorkScope.InsertRangeAsync(listFromCapabilitySettings.Select(s => new CapabilitySetting
            {
                UserType = input.ToUserType,
                CapabilityId = s.CapabilityId,
                PositionId = input.ToPositionId,
                Coefficient = s.Coefficient,
                GuildeLine = s.GuildeLine
            }));

            return new ResponseCapabilitySettingCloneDto
            {
                FromUserType = input.FromUserType,
                FromPositionId = input.FromPositionId,
                ToUserType = input.ToUserType,
                ToPositionId = input.ToPositionId,
            };
        }

            private IQueryable<CapabilitySettingDto> IQGetAllCapabilitySetting()
        {
            var qgetAllCapabilitySettings = from c in WorkScope.GetAll<CapabilitySetting>()
                                            select new CapabilitySettingDto
                                            {
                                                Id = c.Id,
                                                UserType = c.UserType,
                                                CapabilityId = c.Capability.Id,
                                                CapabilityName = c.Capability.Name,
                                                PositionId = c.PositionId,
                                                PositionName = c.Position.Name,
                                                Coefficient = c.Coefficient,
                                                GuildeLine = c.GuildeLine
                                            };
            return qgetAllCapabilitySettings;
        }
        private IQueryable<GetPagingCapabilitySettingDto> IQGetAllCapabilitySettingsGroupBy(CapabilityType? capabilityType, string capabilityName = "")
        {
            var query = WorkScope.GetAll<CapabilitySetting>();
            if (!string.IsNullOrEmpty(capabilityName))
                query = query.Where(x => x.Capability.Name.Contains(capabilityName));
            if (capabilityType.HasValue)
            {
                query = query.Where(x => x.Capability.Type == capabilityType);
            }
            return (from c in query
                    group c by new { c.PositionId, c.Position.Name, c.UserType } into g
                    select new GetPagingCapabilitySettingDto
                    {
                        PositionId = g.Key.PositionId,
                        UserType = g.Key.UserType,
                        PositionName = g.Key.Name,
                        Capabilities = g
                        .OrderBy(z => z.PositionId)
                        .OrderBy(x => x.UserType)
                        .Select(x => new CapabilityInCapabilitySettingDto
                        {
                            CapabilityId = x.CapabilityId,
                            CapabilityName = x.Capability.Name,
                            Id = x.Id,
                            Coefficient = x.Coefficient,
                            Type = x.Capability.Type,
                            GuildeLine = x.GuildeLine,
                        }).OrderBy(z => z.Type).ToList()
                    }
            ).OrderBy(z => z.UserType)
             .ThenBy(x => x.PositionId);
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_AddNew)]
        public async Task<CapabilitySettingDto> CreateCapabilitySetting(CreateUpdateCapabilitySettingDto input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var capabilitiesExists = await WorkScope
                                    .GetAll<CapabilitySetting>()
                                    .Where(s => s.UserType == input.UserType)
                                    .Where(s => s.PositionId == input.PositionId)
                                    .Where(s => s.CapabilityId == input.CapabilityId)
                                    .FirstOrDefaultAsync();
                long id = 0;
                if (capabilitiesExists == default)
                {
                    var capabilitySetting = ObjectMapper.Map<CapabilitySetting>(input);
                    id = await WorkScope.InsertAndGetIdAsync<CapabilitySetting>(capabilitySetting);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    capabilitiesExists.IsDeleted = false;
                    await WorkScope.UpdateAsync(capabilitiesExists);
                    id = capabilitiesExists.Id;
                }
                return await IQGetAllCapabilitySetting()
                            .Where(q => q.Id == id)
                            .FirstOrDefaultAsync();
            }

        }
        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_Edit)]
        public async Task<CapabilitySettingDto> UpdateCapabilitySetting(CreateUpdateCapabilitySettingDto input)
        {
            var capabilitySetting = await WorkScope.GetAsync<CapabilitySetting>(input.Id);
            ObjectMapper.Map<CreateUpdateCapabilitySettingDto, CapabilitySetting>(input, capabilitySetting);
            await WorkScope.UpdateAsync(capabilitySetting);
            await CurrentUnitOfWork.SaveChangesAsync();
            return await IQGetAllCapabilitySetting()
                .Where(q => q.Id == capabilitySetting.Id)
                .FirstOrDefaultAsync();
        }
        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_Delete)]
        public async Task DeleteCapabilitySetting(long Id)
        {
            var capabilitySetting = await WorkScope.GetAsync<CapabilitySetting>(Id);
            capabilitySetting.IsDeleted = true;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_Delete)]
        public async Task DeleteGroupCapabilitySettings(Usertype userType, long PositionId)
        {
            var capabilitySettings = await WorkScope.GetAll<CapabilitySetting>()
                .Where(q => q.UserType == userType && q.PositionId == PositionId)
                .ToListAsync();
            await WorkScope.DeleteRangeAsync(capabilitySettings);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        public List<GetUserTypeForCapabilitySetting> GetUserTypeForCapabilitySettings()
        {
            var userTypes = Enum.GetValues(typeof(Usertype))
                .Cast<Usertype>()
                .Where(x => x == Usertype.Internship || x == Usertype.Staff)
                .Select(x => new GetUserTypeForCapabilitySetting
                {
                    Id = (long)x,
                    Name = x.ToString()
                })
                .ToList();
            return userTypes;
        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_CapabilitySetting_AddNew)]
        public async Task<long> DeActive(long id, Usertype usertype, long positionId)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var capability = WorkScope.GetAll<CapabilitySetting>()
                                          .Where(q => q.CapabilityId == id)
                                          .Where(q => q.UserType == usertype)
                                          .Where(q => q.PositionId == positionId)
                                          .FirstOrDefault();
                if (capability == default) throw new UserFriendlyException("Không có");
                capability.IsDeleted = false;
                await WorkScope.UpdateAsync(capability);
                return id;
            }
        }
    }
}