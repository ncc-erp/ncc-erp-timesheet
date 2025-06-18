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
        public async Task<PunishmentSystemDto> CreatePunishmentSystemAsync(CreatePunishmentSystemDto input)
        {
            Logger.Info($"Received input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                if (input == null)
                {
                    throw new UserFriendlyException("Input cannot be null");
                }

                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new UserFriendlyException("Name is required");
                }

                if (input.Type == UserPunishmentType.NoPunish)
                {
                    throw new UserFriendlyException("Type is required");
                }

                var existingPunishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(ps => ps.Name.ToLower() == input.Name.ToLower() && ps.Type == input.Type);
                if (existingPunishmentSystem != null)
                {
                    throw new UserFriendlyException($"A PunishmentSystem with Name '{input.Name}' and Type '{input.Type}' already exists.");
                }

                var punishmentSystem = ObjectMapper.Map<Timesheet.Entities.PunishmentSystem>(input);
                var createdEntity = await _workScope.InsertAsync(punishmentSystem);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (createdEntity == null || createdEntity.Id <= 0)
                {
                    Logger.Error("Failed to create PunishmentSystem: createdEntity is null or Id is invalid");
                    throw new UserFriendlyException("Failed to create punishment system due to invalid entity.");
                }

                Logger.Info($"Successfully created PunishmentSystem with Id: {createdEntity.Id}");

                var result = ObjectMapper.Map<PunishmentSystemDto>(createdEntity);
                if (result == null)
                {
                    Logger.Error("Mapping to PunishmentSystemDto failed: result is null");
                    throw new UserFriendlyException("Error mapping punishment system data.");
                }
                Logger.Info($"Mapped to DTO: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

                return result;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in CreatePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while creating punishment system. Please try again.");
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
            Logger.Info($"Received input for update: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                if (input == null || input.Id <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystem Id is required");
                }

                var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                if (punishmentSystem == null)
                {
                    Logger.Error($"PunishmentSystem with Id {input.Id} not found");
                    throw new UserFriendlyException("Punishment system not found");
                }
                ObjectMapper.Map(input, punishmentSystem);

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

                Logger.Info($"Successfully updated PunishmentSystem with Id: {input.Id} and related UserPunishments");
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in UpdatePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while updating punishment system. Please try again.");
            }
        }


        [HttpDelete]
        public async Task DeletePunishmentSystemAsync(EntityDto<long> input)
        {
            Logger.Info($"Received input for delete: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {

                if (input == null || input.Id <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystem Id is required");
                }

                var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                if (punishmentSystem == null)
                {
                    Logger.Error($"PunishmentSystem with Id {input.Id} not found");
                    throw new UserFriendlyException("Punishment system not found");
                }


                var hasRelatedUserPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .AnyAsync(up => up.PunishmentSystemId == input.Id);
                if (hasRelatedUserPunishments)
                {
                    Logger.Error($"Cannot delete PunishmentSystem with Id {input.Id} because it is in use by UserPunishment records");
                    throw new UserFriendlyException($"Cannot delete PunishmentSystem with Id {input.Id} because it is currently in use by user punishments.");
                }


                await _workScope.DeleteAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                await CurrentUnitOfWork.SaveChangesAsync();

                Logger.Info($"Successfully deleted PunishmentSystem with Id: {input.Id}");
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in DeletePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while deleting punishment system. Please try again.");
            }
        }


        [HttpGet]
        public async Task<PagedResultDto<PunishmentSystemDto>> GetPunishmentSystemsAsync(GetPunishmentSystemsInput input)
        {


            var query = _workScope.GetAll<Timesheet.Entities.PunishmentSystem>();


            if (!string.IsNullOrEmpty(input.FilterText))
            {
                query = query.Where(x => x.Name.Contains(input.FilterText) ||
                                        x.Description.Contains(input.FilterText));
            }

            if (input.Type != UserPunishmentType.NoPunish) 
            {
                query = query.Where(x => x.Type == input.Type);
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == input.IsActive.Value);
            }
            var totalCount = await query.CountAsync();

            if (string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }
            else
            {
                switch (input.Sorting.ToLower())
                {
                    case "name":
                        query = query.OrderBy(x => x.Name);
                        break;
                    case "name desc":
                        query = query.OrderByDescending(x => x.Name);
                        break;
                    case "type":
                        query = query.OrderBy(x => x.Type);
                        break;
                    case "type desc":
                        query = query.OrderByDescending(x => x.Type);
                        break;
                    case "money":
                        query = query.OrderBy(x => x.Money);
                        break;
                    case "money desc":
                        query = query.OrderByDescending(x => x.Money);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.CreationTime);
                        break;
                }
            }

            var skipCount = input.SkipCount;
            var maxResultCount = input.MaxResultCount;

            var entities = await query
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            var dtos = entities.Select(entity => ObjectMapper.Map<PunishmentSystemDto>(entity)).ToList();

            return new PagedResultDto<PunishmentSystemDto>(totalCount, dtos);
        }

        [HttpGet]
        public async Task<ListResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync()
        {

            var entities = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var dtos = entities.Select(entity => ObjectMapper.Map<PunishmentSystemDto>(entity)).ToList();
            return new ListResultDto<PunishmentSystemDto>(dtos);
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