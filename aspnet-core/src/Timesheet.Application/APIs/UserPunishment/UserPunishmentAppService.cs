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
using Timesheet.APIs.UserPunishments.Dto;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using static Ncc.Entities.Enum.StatusEnum;

namespace TimesheetApplication.UserPunishment
{
    [AbpAuthorize]
    public class UserPunishmentAppService : AppServiceBase, IUserPunishmentAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;

        public UserPunishmentAppService(
          IHttpContextAccessor httpContextAccessor,
          IWorkScope workScope,
          ISettingManager settingManager) : base(workScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _workScope = workScope;
            _settingManager = settingManager;
        }

        [HttpPost]
        public async Task<UserPunishmentDto> CreateUserPunishmentAsync(CreateUserPunishmentDto input)
        {
            try
            {
                await ValidateInputAsync(input);

                var (punishmentSystem, user) = await GetRequiredEntitiesAsync(input);

                await ValidateBusinessRulesAsync(input, punishmentSystem);

                var userPunishment = CreateUserPunishmentEntity(input, punishmentSystem);

                var createdEntity = await _workScope.InsertAsync(userPunishment);
                await CurrentUnitOfWork.SaveChangesAsync();

                return MapToDto(createdEntity);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while creating user punishment. Please try again.");
            }
        }

        private async Task ValidateInputAsync(CreateUserPunishmentDto input)
        {
            var validationErrors = new List<string>();

            if (input == null) validationErrors.Add("Input cannot be null");
            if (input?.UserId <= 0) validationErrors.Add("Invalid UserId");
            if (input?.PunishmentSystemId <= 0) validationErrors.Add("Valid PunishmentSystemId is required");
            if (input?.Count <= 0) validationErrors.Add("Count must be greater than 0");
            if (input?.TotalMoney < 0) validationErrors.Add("TotalMoney cannot be negative");
            if (input?.DateAt ==
              default(DateTime) || input?.DateAt > DateTime.Now.AddDays(1))
                validationErrors.Add("Invalid DateAt value");

            var validTypes = new[] {
        UserPunishmentType.Late,
          UserPunishmentType.NoCheckIn,
          UserPunishmentType.NoCheckOut,
          UserPunishmentType.LateAndNoCheckOut,
          UserPunishmentType.NoCheckInAndNoCheckOut,
          UserPunishmentType.Daily,
          UserPunishmentType.Mention,
          UserPunishmentType.Tracker_20k,
          UserPunishmentType.Tracker_50k,
          UserPunishmentType.Tracker_100k,
          UserPunishmentType.Tracker_200k
      };

            if (input != null && !validTypes.Contains(input.Type))
                validationErrors.Add($"Invalid punishment type: {input.Type}. Valid types are: {string.Join(", ", validTypes)}");

            if (validationErrors.Any())
                throw new UserFriendlyException(string.Join("; ", validationErrors));
        }

        private async Task<(Timesheet.Entities.PunishmentSystem punishmentSystem, User user)> GetRequiredEntitiesAsync(CreateUserPunishmentDto input)
        {
            var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
              .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId);

            var user = await _workScope.GetAll<User>()
              .FirstOrDefaultAsync(x => x.Id == input.UserId);

            var errors = new List<string>();
            if (punishmentSystem == null) errors.Add($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
            if (user == null) errors.Add($"User with Id {input.UserId} not found");

            if (errors.Any())
                throw new UserFriendlyException(string.Join("; ", errors));

            return (punishmentSystem, user);
        }

        private async Task ValidateBusinessRulesAsync(CreateUserPunishmentDto input, Timesheet.Entities.PunishmentSystem punishmentSystem)
        {
            var businessErrors = new List<string>();
            var typeExists = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
              .AnyAsync(x => x.Type == input.Type);
            if (!typeExists)
                businessErrors.Add($"Punishment type {input.Type} is not defined in PunishmentSystem. Please add it to PunishmentSystem first.");

            if (!punishmentSystem.IsActive)
                businessErrors.Add($"PunishmentSystem with Id {input.PunishmentSystemId} is not active");
            var existingPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .AnyAsync(x => x.UserId == input.UserId &&
                x.PunishmentSystemId == input.PunishmentSystemId &&
                x.DateAt.Date == input.DateAt.Date &&
                x.Type == input.Type);
            if (existingPunishment)
                businessErrors.Add($"User already has this punishment type on {input.DateAt:yyyy-MM-dd}");
            if (businessErrors.Any())
                throw new UserFriendlyException(string.Join("; ", businessErrors));
        }
        private Timesheet.Entities.UserPunishment CreateUserPunishmentEntity(CreateUserPunishmentDto input, Timesheet.Entities.PunishmentSystem punishmentSystem)
        {
            var calculatedTotalMoney = punishmentSystem.Money * input.Count;
            return new Timesheet.Entities.UserPunishment
            {
                DateAt = input.DateAt,
                UserId = input.UserId,
                PunishmentSystemId = input.PunishmentSystemId,
                Type = input.Type,
                Count = input.Count,
                TotalMoney = calculatedTotalMoney,
                UserNote = input.UserNote?.Trim(),
                NoteReply = input.NoteReply?.Trim(),
                CreationTime = DateTime.Now,
                CreatorUserId = AbpSession.UserId
            };
        }
        private UserPunishmentDto MapToDto(Timesheet.Entities.UserPunishment entity)
        {
            return new UserPunishmentDto
            {
                Id = entity.Id,
                DateAt = entity.DateAt,
                UserId = entity.UserId,
                PunishmentSystemId = entity.PunishmentSystemId,
                Type = entity.Type,
                Count = entity.Count,
                TotalMoney = entity.TotalMoney,
                UserNote = entity.UserNote,
                NoteReply = entity.NoteReply,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime
            };
        }
        [HttpGet]
        public async Task<UserPunishmentDto> GetUserPunishmentAsync(EntityDto<long> input)
        {
            var userPunishment = await _workScope.GetAsync<Timesheet.Entities.UserPunishment>(input.Id);
            if (userPunishment == null)
            {
                throw new UserFriendlyException("User punishment not found");
            }
            var result = new UserPunishmentDto
            {
                Id = userPunishment.Id,
                DateAt = userPunishment.DateAt,
                UserId = userPunishment.UserId,
                PunishmentSystemId = userPunishment.PunishmentSystemId,
                Type = userPunishment.Type,
                Count = userPunishment.Count,
                TotalMoney = userPunishment.TotalMoney,
                UserNote = userPunishment.UserNote,
                NoteReply = userPunishment.NoteReply,
                CreationTime = userPunishment.CreationTime,
                LastModificationTime = userPunishment.LastModificationTime
            };
            return result;
        }
        [HttpPut]
        public async Task UpdateUserPunishmentAsync(UpdateUserPunishmentDto input)
        {
            try
            {
                ValidateInput(input);
                var entities = await LoadRequiredEntitiesAsync(input);
                await ValidateBusinessRulesAsync(input, entities.PunishmentSystem);
                UpdatePunishmentEntity(input, entities.UserPunishment);
                await _workScope.UpdateAsync(entities.UserPunishment);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while updating user punishment.");
            }
        }
        private async Task<(Timesheet.Entities.UserPunishment UserPunishment, Timesheet.Entities.PunishmentSystem PunishmentSystem, User User)> LoadRequiredEntitiesAsync(UpdateUserPunishmentDto input)
        {
            var userPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .FirstOrDefaultAsync(x => x.Id == input.Id) ??
              throw new UserFriendlyException($"User punishment with Id {input.Id} not found");
            var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
              .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId) ??
              throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
            var user = await _workScope.GetAll<User>()
              .FirstOrDefaultAsync(x => x.Id == input.UserId) ??
              throw new UserFriendlyException($"User with Id {input.UserId} not found");
            return (userPunishment, punishmentSystem, user);
        }
        private async Task ValidateBusinessRulesAsync(UpdateUserPunishmentDto input, Timesheet.Entities.PunishmentSystem punishmentSystem)
        {
            if (!punishmentSystem.IsActive)
                throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} is not active");
            var existingPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .FirstOrDefaultAsync(x => x.Id != input.Id &&
                x.UserId == input.UserId &&
                x.PunishmentSystemId == input.PunishmentSystemId &&
                x.DateAt.Date == input.DateAt.Date &&
                x.Type == input.Type);
            if (existingPunishment != null)
                throw new UserFriendlyException($"User already has this punishment type on {input.DateAt:yyyy-MM-dd}");
        }
        private void UpdatePunishmentEntity(UpdateUserPunishmentDto input, Timesheet.Entities.UserPunishment userPunishment)
        {
            userPunishment.DateAt = input.DateAt;
            userPunishment.UserId = input.UserId;
            userPunishment.PunishmentSystemId = input.PunishmentSystemId;
            userPunishment.Type = input.Type;
            userPunishment.Count = input.Count;
            userPunishment.UserNote = input.UserNote?.Trim();
            userPunishment.NoteReply = input.NoteReply?.Trim();
            userPunishment.LastModificationTime = DateTime.Now;
            userPunishment.LastModifierUserId = AbpSession.UserId;
        }
        private void ValidateInput(UpdateUserPunishmentDto input)
        {
            var validationRules = new List<(bool condition, string message)> {
        (input == null, "Input cannot be null"),
        (input?.Id <= 0, "Valid Id is required"),
        (input?.UserId <= 0, "Valid UserId is required"),
        (input?.PunishmentSystemId <= 0, "Valid PunishmentSystemId is required"),
        (input?.Count <= 0, "Count must be greater than 0"),
        (input?.DateAt ==
          default (DateTime) || input?.DateAt > DateTime.Now.AddDays(1), "Invalid DateAt value")
      };

            var failedRule = validationRules.FirstOrDefault(rule => rule.condition);
            if (failedRule.condition)
                throw new UserFriendlyException(failedRule.message);

            var validTypes = GetValidPunishmentTypes();
            if (!validTypes.Contains(input.Type))
                throw new UserFriendlyException($"Invalid punishment type: {input.Type}. Valid types are: {string.Join(", ", validTypes)}");
        }
        private static UserPunishmentType[] GetValidPunishmentTypes()
        {
            return new[] {
            UserPunishmentType.Late,
            UserPunishmentType.NoCheckIn,
            UserPunishmentType.NoCheckOut,
            UserPunishmentType.LateAndNoCheckOut,
            UserPunishmentType.NoCheckInAndNoCheckOut,
            UserPunishmentType.Daily,
            UserPunishmentType.Mention,
            UserPunishmentType.Tracker_20k,
            UserPunishmentType.Tracker_50k,
            UserPunishmentType.Tracker_100k,
            UserPunishmentType.Tracker_200k
        };
        }
        [HttpDelete]
        public async Task DeleteUserPunishmentAsync(EntityDto<long> input)
        {
            var userPunishment = await _workScope.GetAsync<Timesheet.Entities.UserPunishment>(input.Id);
            if (userPunishment == null)
            {
                throw new UserFriendlyException("User punishment not found");
            }
            await _workScope.DeleteAsync<Timesheet.Entities.UserPunishment>(input.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        [HttpGet]
        public async Task<List<UserPunishmentDto>> GetAllUserPunishmentsAsync()
        {
            Logger.Info("Fetching all UserPunishments");
            try
            {
                var userPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>().ToListAsync();
                if (!userPunishments.Any())
                {
                    Logger.Warn("No UserPunishments found");
                    return new List<UserPunishmentDto>();
                }
                var result = userPunishments.Select(entity => new UserPunishmentDto
                {
                    Id = entity.Id,
                    DateAt = entity.DateAt,
                    UserId = entity.UserId,
                    PunishmentSystemId = entity.PunishmentSystemId,
                    Type = entity.Type,
                    Count = entity.Count,
                    TotalMoney = entity.TotalMoney,
                    UserNote = entity.UserNote,
                    NoteReply = entity.NoteReply,
                    CreationTime = entity.CreationTime,
                    LastModificationTime = entity.LastModificationTime
                }).ToList();
                Logger.Info($"Successfully fetched {result.Count} UserPunishments");
                return result;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in GetAllUserPunishmentsAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while fetching user punishments. Please try again.");
            }
        }
        [HttpGet]
        public async Task<List<UserPunishmentDto>> GetUserPunishmentsAsync(long userId)
        {
            if (userId <= 0)
            {
                throw new UserFriendlyException("Valid UserId is required");
            }
            var query = _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .Where(x => x.UserId == userId);
            var entities = await query.ToListAsync();
            var dtos = entities.Select(entity => new UserPunishmentDto
            {
                Id = entity.Id,
                DateAt = entity.DateAt,
                UserId = entity.UserId,
                PunishmentSystemId = entity.PunishmentSystemId,
                Type = entity.Type,
                Count = entity.Count,
                TotalMoney = entity.TotalMoney,
                UserNote = entity.UserNote,
                NoteReply = entity.NoteReply,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime
            }).ToList();
            return dtos;
        }
    }
}