using Abp.Dependency;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DataExport;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class UserWhitelistServices : BaseDomainService, IUserWhitelistServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        private readonly Dictionary<WhitelistType, string> whitelistTypeDictionary = CommonUtils.WhitelistTypeName();
        public UserWhitelistServices(IWorkScope workScope) : base(workScope)
        {
            _workScope = workScope;
        }

        public async Task<GetUserWhitelistDto> Add(AddUserWhitelistDto input)
        {
            try
            {
                var isExist = await _workScope.GetAll<UserWhitelist>()
                    .Where(x => x.UserId == input.UserId && x.WhitelistSystemId == input.WhitelistSystemId)
                    .AnyAsync();
                if (isExist)
                {
                    throw new UserFriendlyException($"This user is already in the selected whitelist type");
                }

                var whitelistSystem = await _workScope.GetAll<WhitelistSystem>()
                    .FirstOrDefaultAsync(ws => ws.Id == input.WhitelistSystemId)
                    ?? throw new UserFriendlyException($"Cannot find whitelist system with id = {input.WhitelistSystemId}");
                if (!whitelistSystem.IsActive)
                {
                    throw new UserFriendlyException($"The selected whitelist system is not active");
                }

                var user = await _workScope.GetAll<User>()
                    .FirstOrDefaultAsync(u => u.Id == input.UserId)
                    ?? throw new UserFriendlyException($"Cannot find user with id = {input.UserId}");

                var userWhitelist = new UserWhitelist
                {
                    UserId = input.UserId,
                    WhitelistSystemId = input.WhitelistSystemId
                };

                var id = await _workScope.InsertAndGetIdAsync(userWhitelist);

                return new GetUserWhitelistDto
                {
                    Id = id,
                    UserId = user.Id,
                    UserName = user.UserName,
                    WhitelistName = whitelistTypeDictionary[whitelistSystem.Type],
                    WhitelistType = whitelistSystem.Type
                };
            }
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occured while adding user to the whitelist", ex);
            }
        }

        public async Task<List<GetUserWhitelistDto>> GetAll()
        {
            try
            {
                var whitelistData = await _workScope.GetAll<UserWhitelist>()
                    .Where(x => x.WhitelistSystem.IsActive)
                    .Select(x => new
                    {
                        x.Id,
                        x.UserId,
                        x.User.UserName,
                        BranchName = x.User.Branch.Name,
                        BranchColor = x.User.Branch.Color,
                        WhitelistType = x.WhitelistSystem.Type
                    })
                    .ToListAsync();

                var projectUsers = await _workScope.GetAll<ProjectUser>()
                    .Where(pu => pu.Type != ProjectUserType.DeActive && whitelistData.Select(wd => wd.UserId).Contains(pu.UserId))
                    .Join(_workScope.GetAll<Project>()
                        .Where(p => p.Status == ProjectStatus.Active),
                        pu => pu.ProjectId,
                        p => p.Id,
                        (pu, p) => new
                        {
                            pu.UserId,
                            p.Name
                        })
                    .ToListAsync();

                var projectUserDict = projectUsers
                    .GroupBy(pu => pu.UserId)
                    .ToDictionary(g => g.Key, g => g.Select(pu => pu.Name).ToList());

                var result = whitelistData.Select(x => new GetUserWhitelistDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        UserName = x.UserName,
                        Branch = new BranchToDisplayDto
                        {
                            Name = x.BranchName,
                            Color = x.BranchColor
                        },
                        ProjectNames = projectUserDict.ContainsKey(x.UserId) ? projectUserDict[x.UserId] : new List<string>(),
                        WhitelistName = whitelistTypeDictionary[x.WhitelistType],
                        WhitelistType = x.WhitelistType
                    })
                    .OrderBy(x => x.UserName)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occured while retrieving user whitelist", ex);
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await _workScope.DeleteAsync<UserWhitelist>(id);
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occured while deleting user from the whitelist", ex);
            }
        }

        public async Task<FileBase64Dto> DownloadTemplate()
        {
            try
            {
                string fileName = "TemplateImportWhitelist.xlsx";
                string folderPath = Path.Combine("wwwroot", "template");
                string filePath = Path.Combine(folderPath, fileName);

                if (!File.Exists(filePath))
                {
                    throw new UserFriendlyException($"Cannot find whitelist template at path: {filePath}");
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
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occured when reading template file", ex.Message);
            }
        }

        public async Task<ImportUserWhitelistResultDto> ImportFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length <= 0)
                {
                    throw new UserFriendlyException("File not found or empty!");
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx")
                {
                    throw new UserFriendlyException("Invalid file format. Please upload an Excel file with .xlsx extension.");
                }

                var listRowInput = new List<ImportUserWhitelistRowDto>();

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
                            var whitelistType = workSheet.Cells[i, 3].Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(mezonUserId) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(whitelistType))
                                continue;

                            listRowInput.Add(new ImportUserWhitelistRowDto
                            {
                                Row = i,
                                MezonUserId = mezonUserId,
                                Email = email,
                                WhitelistType = whitelistType
                            });
                        }
                    }
                }

                if (!listRowInput.Any()) return new ImportUserWhitelistResultDto();

                var matchedUsers = await _workScope.GetAll<User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork)
                    .Where(u => (!string.IsNullOrEmpty(u.MezonUserId) && listRowInput.Select(x => x.MezonUserId).Contains(u.MezonUserId)) 
                                || (!string.IsNullOrEmpty(u.EmailAddress) && listRowInput.Select(x => x.Email).Contains(u.EmailAddress)))
                    .Select(u => new
                    {
                        u.Id,
                        u.MezonUserId,
                        Email = u.EmailAddress.ToLower().Trim()
                    })
                    .ToListAsync();

                var failedList = new List<string>();

                foreach (var row in listRowInput)
                {
                    var matchedUser = matchedUsers.FirstOrDefault(u =>
                        (!string.IsNullOrEmpty(row.MezonUserId) && u.MezonUserId == row.MezonUserId) ||
                        (!string.IsNullOrEmpty(row.Email) && u.Email == row.Email));

                    row.UserId = matchedUser?.Id;
                }

                var invalidRows = listRowInput.Where(row => row.UserId == null).ToList();

                if (invalidRows.Any())
                {
                    failedList.AddRange(invalidRows.Select(row =>
                    {
                        string userIdentifier = !string.IsNullOrEmpty(row.MezonUserId) ? row.MezonUserId : (row.Email ?? "");
                        return $"Row {row.Row} ({userIdentifier}): User not found or inactive";
                    }));

                    return new ImportUserWhitelistResultDto
                    {
                        SuccessCount = 0,
                        FailCount = failedList.Count,
                        FailedList = failedList
                    };
                }

                var whitelistTypeMap = new Dictionary<string, WhitelistType>(StringComparer.OrdinalIgnoreCase)
                {
                    { "TRACKER_TIME", WhitelistType.TrackerTime },
                    { "FULLY_REMOTE", WhitelistType.FullyRemote }
                };

                var whitelistSystemDictionary = await _workScope.GetAll<WhitelistSystem>()
                    .Where(ws => ws.IsActive)
                    .ToDictionaryAsync(ws => ws.Type, ws => ws.Id);

                var currentUserWhitelist = await _workScope.GetAll<UserWhitelist>()
                    .Where(x => !x.IsDeleted && matchedUsers.Select(u => u.Id).Contains(x.UserId))
                    .Select(x => new
                    {
                        x.UserId,
                        x.WhitelistSystemId
                    })
                    .ToListAsync();

                var userWhitelistInserts = new List<UserWhitelist>();
                foreach (var row in listRowInput)
                {
                    string userIdentifier = !string.IsNullOrEmpty(row.MezonUserId) ? row.MezonUserId : (row.Email ?? "");
                    var inputType = row.WhitelistType ?? "";

                    long userId = row.UserId.Value;
                    var whitelistTypeEnum = whitelistTypeMap[inputType];
                    if (!whitelistSystemDictionary.TryGetValue(whitelistTypeEnum, out long matchedWhitelistSystemId))
                    {
                        failedList.Add($"Row {row.Row} ({userIdentifier}): Whitelist system for type '{inputType}' not found or inactive");
                        continue;
                    }

                    if (currentUserWhitelist.Any(s => s.UserId == userId && s.WhitelistSystemId == matchedWhitelistSystemId))
                    {
                        failedList.Add($"Row {row.Row} ({userIdentifier}): User already exists or processed in whitelist type '{inputType}'");
                        continue;
                    }

                    currentUserWhitelist.Add(new 
                    {
                        UserId = userId,
                        WhitelistSystemId = matchedWhitelistSystemId
                    });

                    userWhitelistInserts.Add(new UserWhitelist
                    {
                        UserId = userId,
                        WhitelistSystemId = matchedWhitelistSystemId
                    });
                }

                if (userWhitelistInserts.Any())
                {
                    await _workScope.InsertRangeAsync(userWhitelistInserts);
                }

                return new ImportUserWhitelistResultDto
                {
                    SuccessCount = userWhitelistInserts.Count,
                    FailCount = failedList.Count,
                    FailedList = failedList
                };
            }
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occured while importing user whitelist from excel file", ex);
            }
        }
    }
}
