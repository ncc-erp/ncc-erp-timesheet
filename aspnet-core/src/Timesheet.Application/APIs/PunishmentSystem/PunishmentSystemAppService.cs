using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.Entities;
using TimesheetApplication.PunishmentSystem;
using Abp.Application.Services.Dto;
using Ncc.Configuration;
using Abp.Configuration;
using Ncc.IoC;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Ncc;
using Timesheet.APIs.PunishmentSystems.Dto;
using Microsoft.AspNetCore.Mvc;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Linq.Extensions;
using Abp.Extensions;
using System.ComponentModel.DataAnnotations;
namespace TimesheetApplication.PunishmentSystem
{
    [AbpAuthorize]
    public class PunishmentSystemAppService : AppServiceBase, IPunishmentSystemAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        public PunishmentSystemAppService(
            IHttpContextAccessor httpContextAccessor,
            IWorkScope workScope,
            ISettingManager settingManager) : base(workScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _workScope = workScope;
            _settingManager = settingManager;
        }
        [HttpPost]
        public async Task<PunishmentSystemDto> CreatePunishmentSystemAsync([Required] CreatePunishmentSystemDto input)
        {
            try
            {
                if (input == null)
                {
                    throw new UserFriendlyException("Input cannot be null");
                }
                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new UserFriendlyException("Name is required and cannot be empty");
                }
                if (!Enum.IsDefined(typeof(UserPunishmentType), input.Type))
                {
                    throw new UserFriendlyException("Type is required and must be a valid punishment type.");
                }
                var existingPunishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .Where(p => p.Type == input.Type.Value).AnyAsync();
                if (existingPunishmentSystem)
                {
                    throw new UserFriendlyException($"Punishment system with type {input.Type.Value} already exists.");
                }
                var punishmentSystem = ObjectMapper.Map<Timesheet.Entities.PunishmentSystem>(input);
                var createdEntity = await _workScope.InsertAsync(punishmentSystem);
                await CurrentUnitOfWork.SaveChangesAsync();
                return ObjectMapper.Map<PunishmentSystemDto>(createdEntity) ??
                       throw new UserFriendlyException("Failed to create punishment system due to invalid entity");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while creating punishment system. Please try again");
            }
        }
        [HttpGet]
        public async Task<PunishmentSystemDto> GetPunishmentSystemAsync(EntityDto<long> input)
        {
            var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
            if (punishmentSystem == null)
            {
                throw new UserFriendlyException("Punishment system not found");
            }
            return ObjectMapper.Map<PunishmentSystemDto>(punishmentSystem);
        }
        [HttpPut]
        public async Task UpdatePunishmentSystemAsync(UpdatePunishmentSystemDto input)
        {
            var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
              .FirstOrDefaultAsync(x => x.Id == input.Id);
            if (punishmentSystem == null)
            {
                throw new UserFriendlyException($"Punishment system with ID {input.Id} not found.");
            }
            if (input.Money < 0)
            {
                throw new UserFriendlyException("Money cannot be negative.");
            }
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                throw new UserFriendlyException("Name is required.");
            }
            punishmentSystem.Name = input.Name;
            punishmentSystem.Description = input.Description;
            punishmentSystem.Type = input.Type;
            punishmentSystem.Money = input.Money;
            punishmentSystem.IsActive = input.IsActive;
            var userPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .Where(up => up.PunishmentSystemId == input.Id)
              .ToListAsync();
            foreach (var userPunishment in userPunishments)
            {
                userPunishment.TotalMoney = punishmentSystem.Money;
            }
            await _workScope.UpdateAsync(punishmentSystem);
            if (userPunishments.Any())
            {
                await _workScope.UpdateRangeAsync(userPunishments);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        [HttpDelete]
        public async Task DeletePunishmentSystemAsync(EntityDto<long> input)
        {
            try
            {
                var hasRelatedUserPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                  .AnyAsync(up => up.PunishmentSystemId == input.Id);
                if (hasRelatedUserPunishments)
                {
                    throw new UserFriendlyException($"Cannot delete PunishmentSystem with Id {input.Id} because it is currently in use by user punishments.");
                }
                await _workScope.DeleteAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while deleting punishment system. Please try again.");
            }
        }
        [HttpGet]
        public async Task<PagedResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync(GetAllActivePunishmentSystemsInput input)
        {
            var query = _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
              .Where(x => x.IsActive)
              .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText),
                x => x.Name.Contains(input.FilterText) || x.Description.Contains(input.FilterText));
            var totalCount = await query.CountAsync();
            var allEntities = await query.ToListAsync();
            var sortedEntities = string.IsNullOrEmpty(input.Sorting) || input.Sorting == "Name" ?
              allEntities.OrderBy(x => x.Name).ToList() :
              allEntities.OrderBy(x => x.Name).ToList();
            var pagedEntities = sortedEntities
              .Skip(input.SkipCount)
              .Take(input.MaxResultCount)
              .ToList();
            var dtos = sortedEntities.Select(entity => ObjectMapper.Map<PunishmentSystemDto>(entity)).ToList();
            return new PagedResultDto<PunishmentSystemDto>(totalCount, dtos);
        }
        private bool CheckSecurityCode()
        {
            var securityCode = _settingManager.GetSettingValue(AppSettingNames.SecurityCode);
            var header = _httpContextAccessor.HttpContext?.Request.Headers;
            var securityCodeHeader = header?["securityCode"].ToString();
            return securityCode == securityCodeHeader;
        }
    }
}