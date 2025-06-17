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

        // Create
        [HttpPost]
        public async Task<PunishmentSystemDto> CreatePunishmentSystemAsync(CreatePunishmentSystemDto input)
        {
            Logger.Info($"Received input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                //if (!CheckSecurityCode())
                //{
                //    throw new UserFriendlyException("Wrong security code");
                //}

                // Validate input
                if (input == null)
                {
                    throw new UserFriendlyException("Input cannot be null");
                }

                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new UserFriendlyException("Name is required");
                }

                if (string.IsNullOrWhiteSpace(input.Type))
                {
                    throw new UserFriendlyException("Type is required");
                }

                // Check for duplicate Name and Type
                var existingPunishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(ps => ps.Name.ToLower() == input.Name.ToLower() && ps.Type.ToLower() == input.Type.ToLower());
                if (existingPunishmentSystem != null)
                {
                    throw new UserFriendlyException($"A PunishmentSystem with Name '{input.Name}' and Type '{input.Type}' already exists.");
                }

                // Map and create entity
                var punishmentSystem = ObjectMapper.Map<Timesheet.Entities.PunishmentSystem>(input);
                var createdEntity = await _workScope.InsertAsync(punishmentSystem);
                await CurrentUnitOfWork.SaveChangesAsync();

                Logger.Info($"Successfully created PunishmentSystem with Id: {createdEntity.Id}");

                return ObjectMapper.Map<PunishmentSystemDto>(createdEntity);
            }
            catch (UserFriendlyException)
            {
                throw; // Re-throw UserFriendlyException as-is
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in CreatePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while creating punishment system. Please try again.");
            }
        }
        // Read
        [HttpGet]
        public async Task<PunishmentSystemDto> GetPunishmentSystemAsync(EntityDto<long> input)
        {
            //if (!CheckSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}

            var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
            if (punishmentSystem == null)
            {
                throw new UserFriendlyException("Punishment system not found");
            }
            return ObjectMapper.Map<PunishmentSystemDto>(punishmentSystem);
        }

        // Update
        [HttpPut]
        public async Task UpdatePunishmentSystemAsync(UpdatePunishmentSystemDto input)
        {
            Logger.Info($"Received input for update: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                //if (!CheckSecurityCode())
                //{
                //    throw new UserFriendlyException("Wrong security code");
                //}

                // Validate input
                if (input == null || input.Id <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystem Id is required");
                }

                // Check if PunishmentSystem exists
                var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                if (punishmentSystem == null)
                {
                    Logger.Error($"PunishmentSystem with Id {input.Id} not found");
                    throw new UserFriendlyException("Punishment system not found");
                }

                // Map input to PunishmentSystem
                ObjectMapper.Map(input, punishmentSystem);

                // Update related UserPunishments
                var userPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .Where(up => up.PunishmentSystemId == input.Id)
                    .ToListAsync();
                foreach (var userPunishment in userPunishments)
                {
                    // Sync Money and IsActive from updated PunishmentSystem
                    userPunishment.TotalMoney = punishmentSystem.Money; // Assuming TotalMoney in UserPunishment reflects PunishmentSystem.Money
                                                                        // Add other fields to sync if needed (e.g., IsActive if relevant)
                                                                        // userPunishment.SomeOtherField = punishmentSystem.SomeOtherField;
                }

                // Save changes
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
                throw; // Re-throw UserFriendlyException as-is
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in UpdatePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while updating punishment system. Please try again.");
            }
        }

        // Delete
        [HttpDelete]
        public async Task DeletePunishmentSystemAsync(EntityDto<long> input)
        {
            Logger.Info($"Received input for delete: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                //if (!CheckSecurityCode())
                //{
                //    throw new UserFriendlyException("Timesheet server can't connect");
                //}

                // Validate input
                if (input == null || input.Id <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystem Id is required");
                }

                // Check if PunishmentSystem exists
                var punishmentSystem = await _workScope.GetAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                if (punishmentSystem == null)
                {
                    Logger.Error($"PunishmentSystem with Id {input.Id} not found");
                    throw new UserFriendlyException("Punishment system not found");
                }

                // Check if any UserPunishment is using this PunishmentSystem
                var hasRelatedUserPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .AnyAsync(up => up.PunishmentSystemId == input.Id);
                if (hasRelatedUserPunishments)
                {
                    Logger.Error($"Cannot delete PunishmentSystem with Id {input.Id} because it is in use by UserPunishment records");
                    throw new UserFriendlyException($"Cannot delete PunishmentSystem with Id {input.Id} because it is currently in use by user punishments.");
                }

                // Proceed with deletion
                await _workScope.DeleteAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                await CurrentUnitOfWork.SaveChangesAsync();

                Logger.Info($"Successfully deleted PunishmentSystem with Id: {input.Id}");
            }
            catch (UserFriendlyException)
            {
                throw; // Re-throw UserFriendlyException as-is
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in DeletePunishmentSystemAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while deleting punishment system. Please try again.");
            }
        }

        // Get All with paging and filtering - Thêm mới
        [HttpPost]
        public async Task<PagedResultDto<PunishmentSystemDto>> GetPunishmentSystemsAsync(GetPunishmentSystemsInput input)
        {
            //if (!CheckSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}

            var query = _workScope.GetAll<Timesheet.Entities.PunishmentSystem>();

            // Apply filters
            if (!string.IsNullOrEmpty(input.FilterText))
            {
                query = query.Where(x => x.Name.Contains(input.FilterText) ||
                                        x.Description.Contains(input.FilterText));
            }

            if (!string.IsNullOrEmpty(input.Type))
            {
                query = query.Where(x => x.Type == input.Type);
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == input.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }
            else
            {
                // Simple sorting - có thể mở rộng thêm
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

            // Apply paging
            var skipCount = input.SkipCount ;
            var maxResultCount = input.MaxResultCount ;

            var entities = await query
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            var dtos = entities.Select(entity => ObjectMapper.Map<PunishmentSystemDto>(entity)).ToList();

            return new PagedResultDto<PunishmentSystemDto>(totalCount, dtos);
        }

        // Get All Active - Thêm mới
        [HttpGet]
        public async Task<ListResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync()
        {
            //if (!CheckSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}

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