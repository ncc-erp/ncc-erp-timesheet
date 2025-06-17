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
                Logger.Info($"Received input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");


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

                if (string.IsNullOrWhiteSpace(input.Type))
                {
                    throw new UserFriendlyException("Type is required");
                }

                if (input.Count <= 0)
                {
                    throw new UserFriendlyException("Count must be greater than 0");
                }


                var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId);
                if (punishmentSystem == null)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
                }


                var user = await _workScope.GetAll<User>()
                    .FirstOrDefaultAsync(x => x.Id == input.UserId.Value);
                if (user == null)
                {
                    throw new UserFriendlyException($"User with Id {input.UserId.Value} not found");
                }


                var userPunishment = new Timesheet.Entities.UserPunishment
                {
                    DateAt = input.DateAt,
                    UserId = input.UserId.Value,
                    PunishmentSystemId = input.PunishmentSystemId,
                    Type = input.Type,
                    Count = input.Count,
                    TotalMoney = input.TotalMoney,
                    UserNote = input.UserNote,
                    NoteReply = input.NoteReply,
                    CreationTime = DateTime.Now,
                    CreatorUserId = AbpSession.UserId
                };

                Logger.Info($"Created UserPunishment object: PunishmentSystemId={userPunishment.PunishmentSystemId}, UserId={userPunishment.UserId}");

                var createdEntity = await _workScope.InsertAsync(userPunishment);
                await CurrentUnitOfWork.SaveChangesAsync();

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
                    CreationTime = createdEntity.CreationTime
                };

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
            return ObjectMapper.Map<UserPunishmentDto>(userPunishment);
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

                if (string.IsNullOrWhiteSpace(input.Type))
                {
                    throw new UserFriendlyException("Type is required");
                }

                if (input.Count <= 0)
                {
                    throw new UserFriendlyException("Count must be greater than 0");
                }


                var userPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .FirstOrDefaultAsync(x => x.Id == input.Id);
                if (userPunishment == null)
                {
                    Logger.Error($"UserPunishment with Id {input.Id} not found");
                    throw new UserFriendlyException($"User punishment with id = {input.Id} not found!");
                }


                var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .FirstOrDefaultAsync(x => x.Id == input.PunishmentSystemId);
                if (punishmentSystem == null)
                {
                    throw new UserFriendlyException($"PunishmentSystem with Id {input.PunishmentSystemId} not found");
                }


                var user = await _workScope.GetAll<User>()
                    .FirstOrDefaultAsync(x => x.Id == input.UserId.Value);
                if (user == null)
                {
                    throw new UserFriendlyException($"User with Id {input.UserId.Value} not found");
                }


                userPunishment.DateAt = input.DateAt;
                userPunishment.UserId = input.UserId.Value;
                userPunishment.PunishmentSystemId = input.PunishmentSystemId;
                userPunishment.Type = input.Type;
                userPunishment.Count = input.Count;
                userPunishment.TotalMoney = input.TotalMoney;
                userPunishment.UserNote = input.UserNote;
                userPunishment.NoteReply = input.NoteReply;
                userPunishment.LastModificationTime = DateTime.Now;
                userPunishment.LastModifierUserId = AbpSession.UserId;

                Logger.Info($"Updated UserPunishment object: PunishmentSystemId={userPunishment.PunishmentSystemId}, UserId={userPunishment.UserId}");

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

                var result = ObjectMapper.Map<List<UserPunishmentDto>>(userPunishments);
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
            Logger.Info($"Fetching UserPunishments with input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
            try
            {
                var query = _workScope.GetAll<Timesheet.Entities.UserPunishment>().AsNoTracking();


                if (!string.IsNullOrEmpty(input.FilterText))
                {
                    query = query.Where(x => x.Type.Contains(input.FilterText) ||
                                            x.UserNote.Contains(input.FilterText) ||
                                            x.NoteReply.Contains(input.FilterText));
                }

                if (input.UserId.HasValue)
                {
                    query = query.Where(x => x.UserId == input.UserId.Value);
                }

                if (input.PunishmentSystemId.HasValue)
                {
                    query = query.Where(x => x.PunishmentSystemId == input.PunishmentSystemId.Value);
                }

                if (!string.IsNullOrWhiteSpace(input.Type))
                {
                    query = query.Where(x => x.Type == input.Type);
                }

                if (input.IsActive.HasValue && input.IsActive.Value)
                {
                    query = query.Where(x => !x.IsDeleted);
                }


                var totalCount = await query.CountAsync();
                if (string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderByDescending(x => x.CreationTime);
                    Logger.Info("Sorting applied: default (CreationTime descending)");
                }
                else
                {
                    Logger.Info($"Applying sorting: {input.Sorting}");
                    switch (input.Sorting.ToLower().Trim())
                    {
                        case "creationtime":
                            query = query.OrderBy(x => x.CreationTime);
                            break;
                        case "creationtime desc":
                            query = query.OrderByDescending(x => x.CreationTime);
                            break;
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
                        default:
                            query = query.OrderByDescending(x => x.CreationTime);
                            Logger.Warn($"Unsupported sorting value '{input.Sorting}', defaulting to CreationTime descending");
                            break;
                    }
                }
                if (input.SkipCount < 0 || input.MaxResultCount <= 0)
                {
                    throw new UserFriendlyException("Invalid paging parameters: SkipCount must be non-negative and MaxResultCount must be positive.");
                }


                var entities = await query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();

                var dtos = entities.Select(entity => ObjectMapper.Map<UserPunishmentDto>(entity)).ToList();
                Logger.Info($"Fetched {dtos.Count} UserPunishments out of {totalCount} total records");

                return new PagedResultDto<UserPunishmentDto>(totalCount, dtos);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error in GetUserPunishmentsAsync: {ex.Message}", ex);
                throw new UserFriendlyException("An error occurred while fetching user punishments. Please try again.");
            }
        }
    }
}