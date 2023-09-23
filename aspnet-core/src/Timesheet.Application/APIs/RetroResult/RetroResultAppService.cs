using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using OfficeOpenXml;
using Ncc.Net.MimeTypes;
using Timesheet.DataExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.RetroDetails.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Ncc.Entities;
using Abp.Collections.Extensions;
using Microsoft.EntityFrameworkCore.Internal;
using Abp.Extensions;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Services.Project;
using Timesheet.APIs.Retros.Dto;
using Abp.Domain.Uow;
using Ncc.IoC;

namespace Timesheet.APIs.RetroDetails
{
    public class RetroResultAppService : AppServiceBase
    {

        public static ProjectService _projectService;
        private readonly string templateFolder = Path.Combine("wwwroot", "template");

        public RetroResultAppService(ProjectService projectService, IWorkScope workScope) : base(workScope)
        {
            _projectService = projectService;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_ViewAllTeam, Ncc.Authorization.PermissionNames.Retro_RetroDetail_AddEmployeeMyTeam)]
        public async Task<GridResult<RetroResultDto>> GetAllPagging(InputMultiFilterRetroResultPagingDto input, long retroId)
        { 
            var isViewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.Retro_RetroDetail_ViewAllTeam);

            /*var myPMProjectIds = WorkScope.GetAll<ProjectUser>()
                .Where(x => x.Type == ProjectUserType.PM && x.UserId == AbpSession.UserId.Value)
                .Select(x => x.ProjectId).ToList();*/

            var myPMProject = await GetProjectPMRetroResult(retroId);
            var myPMProjectIds = myPMProject.Select(x => x.Id).ToList();

            var dicUsers = WorkScope.GetAll<User>()
                                    .Select(s => new
                                    {
                                        s.Id,
                                        s.FullName,
                                    }).ToDictionary(s => s.Id, s => s.FullName);

            var qretroResult = WorkScope.GetAll<RetroResult>()
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Include(s => s.Project)
                .Include(s => s.Position)
                .Include(s => s.Pm)
                .Where(x => x.RetroId == retroId)
                .Where(x => !x.IsDeleted)
                .WhereIf(!isViewAll, s => myPMProjectIds.Contains(s.ProjectId))
                .Select(s => new RetroResultDto
                {
                    UserId = s.UserId,
                    Id = s.Id,
                    FullName = s.User.FullName,
                    EmailAddress = s.User.EmailAddress,
                    ProjectName = s.Project.Name,
                    PositionName = s.Position != null ? s.Position.Name : "",
                    BranchColor = s.Branch.Color,
                    UserBranchColor = s.User.Branch.Color,
                    BranchName = s.Branch.Name,
                    UserBranchName = s.User != null && s.User.Branch != null ? s.User.Branch.Name : "",
                    Level = s.UserLevel,
                    UserLevel = s.User.Level,
                    Type = s.UserType,
                    UserType = s.User.Type,
                    RetroName = s.Retro.Name,
                    Point = s.Point,
                    Note = s.Note,
                    LastModifierTime = s.LastModificationTime,
                    CreationTime = s.CreationTime,
                    PositionId = s.PositionId,
                    CreatedUserName = (s.CreatorUserId.HasValue && dicUsers.ContainsKey(s.CreatorUserId.Value)) ? dicUsers[s.CreatorUserId.Value] : "",
                    ProjectId = s.ProjectId,
                    BranchId = s.BranchId,
                    UserBranchId = s.User.BranchId,
                    LastModifierUserName = (s.LastModifierUserId.HasValue && dicUsers.ContainsKey(s.LastModifierUserId.Value)) ? dicUsers[s.LastModifierUserId.Value] : "",
                    PmId = s.PmId,
                    PmFullName = s.Pm.FullName,
                    PmEmailAddress = s.Pm.EmailAddress,
                });

            qretroResult = qretroResult.WhereIf(!input.Usertypes.IsNullOrEmpty(), x => x.Type.HasValue && input.Usertypes.Contains(x.Type.Value))
                                       .WhereIf(!input.Userlevels.IsNullOrEmpty(), x => x.Level.HasValue && input.Userlevels.Contains(x.Level.Value))
                                       .WhereIf(!input.PositionIds.IsNullOrEmpty(), x => input.PositionIds.Contains(x.PositionId))
                                       .WhereIf(!input.ProjecIds.IsNullOrEmpty(), x => input.ProjecIds.Contains(x.ProjectId))
                                       .WhereIf(!input.BranchIds.IsNullOrEmpty(), x => input.BranchIds.Contains(x.BranchId.Value));
            if (input.LeftPoint <= input.RightPoint) qretroResult = qretroResult.Where(x => input.LeftPoint <= x.Point && input.RightPoint >= x.Point);

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return await qretroResult.GetGridResult(qretroResult, input.GridParam);
            }
        }

        [HttpGet]
        public async Task<List<GetAllUserInRetroResultDto>> GetAllUsersNotInTheRetroResultByRetroId(long retroId, long? projectId)
        {
            var UserIdInRetroResult = WorkScope.GetAll<RetroResult>()
                .Where(x => x.RetroId == retroId)
                .Select(x => x.UserId)
                .ToList();

            var userInProject = WorkScope.GetAll<ProjectUser>()
                                         .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
                                         .Select(s => s.UserId)
                                         .ToList();

            var UserInfo = WorkScope.GetAll<User>()
                .Select(x => new GetAllUserInRetroResultDto
                {
                    UserId = x.Id,
                    FullNameAndEmail = x.FullName + " - " + x.EmailAddress
                })
                .Where(x => !UserIdInRetroResult.Contains(x.UserId))
                .WhereIf(projectId.HasValue, x => userInProject.Contains(x.UserId))
                .ToList();

            return UserInfo;

        }
        [HttpGet]
        public async Task<List<GetAllUserInRetroResultDto>> GetAllUsers(long retroId, long? projectId)
        {
            var UserIdInRetroResult = WorkScope.GetAll<RetroResult>()
                .Where(x => x.RetroId == retroId)
                .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
                .Select(x => x.UserId)
                .ToList();

            var userInProject = WorkScope.GetAll<ProjectUser>()
                                         .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
                                         .Select(s => s.UserId)
                                         .ToList();

            var UserInfo = WorkScope.GetAll<User>()
                .Select(x => new GetAllUserInRetroResultDto
                {
                    UserId = x.Id,
                    FullNameAndEmail = x.FullName + " - " + x.EmailAddress
                })
                .WhereIf(projectId.HasValue,x => !UserIdInRetroResult.Contains(x.UserId))
                .WhereIf(projectId.HasValue, x => userInProject.Contains(x.UserId))
                .ToList();

            return UserInfo;

        }

        [HttpGet]
        public async Task<List<GetAllPmInRetroResultDto>> GetAllPms(long retroId, long? projectId)
        {
            // sửa lại vì theo logic cũ bắt buộc phải có bản ghi của chính PM đó, thì mới hiển thị được các bản ghi trong team của PM đó
            //var UserIdInRetroResult = WorkScope.GetAll<RetroResult>()
            //    .Where(x => x.RetroId == retroId)
            //    .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
            //    .Select(x => x.UserId)
            //    .ToList();
            var userLoginId = AbpSession.UserId.Value;

            var PmInProject = WorkScope.GetAll<ProjectUser>()
                                         .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
                                         .Where(s => s.Type == ProjectUserType.PM)
                                         .Select(s => s.UserId)
                                         .ToList();

            var PmInfo = WorkScope.GetAll<User>()
                .Select(x => new GetAllPmInRetroResultDto
                {
                    PmId = x.Id,
                    PmFullName = x.FullName,
                    PmEmailAddress = x.EmailAddress,
                    IsDefault = x.Id == userLoginId ? true : false,
                })
                //.WhereIf(projectId.HasValue, x => !UserIdInRetroResult.Contains(x.PmId))
                .WhereIf(projectId.HasValue, x => PmInProject.Contains(x.PmId))
                .ToList();

            return PmInfo;
        }

        [HttpGet]
        public async Task<List<GetAllProjectByRetroIdDto>> GetAllProject()
        {
            var userId = AbpSession.UserId.Value;
            var addAllProject = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.Retro_RetroDetail_AddEmployeeAllTeam);

            return await WorkScope.GetAll<ProjectUser>()
                .WhereIf(!addAllProject, s => s.UserId == userId)
                .WhereIf(!addAllProject, s => s.Type == ProjectUserType.PM)
                .Select(s => new GetAllProjectByRetroIdDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Code,
                })
                .Distinct()
                .ToListAsync();
        }
        [HttpGet]
        public async Task<List<GetAllProjectByRetroIdDto>> GetProjectPMRetro()
        {
            var userId = AbpSession.UserId.Value;
            var isViewAllProject = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.Retro_RetroDetail_ViewAllTeam);

            return await WorkScope.GetAll<ProjectUser>()
                .WhereIf(!isViewAllProject, s => s.UserId == userId)
                .WhereIf(!isViewAllProject, s => s.Type == ProjectUserType.PM)
                .Select(s => new GetAllProjectByRetroIdDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Customer.Name
                }).Distinct().OrderBy(s => s.Name).ToListAsync();
        }
        [HttpGet]
        //add tolower.trim to const PMPositionCode
        public async Task<List<GetAllProjectByRetroIdDto>> GetProjectPMRetroResult(long retroId)
        {
            var userId = AbpSession.UserId.Value;
            var isViewAllProject = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.Retro_RetroDetail_ViewAllTeam);

            var projectPM = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => s.ProjectId).ToListAsync();

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return await WorkScope.GetAll<RetroResult>()
                .Where(s => s.RetroId == retroId)
                .Where(s => !s.IsDeleted)
                .WhereIf(!isViewAllProject, s => projectPM.Contains(s.ProjectId))
                //.WhereIf(!isViewAllProject, s => s.CreatorUserId == userId)
                //.WhereIf(!isViewAllProject, s => s.Position.Code.ToLower().Trim() == TimesheetConsts.PMPositonCode.ToLower().Trim())//added here
                .Select(s => new GetAllProjectByRetroIdDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Code,
                })
                .Distinct()
                .OrderBy(s => s.Name)
                .ToListAsync();
            }
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_AddEmployeeAllTeam, Ncc.Authorization.PermissionNames.Retro_RetroDetail_AddEmployeeMyTeam)]
        public async Task<RetroResultCreateDto> Create(RetroResultCreateDto input)
        {
            var isExistUserId = await WorkScope.GetAll<RetroResult>()
               .Where(s => s.UserId == input.UserId)
               .Where(s => s.RetroId == input.RetroId)
               .Where(s => s.ProjectId == input.ProjectId)
               .Where(s => s.PmId == input.PmId)
               .Where(s => s.Id != input.Id).AnyAsync();
            if (isExistUserId)
            {
                throw new UserFriendlyException(string
                    .Format("This UserId {0} in ProjectId {1} already in this Month", input.UserId, input.ProjectId));
            }
            if (input.Point < 0 || input.Point > 5)
            {
                throw new UserFriendlyException(string
                    .Format("Point can not be less than 0 or greater than 5"));
            }
            if (input.Id <= 0)
            {
                var userInfo = WorkScope.GetAll<User>()
                    .Select(s => new
                    {
                        s.Type,
                        s.Level,
                        s.BranchId,
                        s.Id
                    })
                    .Where(s => s.Id == input.UserId)
                    .FirstOrDefault();
                Logger.Info($"UserInfo: Type - {userInfo.Type}, Level - {userInfo.Level}, BranchId - {userInfo.BranchId}");
                if (userInfo == default) throw new UserFriendlyException(string.Format("This User is Null"));
                if (userInfo.Type == default || userInfo.Level == default || userInfo.BranchId == default) throw new UserFriendlyException("Type or Level or Branch is null");
                var item = ObjectMapper.Map<RetroResult>(input);
                item.UserLevel = userInfo.Level.Value;
                item.UserType = userInfo.Type.Value;
                item.BranchId = userInfo.BranchId.Value;

                await WorkScope.InsertAndGetIdAsync(item);
            }
            return input;
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_Edit)]
        public async Task<RetroResultEditDto> Update(RetroResultEditDto input)
        {
            var item = await WorkScope.GetAsync<RetroResult>(input.Id);
            var userId = item.UserId;
            var projectId = item.ProjectId;

            ObjectMapper.Map<RetroResultEditDto, RetroResult>(input, item);

            item.ProjectId = projectId;
            item.UserId = userId;

            await WorkScope.UpdateAsync(item);
            
            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var item = await WorkScope.GetAsync<RetroResult>(input.Id);
            if (item == null)
            {
                throw new UserFriendlyException(string.Format("There is no entity RetroResult with id = {0}!", input.Id));
            }
            await WorkScope.GetRepo<RetroResult>().DeleteAsync(input.Id);
        }

        private async Task<List<string>> GetListPosition()
        {
            return await WorkScope.GetAll<Position>().Select(x => x.Name).ToListAsync();
        }

        private void FillMetaPosition(ExcelPackage excelPackageIn, List<string> listPositionName)
        {
            var sheet = excelPackageIn.Workbook.Worksheets[1];
            var rowIndex = 2;
            foreach (var positionName in listPositionName)
            {
                sheet.Cells[rowIndex, 1].Value = positionName;
                rowIndex++;
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_DownloadTemplate)]
        public async Task<FileBase64Dto> DownloadTemplateImportRetro()
        {
            var listPositionName = await GetListPosition();

            var templateFilePath = Path.Combine(templateFolder, "TemplateImportRetro.xlsx");

            using (var memoryStream = new MemoryStream(File.ReadAllBytes(templateFilePath)))
            {
                using (var excelPackageIn = new ExcelPackage(memoryStream))
                {
                    FillMetaPosition(excelPackageIn, listPositionName);
                    string fileBase64 = Convert.ToBase64String(excelPackageIn.GetAsByteArray());

                    return new FileBase64Dto
                    {
                        FileName = "Template",
                        FileType = MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet,
                        Base64 = fileBase64
                    };
                }
            }
        }

        #region exportFIle

        private void FillRetroResult(ExcelPackage excelPackageIn, List<RetroResultDto> listRetroResultInput)
        {
            var sheet = excelPackageIn.Workbook.Worksheets[0];
            var rowIndex = 4;

            foreach (var item in listRetroResultInput)
            {
                sheet.Cells[1, 1].Value = item.RetroName;

                sheet.Cells[rowIndex, 1].Value = rowIndex - 3;
                sheet.Cells[rowIndex, 2].Value = item.FullName;
                sheet.Cells[rowIndex, 3].Value = item.EmailAddress;
                sheet.Cells[rowIndex, 4].Value = item.Point;
                sheet.Cells[rowIndex, 5].Value = item.Note;
                sheet.Cells[rowIndex, 6].Value = item.ProjectName;
                sheet.Cells[rowIndex, 7].Value = item.PositionName;
                sheet.Cells[rowIndex, 8].Value = item.Type;
                sheet.Cells[rowIndex, 9].Value = item.Level;
                sheet.Cells[rowIndex, 10].Value = item.BranchName;
                rowIndex++;
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_Export)]
        public async Task<FileBase64Dto> ExportRetroResult(InputMultiFilterRetroResultPagingDto input, long retroId)
        {
            input.GridParam.MaxResultCount = 1000000;
            var listRetroResultInput = GetAllPagging(input, retroId).Result.Items.ToList();

            var templateFilePath = Path.Combine(templateFolder, "ExportRetroResult.xlsx");

            using (var memoryStream = new MemoryStream(File.ReadAllBytes(templateFilePath)))
            {
                using (var excelPackageIn = new ExcelPackage(memoryStream))
                {
                    FillRetroResult(excelPackageIn, listRetroResultInput);
                    string fileBase64 = Convert.ToBase64String(excelPackageIn.GetAsByteArray());

                    return new FileBase64Dto
                    {
                        FileName = "Template",
                        FileType = MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet,
                        Base64 = fileBase64
                    };
                }
            }
        }

        #endregion exportFIle

        #region import

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_Import)]
        public async Task<Object> ImportRetroUserFromFile([FromForm] ImportFileDto input)
        {
            if (input == null)
                throw new UserFriendlyException(String.Format("No file upload!"));

            var path = new String[] { ".xlsx", ".xltx" };
            if (!path.Contains(Path.GetExtension(input.File.FileName)))
                throw new UserFriendlyException(String.Format("Invalid file upload."));

            var failedList = new List<string>();
            var errorList =  new List<string>();
            var listEmailSuccess = new List<string>();
            var listWarning = new List<string>();
            var listRetroResultInsert = new List<RetroResult>();
            List<UpdateRetroResultDto> listRetroResultInput = await ReadFile(input);
            List<string> listInputEmail = listRetroResultInput.Select(x => x.Email.ToLower().Trim()).ToList();

            var listRetroResultInDB = WorkScope.GetAll<RetroResult>()
                .Include(x => x.Project)
                .Where(x => listInputEmail.Contains(x.User.EmailAddress.ToLower().Trim()))
                .Where(s => s.RetroId == input.retroId)
                .Select(s => new
                {
                    s.UserId,
                    s.User.EmailAddress,
                    s.ProjectId,
                    ProjectName = s.Project.Name
                }).ToList();
            var listKeyRetroResultInDB = listRetroResultInDB.Select(x => new {Email = x.EmailAddress, ProjectId =  x.ProjectId}).ToList();

            RetroResult userRetroResult = null;

            var userInfo = WorkScope.GetAll<User>()
                                    .Where(x => listInputEmail.Contains(x.EmailAddress.ToLower().Trim()))
                                    .Select(s => new {
                                        s.Id,
                                        s.Type,
                                        s.Level,
                                        s.BranchId,
                                        s.EmailAddress
                                    });

            var mapEmailToUser = userInfo
                .GroupBy(s => s.EmailAddress)
                .ToDictionary(s => s.Key, s => s.Select(x => new
                {
                    Id = x.Id,
                    Type = x.Type,
                    Level = x.Level,
                    BranchId = x.BranchId,
                }));

            var mapPostionToId = WorkScope.GetAll<Position>()
                .Select(s => new { Postion = s.Name.ToLower().Trim(), s.Id })
                .ToDictionary(s => s.Postion, k => k.Id);

            var emailDuplicate = GetEmailFromExcelFile(listRetroResultInput.Select(s => s.Email));

            if(emailDuplicate.Count() > 0)
            {
                emailDuplicate.ForEach(s =>
                {
                    errorList.Add(s);
                });
            }
            foreach (var item in listRetroResultInput)
            {
                var messageError = new List<string>();
                var messageErrorOther = "";
                if (emailDuplicate.Any(s => s.Contains(item.Email)))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(item.Email))
                {
                    messageError.Add($"Email");
                }
                else if (!mapEmailToUser.ContainsKey(item.Email))
                {
                    messageErrorOther += $"Can not find User by Email: {item.Email}";
 
                }
                if (string.IsNullOrEmpty(item.PositionName))
                {
                    messageError.Add($"Position");

                }
                if (string.IsNullOrEmpty(item.Note))
                {
                    messageError.Add($"Note");
                }

                if (!item.PointF.HasValue)
                {
                    messageError.Add($"Point");
                }
                if (item.PointF < 0 || item.PointF > 5)
                {
                    messageErrorOther += ", Point can not be less than 0 or greater than 5";
                }

                if (!messageError.IsNullOrEmpty() || !messageErrorOther.IsNullOrEmpty())
                {
                    var messageText = $"Row {item.Row}: {messageError.Join(", ")}" + (messageError.IsNullOrEmpty() ? "" : " null ");
                    errorList.Add($"{messageText} {messageErrorOther}");
                }

                if(listKeyRetroResultInDB.Contains( new { Email = item.Email, ProjectId = input.projectId }))
                {
                    failedList.Add(item.Email);
                }
            }

            if (failedList.Count > 0 || errorList.Count > 0)
            {
                return new { listEmailSuccess, listWarning, failedList, errorList };
            }

            var listAlreadyHasRetro = listRetroResultInDB.Where(s => listInputEmail.Contains(s.EmailAddress))
                                       .Select(s => $"{s.EmailAddress} [{s.ProjectName}]" )
                                       .ToList();

            foreach (var item in listAlreadyHasRetro)
            {
                listWarning.Add(item);
            }

            if (listWarning.Count > 0)
            {
                return new { listEmailSuccess, listWarning, failedList, errorList };
            }

                foreach (var retroResultInput in listRetroResultInput)
                {
                    try
                    {
                        var user = mapEmailToUser[retroResultInput.Email.ToLower().Trim()];
                        var userId = user.Select(x => x.Id).First();
                        var branchId = user.Select(x => x.BranchId).FirstOrDefault();
                        var postionId = mapPostionToId[retroResultInput.PositionName.ToLower().Trim()];

                        userRetroResult = new RetroResult
                        {
                            UserId = userId,
                            PositionId = postionId,
                            Point = retroResultInput.PointF.Value ,
                            Note = retroResultInput.Note,
                            ProjectId = input.projectId,
                            RetroId = input.retroId,
                            UserLevel = user.Select(x => x.Level).FirstOrDefault().Value,
                            UserType = user.Select(x => x.Type).FirstOrDefault().Value,
                            BranchId = branchId != 0 ? branchId : null,
                            PmId = input.pmId,
                        };


                        listRetroResultInsert.Add(userRetroResult);

                        listEmailSuccess.Add(retroResultInput.Email);
                    }
                    catch (Exception ex)
                    {
                        throw new UserFriendlyException(ex.Message);
                    }
                }
             await WorkScope.InsertRangeAsync(listRetroResultInsert);
             return new { listEmailSuccess, listWarning, failedList };
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_RetroDetail_Import)]
        public async Task<List<string>> ConfirmImportRetro([FromForm] ImportFileDto input)
        {
            List<UpdateRetroResultDto> listRetroResultInput = await ReadFile(input);
            List<string> listSuccess = new List<string>();
            List<RetroResult> listRetroResult = new List<RetroResult>();
            var userInfo = WorkScope.GetAll<User>()
                                   .Select(s => new {
                                       s.Id,
                                       s.Type,
                                       s.Level,
                                       s.BranchId,
                                       s.EmailAddress
                                   });
            var mapEmailToUser = userInfo
                .GroupBy(s => s.EmailAddress)
                .ToDictionary(s => s.Key, s => s.Select(x => new
                {
                    Id = x.Id,
                    Type = x.Type,
                    Level = x.Level,
                    BranchId = x.BranchId,
                }));

            var mapPostionToId = WorkScope.GetAll<Position>()
                .Select(s => new { Postion = s.Name.ToLower().Trim(), s.Id })
                .ToDictionary(s => s.Postion, k => k.Id);

            foreach (var retroResultInput in listRetroResultInput)
            {
                try
                {
                    var user = mapEmailToUser[retroResultInput.Email.ToLower().Trim()];
                    var userId = user.Select(x => x.Id).First();
                    var branchId = user.Select(x => x.BranchId).FirstOrDefault();
                    var postionId = mapPostionToId[retroResultInput.PositionName.ToLower().Trim()];

                    var userRetroResult = new RetroResult
                    {
                        UserId = userId,
                        PositionId = postionId,
                        Point = retroResultInput.PointF.Value,
                        Note = retroResultInput.Note,
                        ProjectId = input.projectId,
                        RetroId = input.retroId,
                        UserLevel = user.Select(x => x.Level).FirstOrDefault().Value,
                        UserType = user.Select(x => x.Type).FirstOrDefault().Value,
                        BranchId = branchId != 0 ? branchId : null,
                        PmId = input.pmId,
                    };

                    var excelKey = new { Email = retroResultInput.Email, ProjectId = input.projectId };

                    listRetroResult.Add(userRetroResult);

                    listSuccess.Add(retroResultInput.Email);

                   await WorkScope.InsertRangeAsync(listRetroResult);
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException(ex.Message);
                }
            }
            return listSuccess;

        }

        private List<string> GetEmailFromExcelFile(IEnumerable<string> emails)
        {
            return emails.Where(x => !x.IsNullOrEmpty())
                .GroupBy(s => s)
                          .Select(s => new
                          {
                              s.Key,
                              Count = s.Count()
                          })
                          .Where(s => s.Count > 1)
                          .Select(s => $"Email: {s.Key} lặp lại {s.Count} lần")
                          .ToList();
        }

        private async Task<List<UpdateRetroResultDto>> ReadFile([FromForm] ImportFileDto input)
        {
            List<UpdateRetroResultDto> updateRetroResult = new List<UpdateRetroResultDto>();
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
                            var rowData = new UpdateRetroResultDto
                            {
                                Row = row,
                                Email = worksheet.Cells[row, 1].Value.GetValueFromObject().Trim(),
                                PositionName = worksheet.Cells[row, 2].Value.GetValueFromObject().Trim(),
                                Point = worksheet.Cells[row, 3].Value.GetValueFromObject().Trim(),
                                Note = worksheet.Cells[row, 4].Value.GetValueFromObject().Trim(),
                            };
                            if (rowData.IsEmpty)
                                continue;

                            updateRetroResult.Add(rowData);
                        }
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                    
                }
            }
            return updateRetroResult;
        }

        #endregion import

        public async Task<List<GetRetroUserInfoDtocs>> GetRetroUserInfo()
        {

            var qmtsOfIntern = WorkScope.GetAll<MyTimesheet>()
              .Where(s => s.CreationTime > new DateTime(2022, 6, 1))
              .Where(s => s.Status >= TimesheetStatus.Pending)
              .Where(s => s.User.IsActive && !s.User.IsStopWork)
              .Where(s => !s.ProjectTask.Project.isAllUserBelongTo);

            var qmts = qmtsOfIntern
                .Select(s => new
                {
                    s.UserId,
                    s.Id,
                    s.WorkingTime,
                    ProjectCode = s.ProjectTask.Project.Code
                })
                .AsNoTracking().ToList()
                .GroupBy(x => x.UserId)
                .Select(x => new {
                    x.Key,
                    HighestWorkingProjectCode = x.GroupBy(s => s.ProjectCode).Select(z => new
                    {
                        z.Key,
                        WorkingTime = z.Sum(p => p.WorkingTime)
                    }).OrderByDescending(s => s.WorkingTime).Select(z => z.Key).FirstOrDefault()
                }).ToDictionary(x => x.Key, x=> x.HighestWorkingProjectCode);

            var ListPMFromProject = _projectService.GetProjectPMName();

            var Project = WorkScope.GetAll<Project>()
                .ToList();

            var projectUserInfo = WorkScope.GetAll<User>()
                .Where(x => qmts.ContainsKey(x.Id))
                .Select(x => new GetRetroUserInfoDtocs
                {
                    FirstName = x.Surname,
                    LastName = x.Name,
                    Email = x.EmailAddress,
                    ProjectName = Project.Where(p => p.Code == qmts[x.Id]).Select(s => s.Name).FirstOrDefault(),
                    PMEmail = ListPMFromProject.Where(s => s.ProjectCode == qmts[x.Id]).FirstOrDefault().PMEmail
                }).ToList();

            return projectUserInfo;
        }
    }


}