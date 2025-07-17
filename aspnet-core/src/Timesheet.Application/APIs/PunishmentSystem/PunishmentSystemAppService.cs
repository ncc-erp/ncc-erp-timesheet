using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc.Net.MimeTypes;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.PunishmentSystems.Dto;
using Timesheet.Entities;
using Timesheet.Users.Dto;
using Timesheet.APIs.UserPunishments.Dto;
using Timesheet.DataExport;
using TimesheetApplication.PunishmentSystem;
using static Ncc.Entities.Enum.StatusEnum;

namespace TimesheetApplication.PunishmentSystem
{
    [AbpAuthorize]
    public class PunishmentSystemAppService : AppServiceBase, IPunishmentSystemAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        private readonly ILogger<PunishmentSystemAppService> _logger;
        private readonly string templateFolder = Path.Combine("wwwroot", "templates");

        public PunishmentSystemAppService(
            IHttpContextAccessor httpContextAccessor,
            IWorkScope workScope,
            ISettingManager settingManager,
            ILogger<PunishmentSystemAppService> logger) : base(workScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _workScope = workScope;
            _settingManager = settingManager;
            _logger = logger;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_AddNew)]
        public async Task<PunishmentSystemDto> CreatePunishmentSystemAsync([Required] CreatePunishmentSystemDto input)
        {
            if (input == null)
                throw new UserFriendlyException("Input cannot be null");
            if (string.IsNullOrWhiteSpace(input.Name))
                throw new UserFriendlyException("Name is required and cannot be empty");
            if (!input.Type.HasValue || !Enum.IsDefined(typeof(UserPunishmentType), input.Type.Value))
                throw new UserFriendlyException("A valid punishment type is required.");
            if (input.Money < 0)
                throw new UserFriendlyException("Money cannot be negative.");

            try
            {
                var exists = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .AsNoTracking()
                    .AnyAsync(p => p.Type == input.Type.Value);

                if (exists)
                {
                    var isActive = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                        .AsNoTracking()
                        .Where(p => p.Type == input.Type.Value)
                        .Select(p => p.IsActive)
                        .FirstAsync();

                    var status = isActive ? "active" : "inactive";
                    throw new UserFriendlyException(
                        $"A {status} punishment system with type {input.Type.Value} already exists. " +
                        "Please update the existing one instead of creating a new one.");
                }

                var entity = ObjectMapper.Map<Timesheet.Entities.PunishmentSystem>(input);
                var createdEntity = await _workScope.InsertAsync(entity);

                await CurrentUnitOfWork.SaveChangesAsync();

                var result = ObjectMapper.Map<PunishmentSystemDto>(createdEntity)
                             ?? throw new UserFriendlyException("Failed to create punishment system due to an internal error");

                return result;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while creating punishment system", ex);
                throw new UserFriendlyException(
                    "An error occurred while creating punishment system. " +
                    "Please try again or contact support if the problem persists.");
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
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_Edit)]
        public async Task UpdatePunishmentSystemAsync(UpdatePunishmentSystemDto input)
        {
            if (input == null)
                throw new UserFriendlyException("Input cannot be null");
            if (input.Money < 0)
                throw new UserFriendlyException("Money cannot be negative.");
            if (string.IsNullOrWhiteSpace(input.Name))
                throw new UserFriendlyException("Name is required.");
            if (!Enum.IsDefined(typeof(UserPunishmentType), input.Type))
                throw new UserFriendlyException("A valid punishment type is required.");

            var punishmentSystem = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            if (punishmentSystem == null)
                throw new UserFriendlyException($"Punishment system with ID {input.Id} not found.");

            if (punishmentSystem.IsActive && !input.IsActive)
            {
                var inUse = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                    .AsNoTracking()
                    .AnyAsync(up => up.PunishmentSystemId == input.Id);

                if (inUse)
                    throw new UserFriendlyException(
                        $"Cannot deactivate punishment with ID {input.Id} because it is currently in use by user punishments."
                    );
            }

            if (punishmentSystem.Type != input.Type)
            {
                var duplicate = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                    .AsNoTracking()
                    .AnyAsync(p => p.Type == input.Type && p.Id != input.Id);

                if (duplicate)
                    throw new UserFriendlyException($"A punishment system with type {input.Type} already exists.");
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
                userPunishment.TotalMoney = input.Money;
            }

            await _workScope.UpdateAsync(punishmentSystem);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_Delete)]
        public async Task DeletePunishmentSystemAsync(EntityDto<long> input)
        {
            if (input == null)
                throw new UserFriendlyException("Input cannot be null");

            var isUsed = await _workScope.GetAll<Timesheet.Entities.UserPunishment>()
                .AsNoTracking()
                .AnyAsync(up => up.PunishmentSystemId == input.Id);

            if (isUsed)
            {
                throw new UserFriendlyException(
                    $"Cannot delete PunishmentSystem with Id {input.Id} " +
                    "because it is currently in use by user punishments."
                );
            }

            try
            {
                await _workScope.DeleteAsync<Timesheet.Entities.PunishmentSystem>(input.Id);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (AbpDbConcurrencyException)
            {
                throw new UserFriendlyException($"PunishmentSystem with Id {input.Id} not found.");
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while deleting punishment system", ex);
                throw new UserFriendlyException(
                    "An error occurred while deleting punishment system. Please try again."
                );
            }
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_View)]
        public async Task<PagedResultDto<PunishmentSystemDto>> GetAllActivePunishmentSystemsAsync(GetAllActivePunishmentSystemsInput input)
        {
            var query = _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_Import)]
        public async Task<Object> ImportPunishmentFromFile([FromForm] FileInputDto input)
        {
            try
            {
                if (input == null)
                    throw new UserFriendlyException("No file upload!");

                var allowedExtensions = new String[] { ".xlsx", ".xltx" };
                if (!allowedExtensions.Contains(Path.GetExtension(input.File.FileName)))
                    throw new UserFriendlyException("Invalid file upload. Only Excel files (.xlsx, .xltx) are allowed.");

                List<PunishmentSystemImportDto> importData = await ReadPunishmentSystemFile(input);

                if (importData == null || !importData.Any())
                    throw new UserFriendlyException("No valid data found in the file.");

                var rowErrorList = new List<string>();
                var validItems = new List<PunishmentSystemImportDto>();
                var rowErrors = new Dictionary<int, List<string>>();

                foreach (var item in importData)
                {
                    var validationErrors = await ValidateImportItemAsync(item);
                    if (validationErrors.Any())
                    {
                        int row = item.Row;
                        if (!rowErrors.ContainsKey(row))
                        {
                            rowErrors[row] = new List<string>();
                        }
                        rowErrors[row].AddRange(validationErrors.Select(e => e.Replace($"Row {row}: ", "")));
                    }
                    else
                    {
                        validItems.Add(item);
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

                if (!validItems.Any())
                {
                    throw new UserFriendlyException("No valid data found in the file after validation.");
                }

                using (var uow = CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    try
                    {
                        _logger.LogInformation($"Starting import of {validItems.Count} punishment systems");
                        var successList = new List<PunishmentSystemImportDto>();

                        foreach (var item in validItems)
                        {
                            var entity = new Timesheet.Entities.PunishmentSystem
                            {
                                Name = item.Name,
                                Description = item.Description,
                                Type = item.Type,
                                Money = item.Money,
                                IsActive = item.IsActive
                            };

                            await _workScope.InsertAsync(entity);
                            successList.Add(item);
                        }

                        await CurrentUnitOfWork.SaveChangesAsync();
                        _logger.LogInformation($"Successfully imported {successList.Count} punishment systems");

                        return new
                        {
                            SuccessCount = successList.Count,
                            SuccessList = successList,
                            FailedCount = 0,
                            FailedList = new List<string>(),
                            ErrorCount = 0,
                            ErrorList = new List<string>()
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during transaction in ImportPunishmentFromFile");
                        throw new UserFriendlyException("An error occurred while importing punishment systems. Please try again.");
                    }
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ImportPunishmentFromFile");
                throw new UserFriendlyException("An error occurred while importing punishment systems. Please try again.");
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Punishments_DownloadTemplate)]
        public async Task<FileBase64Dto> DownloadTemplateImportPunishment()
        {
            try
            {
                var templateFilePath = Path.Combine(templateFolder, "TemplateImportPunishmentSystem.xlsx");

                if (!File.Exists(templateFilePath))
                {
                    await CreatePunishmentSystemTemplateFile(templateFilePath);
                }

                using (var memoryStream = new MemoryStream(File.ReadAllBytes(templateFilePath)))
                {
                    using (var excelPackage = new ExcelPackage(memoryStream))
                    {
                        string fileBase64 = Convert.ToBase64String(excelPackage.GetAsByteArray());

                        return new FileBase64Dto
                        {
                            FileName = "TemplateImportPunishmentSystem.xlsx",
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

        private async Task CreatePunishmentSystemTemplateFile(string templateFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(templateFilePath));

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PunishmentSystem");

                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Description";
                worksheet.Cells[1, 3].Value = "Type";
                worksheet.Cells[1, 4].Value = "Money";
                worksheet.Cells[1, 5].Value = "IsActive";

                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                worksheet.Cells[2, 1].Value = "Example Punishment";
                worksheet.Cells[2, 2].Value = "Description for example punishment";
                worksheet.Cells[2, 3].Value = "Late";
                worksheet.Cells[2, 4].Value = "50000";
                worksheet.Cells[2, 5].Value = "TRUE";

                var validTypes = string.Join(",", Enum.GetNames(typeof(UserPunishmentType)));
                var typeValidation = worksheet.DataValidations.AddListValidation("C2:C1000");
                typeValidation.Formula.Values.Add(validTypes);

                var boolValidation = worksheet.DataValidations.AddListValidation("E2:E1000");
                boolValidation.Formula.Values.Add("TRUE");
                boolValidation.Formula.Values.Add("FALSE");

                package.SaveAs(new FileInfo(templateFilePath));
            }
        }

        private async Task<List<PunishmentSystemImportDto>> ReadPunishmentSystemFile([FromForm] FileInputDto input)
        {
            List<PunishmentSystemImportDto> result = new List<PunishmentSystemImportDto>();
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
                                var name = worksheet.Cells[row, 1].Value?.ToString().Trim() ?? "";
                                var description = worksheet.Cells[row, 2].Value?.ToString().Trim() ?? "";
                                var typeStr = worksheet.Cells[row, 3].Value?.ToString().Trim() ?? "";
                                var money = Convert.ToInt32(worksheet.Cells[row, 4].Value?.ToString().Trim() ?? "0");
                                var isActiveStr = worksheet.Cells[row, 5].Value?.ToString().Trim().ToUpper() ?? "TRUE";
                                bool isActive = isActiveStr == "TRUE";

                                UserPunishmentType type = UserPunishmentType.NoCheckIn;
                                bool isValidType = Enum.TryParse(typeStr, out type);

                                var rowData = new PunishmentSystemImportDto
                                {
                                    Row = row,
                                    Name = name,
                                    Description = description,
                                    Type = type,
                                    OriginalTypeValue = typeStr,
                                    IsValidType = isValidType,
                                    Money = money,
                                    IsActive = isActive
                                };

                                if (rowData.IsEmpty())
                                    continue;

                                result.Add(rowData);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error reading row {row}: {ex.Message}");

                                var errorRow = new PunishmentSystemImportDto
                                {
                                    Row = row,
                                    Name = "Error",
                                    Type = UserPunishmentType.NoCheckIn,
                                    OriginalTypeValue = "",
                                    IsValidType = false,
                                    Money = 0,
                                    IsActive = false
                                };
                                errorRow.ParsingErrors.Add($"Error parsing row: {ex.Message}");

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

        private async Task<List<string>> ValidateImportItemAsync(PunishmentSystemImportDto item)
        {
            var errors = new List<string>();

            if (item.ParsingErrors != null && item.ParsingErrors.Any())
            {
                errors.AddRange(item.ParsingErrors);
                return errors;
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errors.Add($"Row {item.Row}: Name is required");
            }
            else if (item.Name.Length > 256)
            {
                errors.Add($"Row {item.Row}: Name cannot exceed 256 characters");
            }

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                errors.Add($"Row {item.Row}: Description is required");
            }
            else if (item.Description.Length > 1000)
            {
                errors.Add($"Row {item.Row}: Description cannot exceed 1000 characters");
            }

            if (!item.IsValidType || !Enum.IsDefined(typeof(UserPunishmentType), item.Type))
            {
                errors.Add($"Row {item.Row}: Invalid punishment type: {item.OriginalTypeValue}");
            }

            if (item.Money < 0)
            {
                errors.Add($"Row {item.Row}: Money cannot be negative");
            }

            bool exists = await _workScope.GetAll<Timesheet.Entities.PunishmentSystem>()
                .AsNoTracking()
                .AnyAsync(p => p.Type == item.Type);

            if (exists)
            {
                errors.Add($"Row {item.Row}: A punishment system with type {item.Type} already exists. Please update the existing one instead of creating a new one.");
            }

            return errors;
        }
    }
}