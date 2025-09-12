using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Authorization.Roles;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc.Net.MimeTypes;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.UserPunishments.Dto;
using Timesheet.DataExport;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Project.Dto;
using Timesheet.Users.Dto;
using TimesheetApplication.PunishmentSystem;
using static Ncc.Entities.Enum.StatusEnum;

namespace TimesheetApplication.UserPunishment
{
    [AbpAuthorize]
    public class UserPunishmentAppService : AppServiceBase, IUserPunishmentAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        private readonly IUserPunishmentServices _userPunishmentServices;
        private readonly IUserServices _userServices;
        private readonly ILogger<UserPunishmentAppService> _logger;

        private readonly string templateFolder = Path.Combine("wwwroot", "template");

        public UserPunishmentAppService(IWorkScope workScope, ILogger<UserPunishmentAppService> logger, IUserPunishmentServices userPunishmentServices, IUserServices userServices, IHttpContextAccessor httpContextAccessor, ISettingManager settingManager) : base(workScope)
        {
            _workScope = workScope;
            _logger = logger;
            _userPunishmentServices = userPunishmentServices;
            _userServices = userServices;
            _httpContextAccessor = httpContextAccessor;
            _settingManager = settingManager;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserPunishments_AddNew)]
        public async Task<UserPunishmentDto> CreateUserPunishmentAsync(CreateUserPunishmentDto input)
        {
            try
            {
                await ValidateInputAsync(input);

                var punishmentSystem = await GetPunishmentSystemAsync(input.Type);
                var user = await GetUserAsync(input.UserId);

                await ValidateBusinessRulesAsync(input, punishmentSystem, user);

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
                _logger.LogError(ex, "Error in CreateUserPunishmentAsync");
                throw new UserFriendlyException("An error occurred while creating user punishment. Please try again.");
            }
        }

        private async Task ValidateInputAsync(CreateUserPunishmentDto input)
        {
            var validationErrors = new List<string>();

            if (input == null) validationErrors.Add("Input cannot be null");
            if (input?.UserId <= 0) validationErrors.Add("Invalid UserId");
            if (input?.Money <= 0) validationErrors.Add("Money must be greater than 0");
            if (input?.DateAt == default(DateTime) || input?.DateAt > DateTime.Now.AddDays(1))
                validationErrors.Add("Invalid DateAt value");

            var validTypes = GetValidPunishmentTypes();
            if (input != null && !validTypes.Contains(input.Type))
                validationErrors.Add($"Invalid punishment type: {input.Type}.");

            if (validationErrors.Any())
                throw new UserFriendlyException(string.Join("; ", validationErrors));
        }

        private async Task<Timesheet.Entities.PunishmentSystem> GetPunishmentSystemAsync(UserPunishmentType type)
        {
            var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                .Where(x => x.Type == type && x.IsActive)
                .FirstOrDefaultAsync();

            if (punishmentSystem == null)
                throw new UserFriendlyException($"Active punishment system for type {type} not found. Please add it to PunishmentSystem first.");

            return punishmentSystem;
        }

        private async Task<User> GetUserAsync(long userId)
        {
            var user = await _workScope.GetAll<User>()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new UserFriendlyException($"User with Id {userId} not found");

            return user;
        }

        private async Task ValidateBusinessRulesAsync(CreateUserPunishmentDto input, Timesheet.Entities.PunishmentSystem punishmentSystem, User user)
        {
            var businessErrors = new List<string>();

            var existingPunishment = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
              .AnyAsync(x => x.UserId == input.UserId &&
                x.DateAt.Date == input.DateAt.Date &&
                x.Type == input.Type);

            if (existingPunishment)
                businessErrors.Add($"User already has this punishment type on {input.DateAt:yyyy-MM-dd}");

            var newPunishmentGroup = GetPunishmentTypeGroup(input.Type);

            var existingPunishmentsOnSameDay = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                .Where(x => x.UserId == input.UserId &&
                       x.DateAt.Date == input.DateAt.Date)
                .Select(x => x.Type)
                .ToListAsync();

            foreach (var existingType in existingPunishmentsOnSameDay)
            {
                var existingGroup = GetPunishmentTypeGroup(existingType);
                if (existingGroup == newPunishmentGroup)
                {
                    businessErrors.Add($"User already has a punishment of group '{newPunishmentGroup}' on {input.DateAt:yyyy-MM-dd}. Cannot add another punishment from the same group.");
                    break;
                }
            }

            if (businessErrors.Any())
                throw new UserFriendlyException(string.Join("; ", businessErrors));
        }

        private Timesheet.Entities.UserPunishment CreateUserPunishmentEntity(CreateUserPunishmentDto input, Timesheet.Entities.PunishmentSystem punishmentSystem)
        {
            var calculatedTotalMoney = input.Money;
            return new Timesheet.Entities.UserPunishment
            {
                DateAt = input.DateAt,
                UserId = input.UserId,
                PunishmentSystemId = punishmentSystem.Id,
                Type = input.Type,
                Count = 1,
                TotalMoney = calculatedTotalMoney,
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

        private string GetPunishmentTypeGroup(UserPunishmentType type)
        {
            switch (type)
            {
                case UserPunishmentType.NoPunish:
                    return "NoPunish";

                case UserPunishmentType.Late:
                case UserPunishmentType.NoCheckIn:
                case UserPunishmentType.NoCheckOut:
                case UserPunishmentType.LateAndNoCheckOut:
                case UserPunishmentType.NoCheckInAndNoCheckOut:
                    return "Attendance";

                case UserPunishmentType.Daily:
                    return "Daily";

                case UserPunishmentType.Mention:
                    return "Mention";

                case UserPunishmentType.Tracker_20k:
                case UserPunishmentType.Tracker_50k:
                case UserPunishmentType.Tracker_100k:
                case UserPunishmentType.Tracker_200k:
                    return "Tracker";

                case UserPunishmentType.ReviewIntern:
                    return "ReviewIntern";

                case UserPunishmentType.PMReport_20k:
                case UserPunishmentType.PMReport_50k:
                    return "PMReport";

                case UserPunishmentType.Ant:
                    return "Ant";

                case UserPunishmentType.UnlockTSGmail:
                    return "UnlockTSGmail";

                case UserPunishmentType.UnlockTSIMS:
                    return "UnlockTSIMS";

                default:
                    return "Unknown";
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserPunishments_Import)]
        public async Task<Object> ImportUserPunishmentFromFile([FromForm] FileInputDto input)
        {
            try
            {
                if (input == null)
                    throw new UserFriendlyException("No file upload!");

                var allowedExtensions = new String[] { ".xlsx", ".xltx" };
                if (!allowedExtensions.Contains(Path.GetExtension(input.File.FileName)))
                    throw new UserFriendlyException("Invalid file upload. Only Excel files (.xlsx, .xltx) are allowed.");

                List<UserPunishmentImportDto> importData = await ReadUserPunishmentFile(input);

                if (importData == null || !importData.Any())
                    throw new UserFriendlyException("No valid data found in the file.");

                var rowErrors = new Dictionary<int, List<string>>();

                var emailList = importData.Select(x => x.Email?.ToLower().Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();

                var userInfoMap = await _workScope.GetAll<User>()
                    .Where(x => emailList.Contains(x.EmailAddress.ToLower().Trim()))
                    .Select(s => new {
                        s.Id,
                        Email = s.EmailAddress.ToLower().Trim()
                    })
                    .ToDictionaryAsync(s => s.Email, s => s.Id);

                var validItems = new List<(UserPunishmentImportDto Item, long UserId)>();

                foreach (var item in importData)
                {
                    var rowErrorList = new List<string>();

                    if (item.ParsingErrors != null && item.ParsingErrors.Any())
                    {
                        rowErrorList.AddRange(item.ParsingErrors);
                    }

                    var (validationErrors, userId) = await ValidateImportItemAsync(item, userInfoMap);
                    if (validationErrors.Any())
                    {
                        rowErrorList.AddRange(validationErrors);
                    }
                    else
                    {
                        validItems.Add((item, userId));
                    }

                    if (rowErrorList.Any())
                    {
                        rowErrors[item.Row] = rowErrorList;
                    }
                }

                if (rowErrors.Any())
                {
                    var formattedErrors = rowErrors
                        .OrderBy(kv => kv.Key)
                        .Select(kv => $"Row {kv.Key}: {string.Join("; ", kv.Value)}")
                        .ToList();

                    throw new UserFriendlyException(
                        "Import failed due to validation errors. Please fix the following issues and try again:\n" +
                        string.Join("\n", formattedErrors));
                }

                using (var uow = CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    try
                    {
                        foreach (var (item, userId) in validItems)
                        {
                            var createDto = new CreateUserPunishmentDto
                            {
                                UserId = userId,
                                Type = item.Type,
                                Money = item.Money,
                                DateAt = item.DateAt,
                                NoteReply = item.NoteReply
                            };

                            var punishmentSystem = await GetPunishmentSystemAsync(item.Type);
                            var userPunishment = CreateUserPunishmentEntity(createDto, punishmentSystem);
                            await _workScope.InsertAsync(userPunishment);
                        }

                        await CurrentUnitOfWork.SaveChangesAsync();

                        return new
                        {
                            Success = true,
                            Message = $"Successfully imported {validItems.Count} records."
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving imported user punishments");
                        throw new UserFriendlyException("An error occurred while saving the imported data. Please try again.");
                        throw new UserFriendlyException("An error occurred while importing user punishments. Please try again.");
                    }
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ImportUserPunishmentFromFile");
                throw new UserFriendlyException("An error occurred while importing user punishments. Please try again.");
            }
        }

        private async Task<List<UserPunishmentImportDto>> ReadUserPunishmentFile([FromForm] FileInputDto input)
        {
            List<UserPunishmentImportDto> result = new List<UserPunishmentImportDto>();

            using (var stream = input.File.OpenReadStream())
            {
                using (var package = new ExcelPackage(stream))
                {
                    try
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.End.Row;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var email = worksheet.Cells[row, 1].Value?.ToString().Trim() ?? "";
                                var moneyStr = worksheet.Cells[row, 2].Value?.ToString().Trim() ?? "0";
                                var typeStr = worksheet.Cells[row, 3].Value?.ToString().Trim() ?? "";
                                var dateAtStr = worksheet.Cells[row, 4].Value?.ToString().Trim() ?? "";
                                var noteReply = worksheet.Cells[row, 5].Value?.ToString().Trim() ?? "";

                                DateTime dateAt = DateTime.Now;
                                int money = 0;
                                UserPunishmentType type;
                                int.TryParse(moneyStr, out money);

                                bool isValidDate = DateTime.TryParse(dateAtStr, out dateAt);
                                Enum.TryParse(typeStr, out type);

                                var rowData = new UserPunishmentImportDto
                                {
                                    Row = row,
                                    Email = email,
                                    Type = type,
                                    OriginalTypeValue = typeStr,
                                    Money = money,
                                    DateAt = dateAt,
                                    OriginalDateAtValue = dateAtStr,
                                    IsValidDate = isValidDate,
                                    UserNote = "",
                                    NoteReply = noteReply
                                };

                                if (rowData.IsEmpty())
                                    continue;

                                result.Add(rowData);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error reading row {row}: {ex.Message}");

                                var errorRow = new UserPunishmentImportDto
                                {
                                    Row = row,
                                    Email = "error@parsing.row",
                                    Type = UserPunishmentType.NoCheckIn,
                                    Count = 0,
                                    DateAt = DateTime.Now,
                                    UserNote = "",
                                    NoteReply = ""
                                };

                                result.Add(errorRow);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading Excel file");
                        throw new UserFriendlyException("Error reading Excel file. Please check the file format.");
                    }
                }
            }
            return result;
        }

        private async Task<(List<string> Errors, long UserId)> ValidateImportItemAsync(UserPunishmentImportDto item, Dictionary<string, long> userInfoMap)
        {
            var errors = new List<string>();
            long userId = 0;

            if (string.IsNullOrEmpty(item.Email))
            {
                errors.Add("Email is required");
            }
            else if (item.Email == "error@parsing.row")
            {
                errors.Add("Error parsing row data");
            }
            else
            {
                var email = item.Email.ToLower().Trim();
                if (!userInfoMap.ContainsKey(email))
                {
                    errors.Add($"User with email '{item.Email}' not found");
                }
                else
                {
                    userId = userInfoMap[email];
                }
            }

            if (item.Money <= 0)
                errors.Add("Money must be greater than 0");

            if (!item.IsValidDate || item.DateAt == default(DateTime) || item.DateAt > DateTime.Now.AddDays(1))
                errors.Add("Invalid DateAt value: " + item.OriginalDateAtValue);

            var validTypes = GetValidPunishmentTypes();
            if (!validTypes.Contains(item.Type))
            {
                errors.Add($"Invalid punishment type: {item.OriginalTypeValue}");
            }

            if (userId > 0 && errors.Count == 0)
            {
                try
                {
                    var moneyToUse = item.Money > 0 ? item.Money : item.Count * (await GetPunishmentSystemAsync(item.Type)).Money;
                    var createDto = new CreateUserPunishmentDto
                    {
                        UserId = userId,
                        Type = item.Type,
                        Money = moneyToUse,
                        DateAt = item.DateAt,
                        NoteReply = item.NoteReply
                    };

                    var punishmentSystem = await GetPunishmentSystemAsync(item.Type);
                    var user = await GetUserAsync(userId);
                    await ValidateBusinessRulesAsync(createDto, punishmentSystem, user);
                }
                catch (UserFriendlyException ex)
                {
                    errors.Add(ex.Message);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return (errors, userId);
        }

        private UserPunishmentType[] GetValidPunishmentTypes()
        {
            return new[] {
                UserPunishmentType.Late,
                UserPunishmentType.NoCheckIn,
                UserPunishmentType.NoCheckOut,
                UserPunishmentType.LateAndNoCheckOut,
                UserPunishmentType.NoCheckInAndNoCheckOut,
                UserPunishmentType.Daily,
                UserPunishmentType.Mention,
                UserPunishmentType.ReviewIntern,
                UserPunishmentType.Tracker_20k,
                UserPunishmentType.Tracker_50k,
                UserPunishmentType.Tracker_100k,
                UserPunishmentType.Tracker_200k,
                UserPunishmentType.PMReport_20k,
                UserPunishmentType.PMReport_50k,
                UserPunishmentType.Ant,
                UserPunishmentType.UnlockTSGmail,
                UserPunishmentType.UnlockTSIMS
            };
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserPunishments_DownloadTemplate)]
        public async Task<FileBase64Dto> DownloadTemplateImportUserPunishment()
        {
            try
            {
                var templateFilePath = Path.Combine(templateFolder, "TemplateImportUserPunishment.xlsx");

                if (!File.Exists(templateFilePath))
                {
                    await CreateUserPunishmentTemplateFile(templateFilePath);
                }

                using (var memoryStream = new MemoryStream(File.ReadAllBytes(templateFilePath)))
                {
                    using (var excelPackage = new ExcelPackage(memoryStream))
                    {
                        string fileBase64 = Convert.ToBase64String(excelPackage.GetAsByteArray());

                        return new FileBase64Dto
                        {
                            FileName = "TemplateImportUserPunishment.xlsx",
                            FileType = MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet,
                            Base64 = fileBase64
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template file");
                throw new UserFriendlyException("Error creating template file");
            }
        }

        private async Task CreateUserPunishmentTemplateFile(string templateFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(templateFilePath));

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UserPunishment");

                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "Money";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "DateAt";
                worksheet.Cells[1, 5].Value = "NoteReply";

                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                worksheet.Cells[2, 1].Value = "example@ncc.asia";
                worksheet.Cells[2, 2].Value = 20000; 
                worksheet.Cells[2, 3].Value = "Ant"; 
                worksheet.Cells[2, 4].Value = DateTime.Now.ToString("M/d/yyyy");
                worksheet.Cells[2, 5].Value = "";

                var allowedTypes = new[] { "Ant", "UnlockTSGmail" };
                var typeValidation = worksheet.DataValidations.AddListValidation("C2:C1000");
                foreach (var type in allowedTypes)
                {
                    typeValidation.Formula.Values.Add(type);
                }

                package.SaveAs(new FileInfo(templateFilePath));
            }
        }

        [HttpPost]
        [AbpAuthorize]
        public async Task<List<PMReportItemDto>> ApplyPMReportPunishmentsAsync()
        {
            return await _userPunishmentServices.ApplyPMReportPunishmentsAsync();
        }

        [HttpGet]
        [AbpAllowAnonymous]
        public async Task<object> GetCompanyPunishmentComparisonAsync(string username)
        {
            try
            {
                string securityCode = _httpContextAccessor.HttpContext.Request.Headers["Security-Code"].FirstOrDefault();
                string validSecurityCode = await _settingManager.GetSettingValueAsync(AppSettingNames.MezonSecurityCode);

                if ((string.IsNullOrEmpty(securityCode) || securityCode != validSecurityCode) && !AbpSession.UserId.HasValue)
                {
                    throw new UserFriendlyException("Unauthorized access");
                }

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = now; 

                var user = await _workScope.GetAll<User>()
                    .Where(u => u.UserName == username)
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    throw new UserFriendlyException($"Không tìm thấy người dùng với username: {username}");
                }

                long userId = user.Id;

                var userRoles = await _workScope.GetRepo<UserRole, long>()
                    .GetAll()
                    .Where(ur => ur.UserId == userId)
                    .Join(_workScope.GetRepo<Ncc.Authorization.Roles.Role, int>()
                        .GetAll(),
                        userRole => userRole.RoleId,
                        role => role.Id,
                        (userRole, role) => role.Name)
                    .ToListAsync();

                bool isOnlyBasicUser = userRoles.Count == 1 && userRoles[0] == "BasicUser";

                var userPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .Where(p => p.UserId == userId && 
                           p.DateAt >= startOfMonth && 
                           p.DateAt <= endOfMonth)
                    .ToListAsync();

                var companyPunishments = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .Where(p => p.UserId != userId && 
                           p.DateAt >= startOfMonth && 
                           p.DateAt <= endOfMonth)
                    .ToListAsync();

                var employeeCount = await _workScope.GetAll<User>()
                    .Where(u => u.IsActive)
                    .CountAsync();

                var userTotalPunishmentAmount = userPunishments.Sum(p => p.TotalMoney);

                var companyTotalPunishmentAmount = companyPunishments.Sum(p => p.TotalMoney);
                var companyAverageTotalPunishmentAmount = companyTotalPunishmentAmount / (employeeCount - 1);

                var maxAmount = Math.Max(
                    Math.Max(userTotalPunishmentAmount, companyAverageTotalPunishmentAmount),
                    1);

                var userTotalBarPercentage = (userTotalPunishmentAmount * 100.0) / maxAmount;
                var companyTotalBarPercentage = (companyAverageTotalPunishmentAmount * 100.0) / maxAmount;

                UserPunishmentType[] punishmentTypes;
                
                if (isOnlyBasicUser)
                {
                    punishmentTypes = new[] {
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
                        UserPunishmentType.Tracker_200k,  
                        UserPunishmentType.Ant,            
                        UserPunishmentType.UnlockTSGmail, 
                        UserPunishmentType.UnlockTSIMS     
                    };
                }
                else
                {
                    punishmentTypes = new[] {
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
                        UserPunishmentType.Tracker_200k,  
                        UserPunishmentType.ReviewIntern,    
                        UserPunishmentType.PMReport_20k, 
                        UserPunishmentType.PMReport_50k,   
                        UserPunishmentType.Ant,            
                        UserPunishmentType.UnlockTSGmail, 
                        UserPunishmentType.UnlockTSIMS     
                    };
                }

                var punishmentDetails = new List<object>();

                var maxPunishmentTypeAmount = 1.0; 

                foreach (var punishmentType in punishmentTypes)
                {
                    var userAmount = userPunishments
                        .Where(p => p.Type == punishmentType)
                        .Sum(p => p.TotalMoney);

                    var companyAmount = companyPunishments
                        .Where(p => p.Type == punishmentType)
                        .Sum(p => p.TotalMoney);
                    var companyAverageAmount = companyAmount / (employeeCount - 1);

                    maxPunishmentTypeAmount = Math.Max(maxPunishmentTypeAmount, 
                        Math.Max(userAmount, companyAverageAmount));
                }

                foreach (var punishmentType in punishmentTypes)
                {
                    var userAmount = userPunishments
                        .Where(p => p.Type == punishmentType)
                        .Sum(p => p.TotalMoney);

                    var companyAmount = companyPunishments
                        .Where(p => p.Type == punishmentType)
                        .Sum(p => p.TotalMoney);
                    var companyAverageAmount = companyAmount / (employeeCount - 1);

                    var userBarPercentage = (userAmount * 100.0) / maxPunishmentTypeAmount;
                    var companyBarPercentage = (companyAverageAmount * 100.0) / maxPunishmentTypeAmount;

                    punishmentDetails.Add(new
                    {
                        punishmentType = punishmentType.ToString(),
                        userAmount,
                        companyAverageAmount,
                        userBarPercentage,
                        companyBarPercentage
                    });
                }

                var response = new
                {
                    title = $"Tiền phạt của bạn so với trung bình công ty (VNĐ)",
                    year = now.Year,
                    month = now.Month,
                    punishmentDetails,
                    userTotalPunishmentAmount,
                    companyAverageTotalPunishmentAmount,
                    totalBarPercentage = new
                    {
                        user = userTotalBarPercentage,
                        company = companyTotalBarPercentage
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyPunishmentComparisonAsync");
                throw new UserFriendlyException("An error occurred while getting punishment comparison data.");
            }
        }
    }
}