using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DataExport;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class RemoteBlacklistServices : BaseDomainService, IRemoteBlacklistServices, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IWorkScope _workScope;
        public RemoteBlacklistServices(IWorkScope workScope, ISettingManager settingManager) : base(workScope)
        {
            _settingManager = settingManager;
            _workScope = workScope;
        }

        public async Task<int> GetMaxRemoteDaysAsync()
        {
            var settingValue = await _settingManager.GetSettingValueAsync(AppSettingNames.WFHSetting);
            return int.Parse(settingValue);
        }

        public async Task<GetRemoteBlacklistDto> AddNewUser(AddNewUserToRemoteBlacklistDto input)
        {
            try
            {
                int maxAllowedRemoteDays = await GetMaxRemoteDaysAsync();
                if (input.PenaltyDays <= 0 || input.PenaltyDays > maxAllowedRemoteDays)
                {
                    throw new UserFriendlyException($"Penalty days must be between 1 and {maxAllowedRemoteDays}!");
                }

                var isAlreadyInBlacklist = await _workScope.GetAll<RemoteBlacklist>()
                    .AnyAsync(b => b.UserId == input.UserId && !b.IsDeleted);
                if (isAlreadyInBlacklist)
                {
                    throw new UserFriendlyException("User is already in remote blacklist.");
                }

                var userInfo = await _workScope.GetAsync<User>(input.UserId);
                if (userInfo == null)
                {
                    throw new UserFriendlyException("User not found!");
                }

                var remoteBlacklistEntity = new RemoteBlacklist
                {
                    UserId = input.UserId,
                    PenaltyDays = input.PenaltyDays
                };
                var newId = await _workScope.InsertAndGetIdAsync(remoteBlacklistEntity);

                return new GetRemoteBlacklistDto
                {
                    Id = newId,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName,
                    FullName = userInfo.FullName,
                    PenaltyDays = remoteBlacklistEntity.PenaltyDays
                };
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while adding user to remote blacklist.", ex);
            }
        }

        public async Task<PagedResultDto<GetRemoteBlacklistDto>> GetAll(GridParam param)
        {
            try
            {
                var result = _workScope.GetAll<RemoteBlacklist>()
                    .Where(b => !b.IsDeleted)
                    .Join(_workScope.GetAll<User>(),
                          b => b.UserId,
                          u => u.Id,
                          (b, u) => new GetRemoteBlacklistDto
                          {
                              Id = b.Id,
                              UserId = u.Id,
                              FullName = u.FullName,
                              UserName = u.UserName,
                              PenaltyDays = b.PenaltyDays
                          });

                var totalCount = await result.CountAsync();
                var pagedResult = await result
                    .Skip(param.SkipCount)
                    .Take(param.MaxResultCount)
                    .ToListAsync();

                return new PagedResultDto<GetRemoteBlacklistDto>
                {
                    TotalCount = totalCount,
                    Items = pagedResult
                };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while retrieving the remote blacklist.", ex);
            }
        }

        public async Task<GetRemoteBlacklistDto> Update(UpdatePenaltyDaysDto input)
        {
            try
            {
                int maxAllowedRemoteDays = await GetMaxRemoteDaysAsync();
                if (input.PenaltyDays <= 0 || input.PenaltyDays > maxAllowedRemoteDays)
                {
                    throw new UserFriendlyException($"Penalty days must be between 1 and {maxAllowedRemoteDays}!");
                }

                var entity = await _workScope.GetAsync<RemoteBlacklist>(input.Id);
                if (entity == null)
                {
                    throw new UserFriendlyException("User not found in remote blacklist!");
                }

                entity.PenaltyDays = input.PenaltyDays;
                await _workScope.UpdateAsync(entity);

                var userInfo = await _workScope.GetAsync<User>(entity.UserId);

                return new GetRemoteBlacklistDto
                {
                    Id = entity.Id,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName,
                    FullName = userInfo.FullName,
                    PenaltyDays = entity.PenaltyDays
                };
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while updating penalty days.", ex);
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await _workScope.DeleteAsync<RemoteBlacklist>(id);
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while deleting the user from remote blacklist.", ex);
            }
        }

        public async Task<FileBase64Dto> DownloadTemplate()
        {
            try
            {
                string fileName = "TemplateImportRemoteBlacklist.xlsx";
                string folderPath = Path.Combine("wwwroot", "template");
                string filePath = Path.Combine(folderPath, fileName);

                if (!File.Exists(filePath))
                {
                    throw new UserFriendlyException($"Cannot find remote blacklist template at path: {filePath}");
                }

                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                string fileBase64 = Convert.ToBase64String(fileBytes);

                return new FileBase64Dto
                {
                    FileName = fileName,
                    FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Base64 = fileBase64
                };
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occured when reading template file.", ex.Message);
            }
        }

        public async Task<ImportRemoteBlacklistResultDto> ImportFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length <= 0)
                    throw new UserFriendlyException("File not found or empty!");

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new UserFriendlyException("Invalid file format. Please use .xlsx");
                }

                var listRowInput = new List<ImportRemoteBlacklistRowDto>();

                using (var stream = file.OpenReadStream())
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var workSheet = package.Workbook.Worksheets[0];
                        int totalRows = workSheet.Dimension?.Rows ?? 0;

                        for (int i = 2; i <= totalRows; i++)
                        {
                            var mezonUserId = workSheet.Cells[i, 1].Value?.ToString()?.Trim();
                            var email = workSheet.Cells[i, 2].Value?.ToString()?.Trim();
                            var penaltyDaysStr = workSheet.Cells[i, 3].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(mezonUserId) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(penaltyDaysStr))
                                continue;

                            listRowInput.Add(new ImportRemoteBlacklistRowDto
                            {
                                Row = i,
                                MezonUserId = mezonUserId,
                                Email = email,
                                PenaltyDaysStr = penaltyDaysStr
                            });
                        }
                    }
                }

                if (!listRowInput.Any()) return new ImportRemoteBlacklistResultDto();

                var excelMezonIds = listRowInput
                    .Where(x => !string.IsNullOrEmpty(x.MezonUserId))
                    .Select(x => x.MezonUserId)
                    .Distinct()
                    .ToList();

                var excelEmails = listRowInput
                    .Where(x => !string.IsNullOrEmpty(x.Email))
                    .Select(x => x.Email.ToLower())
                    .Distinct()
                    .ToList();

                int maxAllowedRemoteDays = await GetMaxRemoteDaysAsync();

                var relevantUsers = await _workScope.GetAll<User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork &&
                                (excelMezonIds.Contains(u.MezonUserId) || excelEmails.Contains(u.EmailAddress.ToLower())))
                    .Select(u => new { 
                        u.Id,
                        u.MezonUserId, 
                        EmailAddress = u.EmailAddress.ToLower().Trim() 
                    })
                    .ToListAsync();

                var relevantUserIds = relevantUsers.Select(u => u.Id).ToList();

                var currentRemoteBlacklistDict = await _workScope.GetAll<RemoteBlacklist>()
                    .Where(b => !b.IsDeleted && relevantUserIds.Contains(b.UserId))
                    .ToDictionaryAsync(b => b.UserId, b => b);

                var failedList = new List<string>();
                var processedUserIds = new HashSet<long>();
                var newRemoteBlacklistInserts = new List<RemoteBlacklist>();
                var remoteBlacklistUpdates = new List<RemoteBlacklist>();

                foreach (var row in listRowInput)
                {
                    string userIdentifier = !string.IsNullOrEmpty(row.Email) ? row.Email : "";

                    if (!int.TryParse(row.PenaltyDaysStr, out int appliedPenaltyDays) || appliedPenaltyDays < 1 || appliedPenaltyDays > maxAllowedRemoteDays)
                    {
                        failedList.Add($"Row {row.Row} ({userIdentifier}): Invalid penalty days");
                        continue;
                    }

                    var foundUser = relevantUsers.FirstOrDefault(u =>
                        (!string.IsNullOrEmpty(row.MezonUserId) && u.MezonUserId == row.MezonUserId) ||
                        (!string.IsNullOrEmpty(row.Email) && u.EmailAddress == row.Email.ToLower().Trim())
                    );
                    if (foundUser == null)
                    {
                        failedList.Add($"Row {row.Row} ({userIdentifier}): User not found or inactive");
                        continue;
                    }

                    if (processedUserIds.Contains(foundUser.Id))
                    {
                        failedList.Add($"Row {row.Row} ({userIdentifier}): User already processed in previous row");
                        continue;
                    }
                    processedUserIds.Add(foundUser.Id);

                    bool hasExistingBlacklist = currentRemoteBlacklistDict.TryGetValue(foundUser.Id, out var existingBlacklist);

                    if (!hasExistingBlacklist)
                    {
                        newRemoteBlacklistInserts.Add(new RemoteBlacklist
                        {
                            UserId = foundUser.Id,
                            PenaltyDays = appliedPenaltyDays
                        });
                    }
                    else
                    {
                        if (existingBlacklist.PenaltyDays != appliedPenaltyDays)
                        {
                            existingBlacklist.PenaltyDays = appliedPenaltyDays;
                            if (!remoteBlacklistUpdates.Contains(existingBlacklist))
                            {
                                remoteBlacklistUpdates.Add(existingBlacklist);
                            }
                        }
                    }
                }

                if (newRemoteBlacklistInserts.Any())
                {
                    await _workScope.InsertRangeAsync(newRemoteBlacklistInserts);
                }

                if (remoteBlacklistUpdates.Any())
                {
                    await _workScope.UpdateRangeAsync(remoteBlacklistUpdates);
                }

                return new ImportRemoteBlacklistResultDto
                {
                    SuccessCount = newRemoteBlacklistInserts.Count + remoteBlacklistUpdates.Count,
                    FailCount = failedList.Count,
                    FailedList = failedList
                };
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred during remote blacklist import.", ex);
            }
        }
    }
}
