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
            Logger.Info($"Received input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                if (input == null)
                {
                    throw new UserFriendlyException("Input cannot be null");
                }

                if (!input.UserId.HasValue || input.UserId.Value <= 0)
                {
                    throw new UserFriendlyException("Valid UserId is required");
                }

                if (input.PunishmentSystemId <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystemId is required");
                }

                var validTypes = new[] {
            UserPunishmentType.Late,
            UserPunishmentType.NoCheckIn,
            UserPunishmentType.NoCheckOut,
            UserPunishmentType.LateAndNoCheckOut,
            UserPunishmentType.NoCheckInAndNoCheckOut,
            UserPunishmentType.Daily,
            UserPunishmentType.Mention,
            UserPunishmentType.Level1_20k,
            UserPunishmentType.Level2_50k,
            UserPunishmentType.Level3_100k,
            UserPunishmentType.Level4_200k
        };

                if (!validTypes.Contains(input.Type))
                {
                    throw new UserFriendlyException($"Invalid punishment type: {input.Type}. Valid types are: {string.Join(", ", validTypes)}");
                }

                if (input.Count <= 0)
                {
                    throw new UserFriendlyException("Count must be greater than 0");
                }

                if (input.TotalMoney < 0)
                {
                    throw new UserFriendlyException("TotalMoney cannot be negative");
                }

                if (input.DateAt == default(DateTime) || input.DateAt > DateTime.Now.AddDays(1))
                {
                    throw new UserFriendlyException("Invalid DateAt value");
                }

                var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId);

                if (punishmentSystem == null)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
                }

                if (!punishmentSystem.IsActive)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} is not active");
                }

                var user = await _workScope.GetAll<User>()
                    .FirstOrDefaultAsync(x => x.Id == input.UserId.Value);

                if (user == null)
                {
                    throw new UserFriendlyException($"User with Id {input.UserId.Value} not found");
                }

                var existingPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .FirstOrDefaultAsync(x => x.UserId == input.UserId.Value
                                           && x.PunishmentSystemId == input.PunishmentSystemId
                                           && x.DateAt.Date == input.DateAt.Date
                                           && x.Type == input.Type);

                if (existingPunishment != null)
                {
                    throw new UserFriendlyException($"User already has this punishment type on {input.DateAt:yyyy-MM-dd}");
                }

                var calculatedTotalMoney = punishmentSystem.Money * input.Count;
                if (input.TotalMoney > 0 && input.TotalMoney != calculatedTotalMoney)
                {
                    Logger.Warn($"Provided TotalMoney ({input.TotalMoney}) differs from calculated ({calculatedTotalMoney}). Using calculated value.");
                }

                var userPunishment = new Timesheet.Entities.UserPunishment
                {
                    DateAt = input.DateAt,
                    UserId = input.UserId.Value,
                    PunishmentSystemId = input.PunishmentSystemId,
                    Type = input.Type,
                    Count = input.Count,
                    TotalMoney = calculatedTotalMoney, 
                    UserNote = input.UserNote?.Trim(),
                    NoteReply = input.NoteReply?.Trim(),
                    CreationTime = DateTime.Now,
                    CreatorUserId = AbpSession.UserId
                };

                Logger.Info($"Creating UserPunishment: UserId={userPunishment.UserId}, PunishmentSystemId={userPunishment.PunishmentSystemId}, Type={userPunishment.Type}");

                var createdEntity = await _workScope.InsertAsync(userPunishment);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (createdEntity == null || createdEntity.Id <= 0)
                {
                    Logger.Error("Failed to create UserPunishment: createdEntity is null or Id is invalid");
                    throw new UserFriendlyException("Failed to create user punishment due to invalid entity.");
                }

                Logger.Info($"Successfully created UserPunishment with Id: {createdEntity.Id}");
                var result = new UserPunishmentDto
                {
                    Id = createdEntity.Id,
                    DateAt = createdEntity.DateAt,
                    UserId = createdEntity.UserId,
                    PunishmentSystemId = createdEntity.PunishmentSystemId,
                    Type = createdEntity.Type,
                    Count = createdEntity.Count,
                    TotalMoney = createdEntity.TotalMoney,
                    UserNote = createdEntity.UserNote,
                    NoteReply = createdEntity.NoteReply,
                    CreationTime = createdEntity.CreationTime,
                    LastModificationTime = createdEntity.LastModificationTime
                };

                if (result == null)
                {
                    Logger.Error("Mapping to UserPunishmentDto failed: result is null");
                    throw new UserFriendlyException("Error mapping user punishment data.");
                }

                return result;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in CreateUserPunishmentAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while creating user punishment. Please try again.");
            }
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
            Logger.Info($"Received input for update: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
               
                if (input == null)
                {
                    throw new UserFriendlyException("Input cannot be null");
                }

                if (input.Id <= 0)
                {
                    throw new UserFriendlyException("Valid Id is required");
                }

                if (!input.UserId.HasValue || input.UserId.Value <= 0)
                {
                    throw new UserFriendlyException("Valid UserId is required");
                }

                if (input.PunishmentSystemId <= 0)
                {
                    throw new UserFriendlyException("Valid PunishmentSystemId is required");
                }

                var validTypes = new[] {
            UserPunishmentType.Late,
            UserPunishmentType.NoCheckIn,
            UserPunishmentType.NoCheckOut,
            UserPunishmentType.LateAndNoCheckOut,
            UserPunishmentType.NoCheckInAndNoCheckOut,
            UserPunishmentType.Daily,
            UserPunishmentType.Mention,
            UserPunishmentType.Level1_20k,
            UserPunishmentType.Level2_50k,
            UserPunishmentType.Level3_100k,
            UserPunishmentType.Level4_200k
        };

                if (!validTypes.Contains(input.Type))
                {
                    throw new UserFriendlyException($"Invalid punishment type: {input.Type}. Valid types are: {string.Join(", ", validTypes)}");
                }

                if (input.Count <= 0)
                {
                    throw new UserFriendlyException("Count must be greater than 0");
                }

                if (input.TotalMoney < 0)
                {
                    throw new UserFriendlyException("TotalMoney cannot be negative");
                }

                if (input.DateAt == default(DateTime) || input.DateAt > DateTime.Now.AddDays(1))
                {
                    throw new UserFriendlyException("Invalid DateAt value");
                }

                var userPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .FirstOrDefaultAsync(x => x.Id == input.Id);

                if (userPunishment == null)
                {
                    Logger.Error($"UserPunishment with Id {input.Id} not found");
                    throw new UserFriendlyException($"User punishment with Id {input.Id} not found");
                }

                var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId);

                if (punishmentSystem == null)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
                }

                if (!punishmentSystem.IsActive)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} is not active");
                }

                var user = await _workScope.GetAll<User>()
                    .FirstOrDefaultAsync(x => x.Id == input.UserId.Value);

                if (user == null)
                {
                    throw new UserFriendlyException($"User with Id {input.UserId.Value} not found");
                }

               
                var existingPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .FirstOrDefaultAsync(x => x.Id != input.Id
                                           && x.UserId == input.UserId.Value
                                           && x.PunishmentSystemId == input.PunishmentSystemId
                                           && x.DateAt.Date == input.DateAt.Date
                                           && x.Type == input.Type);

                if (existingPunishment != null)
                {
                    throw new UserFriendlyException($"User already has this punishment type on {input.DateAt:yyyy-MM-dd}");
                }

            
                var calculatedTotalMoney = punishmentSystem.Money * input.Count;
                if (input.TotalMoney > 0 && input.TotalMoney != calculatedTotalMoney)
                {
                    Logger.Warn($"Provided TotalMoney ({input.TotalMoney}) differs from calculated ({calculatedTotalMoney}). Using calculated value.");
                }

               
                userPunishment.DateAt = input.DateAt;
                userPunishment.UserId = input.UserId.Value;
                userPunishment.PunishmentSystemId = input.PunishmentSystemId;
                userPunishment.Type = input.Type;
                userPunishment.Count = input.Count;
                userPunishment.TotalMoney = calculatedTotalMoney; 
                userPunishment.UserNote = input.UserNote?.Trim();
                userPunishment.NoteReply = input.NoteReply?.Trim();
                userPunishment.LastModificationTime = DateTime.Now;
                userPunishment.LastModifierUserId = AbpSession.UserId;

                Logger.Info($"Updating UserPunishment: Id={userPunishment.Id}, UserId={userPunishment.UserId}, PunishmentSystemId={userPunishment.PunishmentSystemId}");

                await _workScope.UpdateAsync(userPunishment);
                await CurrentUnitOfWork.SaveChangesAsync();

                Logger.Info($"Successfully updated UserPunishment with Id: {userPunishment.Id}");
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in UpdateUserPunishmentAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while updating user punishment. Please try again.");
            }
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
        public async Task<PagedResultDto<UserPunishmentDto>> GetUserPunishmentsAsync(GetUserPunishmentsInput input)
        {
            var query = _workScope.GetAll<Timesheet.Entities.UserPunishment>();

          
            if (!string.IsNullOrEmpty(input.FilterText))
            {
                query = query.Where(x => x.Type.ToString().Contains(input.FilterText) ||
                                        x.UserNote.Contains(input.FilterText) ||
                                        x.NoteReply.Contains(input.FilterText));
            }

            if (input.Type != UserPunishmentType.NoPunish && input.Type != 0)
            {
                query = query.Where(x => x.Type == input.Type);
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
                    case "type":
                        query = query.OrderBy(x => x.Type);
                        break;
                    case "type desc":
                        query = query.OrderByDescending(x => x.Type);
                        break;
                    case "userid":
                        query = query.OrderBy(x => x.UserId);
                        break;
                    case "userid desc":
                        query = query.OrderByDescending(x => x.UserId);
                        break;
                    case "punishmentsystemid":
                        query = query.OrderBy(x => x.PunishmentSystemId);
                        break;
                    case "punishmentsystemid desc":
                        query = query.OrderByDescending(x => x.PunishmentSystemId);
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

            return new PagedResultDto<UserPunishmentDto>(totalCount, dtos);
        }
    }
}