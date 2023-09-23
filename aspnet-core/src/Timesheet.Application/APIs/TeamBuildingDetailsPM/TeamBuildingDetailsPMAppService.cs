using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Branchs.Dto;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.APIs.TeamBuildingDetailsPM.dto;
using Timesheet.APIs.TeamBuildingDetailsPM.Dto;
using Timesheet.APIs.TeamBuildingRequestMoney.dto;
using Timesheet.Constants;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Uitls;
using Timesheet.UploadFilesService;

namespace Timesheet.APIs.TeamBuildingDetailsPM
{
    public class TeamBuildingDetailsPMAppService : AppServiceBase
    {
        public static ProjectService _projectService;
        public UploadTeamBuildingService _uploadTeamBuildingService;
        private readonly KomuService _komuService;
        public TeamBuildingDetailsPMAppService(UploadTeamBuildingService uploadTeamBuildingService, ProjectService projectService, KomuService komuService, IWorkScope workScope) : base(workScope)
        {
            _projectService = projectService;
            _uploadTeamBuildingService = uploadTeamBuildingService;
            _komuService = komuService;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailPM_ViewMyProject)]
        public async Task<GridResult<GetTeamBuildingDetailDto>> GetAllPagging(InputFilterTeamBuildingDetailPagingDto input)
        {
            long currentUserId = AbpSession.UserId.Value;

            List<long> listProjectIdByPmId = GetAllProjectValidByPM(currentUserId).Select(s => s.Id).ToList();
            if (listProjectIdByPmId.IsEmpty())
                throw new UserFriendlyException("No project found");

            var pmFromProject = _projectService.GetProjectPMName();

            var dicUsers = WorkScope.GetAll<User>()
                                    .Select(s => new
                                    {
                                        s.Id,
                                        s.FullName,
                                    }).ToDictionary(s => s.Id, s => s.FullName);

            var qTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                 .WhereIf(input.Status.HasValue, s => s.Status == input.Status.Value)
                 .Where(s => s.ApplyMonth.Year == input.Year)
                 .WhereIf(input.Month.HasValue && input.Month != -1, s => s.ApplyMonth.Month == input.Month)
                 .Where(s => listProjectIdByPmId.Contains(s.ProjectId))
                .Select(s => new GetTeamBuildingDetailDto
                {
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                    ProjectCode = s.Project.Code,
                    EmployeeId = s.EmployeeId,
                    PMEmailAddress = pmFromProject.Where(x => x.ProjectCode == s.Project.Code).Select(x => x.PMEmail).FirstOrDefault(),
                    Id = s.Id,
                    EmployeeFullName = s.Employee.FullName,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    RequesterId = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.RequesterId : default,
                    RequesterEmailAddress = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.EmailAddress : default,
                    RequesterFullName = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.FullName : default,
                    LastModifierTime = s.LastModificationTime,
                    CreationTime = s.CreationTime,
                    CreatedUserName = s.CreatorUserId.HasValue && dicUsers.ContainsKey(s.CreatorUserId.Value) ? dicUsers[s.CreatorUserId.Value] : "",
                    LastModifierUserName = s.LastModifierUserId.HasValue && dicUsers.ContainsKey(s.LastModifierUserId.Value) ? dicUsers[s.LastModifierUserId.Value] : "",
                    Status = s.Status,
                    Money = s.Money,
                    ApplyMonth = s.ApplyMonth
                })
                .OrderBy(s => s.CreationTime)
                .OrderBy(s => s.ApplyMonth.Month)
                .OrderBy(s => s.EmployeeEmailAddress);

            return await qTeamBuildingDetail.GetGridResult(qTeamBuildingDetail, input.GridParam);
        }

        public List<Project> GetAllProjectValidByPM(long PmId)
        {
            var listProjectId = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == PmId)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => s.ProjectId)
                .ToList();
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var ListProject = WorkScope.GetAll<Project>()
                     .Where(s => listProjectId.Contains(s.Id))
                     .Where(s => s.IsDeleted == false)
                     .Where(s => s.IsAllowTeamBuilding == true)
                     .ToList();
                return ListProject;
            }
        }

        [HttpGet]
        public async Task<List<GetAllProjectInTeamBuildingDetailDto>> GetAllProjectInTeamBuildingDetailPM()
        {
            long currentUserId = AbpSession.UserId.Value;
            var listProjectIdByPmId = GetAllProjectValidByPM(currentUserId).Select(s => s.Id).ToList();

            var listProjectIdInDetail = WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => listProjectIdByPmId.Contains(s.ProjectId))
                .Select(s => s.ProjectId)
                .Distinct()
                .ToList();

            return await WorkScope.GetAll<Project>().Where(s => listProjectIdInDetail.Contains(s.Id)).Select(s => new GetAllProjectInTeamBuildingDetailDto
            {
                ProjectId = s.Id,
                ProjectName = s.Name
            }).ToListAsync();
        }

        [HttpGet]
        public async Task<List<GetAllRequesterEmailAddressInTeamBuildingDetailDto>> GetAllRequesterEmailAddressInTeamBuildingDetailPM()
        {
            var currentUserId = AbpSession.UserId.Value;
            var listProjectId = GetAllProjectValidByPM(currentUserId).Select(s => s.Id).ToList();

            var listRequest = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistoryId != null)
                .Where(s => listProjectId.Contains(s.ProjectId))
              .Select(s => s.TeamBuildingRequestHistoryId).Distinct().ToListAsync();

            return await WorkScope.GetAll<TeamBuildingRequestHistory>().Where(s => listRequest.Contains(s.Id))
                .Select(s => new GetAllRequesterEmailAddressInTeamBuildingDetailDto
                {
                    RequesterEmailAddress = s.Requester.EmailAddress,
                    RequesterId = s.RequesterId
                }).Distinct().ToListAsync();
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailPM_CreateRequest)]
        public async Task<SelectDetailPopupDto> GetRequestMoney(long? projectId = null, int? month = null, long? branchId = null)
        {
            var userId = AbpSession.UserId.Value;

            var listProjectIdByUserId = await WorkScope.GetAll<ProjectUser>()
               .Where(s => s.UserId == userId)
               .Where(s => s.Type == ProjectUserType.PM)
               .WhereIf(projectId.HasValue, s => s.ProjectId == projectId)
               .Select(s => s.ProjectId)
               .ToListAsync();

            var listProject = await WorkScope.GetAll<Project>()
                .Where(s => listProjectIdByUserId.Contains(s.Id))
                .Where(s => s.IsDeleted == false && s.IsAllowTeamBuilding == true)
                .Select(s => s.Id)
                .ToListAsync();

            var listUser = await WorkScope.GetAll<TeamBuildingDetail>()
                .Include(s => s.Employee)
                .Where(s => listProject.Contains(s.ProjectId))
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Open || s.Status == StatusEnum.TeamBuildingStatus.Requested)
                .WhereIf(month.HasValue, s => s.ApplyMonth.Month == month)
                .WhereIf(branchId.HasValue, s => s.Employee.BranchId == branchId)
                .Select(s => new SelectTeamBuildingDetailDto
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    EmployeeFullName = s.Employee.FullName,
                    ProjectId = s.Project.Id,
                    ProjectName = s.Project.Name,
                    BranchName = s.Employee != null && s.Employee.Branch != null ? s.Employee.Branch.Name : "",
                    BranchColor = s.Employee.Branch.Color,
                    BranchId = s.Employee.BranchId,
                    Money = s.Money,
                    CreationTime = s.CreationTime,
                    Status = s.Status,
                    ApplyMonth = s.ApplyMonth,
                    RequesterEmailAddress = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.EmailAddress : default,
                    RequesterFullName = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.FullName : default,
                })
                .OrderByDescending(x => x.Status).ThenBy(x => x.ApplyMonth.Month)
                .ToListAsync();

            var lastRequestHistory = await WorkScope.GetAll<TeamBuildingRequestHistory>()
                  .Where(s => s.RequesterId == userId)
                  .Where(s => s.Status == StatusEnum.TeamBuildingRequestStatus.Done)
                  .OrderByDescending(s => s.CreationTime)
                  .FirstOrDefaultAsync();

            float lastRequestRemainingMoney = 0;

            if (lastRequestHistory != null && lastRequestHistory.RemainingMoneyStatus == StatusEnum.RemainingMoneyStatus.Remaining)
            {
                lastRequestRemainingMoney = lastRequestHistory.RemainingMoney ?? 0;
            }

            return new SelectDetailPopupDto
            {
                TeamBuildingDetailDtos = listUser,
                LastRemainMoney = lastRequestRemainingMoney
            };
        }

        [HttpPost]
        public async Task<List<GetAllUserRequestMoneyDto>> GetUserNotPaggingRequestMoney(InputGetUserOtherProjectDto input)
        {
            var listUserIdByTeamBuildingDetail = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(x => !input.Ids.Contains(x.EmployeeId))
                .Where(x => x.Employee.IsActive)
                .Where(x => x.Status != StatusEnum.TeamBuildingStatus.Done)
                .WhereIf(input.BranchId != null, x => x.Employee.BranchId == input.BranchId)
                .WhereIf(input.SearchText != null, x => x.Employee.FullName.Contains(input.SearchText) || x.Employee.EmailAddress.Contains(input.SearchText))
                .Select(x => x.EmployeeId).Distinct().ToListAsync();

            var listInfoUser = await WorkScope.GetAll<User>()
                .Where(s => listUserIdByTeamBuildingDetail.Contains(s.Id))
                .Select(s => new GetAllUserRequestMoneyDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    EmailAddress = s.EmailAddress,
                    BranchId = s.BranchId,
                    BranchName = s.Branch.Name,
                    BranchColor = s.Branch.Color,
                }).OrderBy(s => s.EmailAddress).Distinct().ToListAsync();

            return listInfoUser;
        }

        [HttpPost]
        public async Task<List<SelectTeamBuildingDetailDto>> GetRequestMoneyInfoUser(List<long> listEmpIds)
        {
            var userId = AbpSession.UserId.Value;

            var listProjectIdByUserId = await WorkScope.GetAll<ProjectUser>()
               .Where(s => s.UserId == userId)
               .Where(s => s.Type == ProjectUserType.PM)
               .Select(s => s.ProjectId)
               .ToListAsync();

            return await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => listEmpIds.Contains(s.EmployeeId))
                .Where(s => !listProjectIdByUserId.Contains(s.ProjectId))
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Open || s.Status == StatusEnum.TeamBuildingStatus.Requested)
                .OrderBy(s => s.Employee.FullName)
                .OrderBy(s => s.ApplyMonth)
                .Select(s => new SelectTeamBuildingDetailDto
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    EmployeeFullName = s.Employee.FullName,
                    ProjectId = s.Project.Id,
                    ProjectName = s.Project.Name,
                    BranchName = s.Employee != null && s.Employee.Branch != null ? s.Employee.Branch.Name : "",
                    BranchColor = s.Employee.Branch.Color,
                    BranchId = s.Employee.BranchId,
                    Money = s.Money,
                    CreationTime = s.CreationTime,
                    Status = s.Status,
                    ApplyMonth = s.ApplyMonth,
                    RequesterEmailAddress = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.EmailAddress : default,
                    RequesterFullName = s.TeamBuildingRequestHistoryId.HasValue ? s.TeamBuildingRequestHistory.Requester.FullName : default,
                })
                .OrderByDescending(x => x.Status).ThenBy(x => x.EmployeeFullName).ThenBy(x => x.ApplyMonth)
                .ToListAsync();
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_DetailPM_CreateRequest)]
        public async Task<ReturnRequestHistoryDto> SubmitRequestMoney([FromForm] SubmitRequestMoneyDto input)
        {   
            var userId = AbpSession.UserId.Value;
            float totalMoney = 0;
            PMRequestDto pMRequestDto = new PMRequestDto();
            if (!input.PMRequest.IsNullOrEmpty())
            {
                try
                {
                    pMRequestDto = JsonConvert.DeserializeObject<PMRequestDto>(input.PMRequest);
                } catch (Exception ex)
                {
                    throw new JsonSerializationException("Request DTO Format is Invalid!");
                }
                
            } else
            {
                throw new UserFriendlyException("Request is Invalid!");
            }
            
            var strTeamBuildingMoney = SettingManager.GetSettingValueForApplication(AppSettingNames.TeamBuildingMoney);

            var listDetail = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => pMRequestDto.ListDetailId.Contains(s.Id))
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Open)
                .ToListAsync();

            listDetail.ForEach(
                s =>
                {
                    totalMoney += s.Money;
                    s.Status = StatusEnum.TeamBuildingStatus.Requested;
                    WorkScope.UpdateAsync(s);
                });

            if (totalMoney == 0)
            {
                Logger.Info("SubmitRequestMoney totalMoney = 0 with input data: " + JsonConvert.SerializeObject(input));
                throw new UserFriendlyException("You need to select at least one record!");
            }


            if (pMRequestDto.ListInvoiceRequestDto.IsNullOrEmpty())
                throw new UserFriendlyException("Invoice Request cannot be null!");

            List<InvoiceRequestDto> invoiceRequestDtos = pMRequestDto.ListInvoiceRequestDto;

            foreach (InvoiceRequestDto invoiceRequest in invoiceRequestDtos)
            {
                if (invoiceRequest.InvoiceUrl.IsNullOrEmpty() && invoiceRequest.InvoiceImageName.IsNullOrEmpty())
                {
                    throw new UserFriendlyException("One invoice request must have invoice file or invoice url!");
                }
            }

            if (pMRequestDto.ListInvoiceRequestDto != null)
            {
                pMRequestDto.ListInvoiceRequestDto.ForEach(item =>
                {
                    if(!item.InvoiceUrl.IsNullOrEmpty())
                    {
                        UrlUtils.CheckValidUrl(item.InvoiceUrl);
                    }
                    //UrlUtils.CheckValidUrlFileContentType(url, ConstantTeamBuildingFile.AllowFileTypes);
                });
            }

            if (input.ListFile != null)
            {
                input.ListFile.ForEach(file =>
                {
                    FileUtils.CheckValidFile(file, ConstantTeamBuildingFile.AllowFileTypes);
                });
            }

            // Bỏ chặn cho phép PM tạo Request mới, khi họ đang có Request Pending
            //if (HaveRequestPending(userId))
            //    throw new UserFriendlyException("Already have a pending request");

            var userInfo = await WorkScope.GetAll<User>()
                .Where(s => s.Id == userId)
                .Select(s => new
                {
                    s.FullName,
                    BranchName = s.Branch.Name
                })
                .FirstOrDefaultAsync();

            var listProjectIdByUserId = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => s.ProjectId)
                .ToListAsync();

            var listProject = await WorkScope.GetAll<Project>()
                .Where(s => listProjectIdByUserId.Contains(s.Id))
                .Where(s => s.IsDeleted == false && s.IsAllowTeamBuilding == true)
                .Select(s => s.Id)
                .ToListAsync();

            var LastRequestHistory = GetLastRequestHistoryDoneByPMId(userId);

            if (LastRequestHistory != null && LastRequestHistory.RemainingMoneyStatus == StatusEnum.RemainingMoneyStatus.Remaining)
            {
                totalMoney += LastRequestHistory?.RemainingMoney ?? 0;
            }

            if(totalMoney > (pMRequestDto.InvoiceAmount + float.Parse(strTeamBuildingMoney)))
            {
                throw new UserFriendlyException($"Total money greater than in Invoice amount {float.Parse(strTeamBuildingMoney)} VNĐ");
            }

            //create new history
            TeamBuildingRequestHistory history = new TeamBuildingRequestHistory
            {
                RequesterId = userId,
                RequestMoney = totalMoney,
                TitleRequest = userInfo.FullName + " - " + userInfo.BranchName,
                RemainingMoneyStatus = StatusEnum.RemainingMoneyStatus.Remaining,
                Status = StatusEnum.TeamBuildingRequestStatus.Pending,
                Note = pMRequestDto.Note,
                InvoiceAmount = pMRequestDto.InvoiceAmount,
            };

            long historyId = WorkScope.InsertAndGetId(history);

            //create request history file

            if (input.ListFile != null)
            {   
                // each new url is corresponding to old url in the original order
                Dictionary<String, String> mapOldNameWithNewNameDictionary = new Dictionary<string, string>(); 
                mapOldNameWithNewNameDictionary = _uploadTeamBuildingService.UploadMultipleInvoiceFileTeamBuildingAsync(input.ListFile).Result;
                foreach(KeyValuePair<String, String> entry in mapOldNameWithNewNameDictionary)
                {
                    InvoiceRequestDto invoice = new InvoiceRequestDto();
                    invoice = invoiceRequestDtos.FirstOrDefault(item => item.InvoiceImageName != null ? item.InvoiceImageName.Equals(entry.Key) : false);
                    if(invoice != null) invoice.InvoiceImageName = entry.Value;
                }
            }

            List<TeamBuildingRequestHistoryFile> listRequestHistoryFile = new List<TeamBuildingRequestHistoryFile>();
            foreach (var invoiceRequestDto in invoiceRequestDtos)
            {
                TeamBuildingRequestHistoryFile requestHistoryFile = new TeamBuildingRequestHistoryFile
                {
                    Url = !invoiceRequestDto.InvoiceUrl.IsNullOrEmpty() ? invoiceRequestDto.InvoiceUrl : invoiceRequestDto.InvoiceImageName,
                    FileName = !invoiceRequestDto.InvoiceImageName.IsNullOrEmpty() ? Path.GetFileName(invoiceRequestDto.InvoiceImageName) : Path.GetFileName(invoiceRequestDto.InvoiceUrl),
                    TeamBuildingRequestHistoryId = historyId,
                    InvoiceAmount = invoiceRequestDto.Amount,
                    IsVAT = invoiceRequestDto.HasVat

                };
                await WorkScope.InsertAsync(requestHistoryFile);
                listRequestHistoryFile.Add(requestHistoryFile);
            }
            //update foreign key

            listDetail.ForEach(
                s =>
                {
                    s.TeamBuildingRequestHistoryId = historyId;
                    WorkScope.UpdateAsync(s);
                }
                );

            var listUserId = listDetail.Select(s => s.Id).ToList();

            NotifyPMSelectTeamBuildingDetail(listUserId);

            return new ReturnRequestHistoryDto
            {
                Id = history.Id,
                RequesterId = history.RequesterId,
                RequestMoney = history.RequestMoney,
                TitleRequest = history.TitleRequest,
                DisbursedMoney = history.DisbursedMoney,
                RemainingMoney = history.RemainingMoney,
                RemainingMoneyStatus = history.RemainingMoneyStatus,
                Status = history.Status,
                TeamBuildingRequestHistoryFileDtos = listRequestHistoryFile.Select(s => s.MapTo<TeamBuildingRequestHistoryFileDto>()).ToList()
            };
        }

        public TeamBuildingRequestHistory GetLastRequestHistoryDoneByPMId(long PmId)
        {
            return WorkScope.GetAll<TeamBuildingRequestHistory>()
                  .Where(s => s.RequesterId == PmId)
                  .Where(s => s.Status == StatusEnum.TeamBuildingRequestStatus.Done)
                  .OrderByDescending(s => s.CreationTime)
                  .FirstOrDefault();
        }

        //private bool HaveRequestPending(long PmId)
        //{
        //    return WorkScope.GetAll<TeamBuildingRequestHistory>()
        //             .Where(s => s.RequesterId == PmId)
        //             .Any(s => s.Status == StatusEnum.TeamBuildingRequestStatus.Pending);
        //}

        [HttpGet]
        public async Task<List<BranchDto>> GetAllBranchTeamBuildingFilter()
        {
            var branchId = await WorkScope.GetAll<Branch>().Select(s => s.Id).FirstOrDefaultAsync();
            var query = await WorkScope.GetAll<Branch>()
                 .Select(s => new BranchDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     DisplayName = s.DisplayName
                 }).ToListAsync();
            return query.OrderBy(s => s.Id).ToList();
        }

        public void NotifyPMSelectTeamBuildingDetail(List<long> listTeamBuildingDetaild)
        {
            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRToChannels);
            if (string.IsNullOrEmpty(notifyToChannels))
            {
                throw new UserFriendlyException("config App.SendMessageRequestPendingTeamBuildingToHRToChannels is null or empty, ");
            }

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            if (countListChannel <= 0)
            {
                throw new UserFriendlyException($"The list of notify channels is null or empty. Please contact the administrator to check the config");
            }

            long currentUserId = AbpSession.UserId.Value;

            var projectPMByCurrentUserIds = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == currentUserId)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new
                {
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                }).Distinct().ToList();

            var listTBDOtherProject = WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => listTeamBuildingDetaild.Contains(s.Id))
                .Where(s => !s.Project.isAllUserBelongTo)
                .Where(s => !projectPMByCurrentUserIds.Select(x => x.ProjectId).Contains(s.ProjectId))
                .Select(s => new
                {
                    Id = s.Id,
                    UserName = s.Employee.UserName,
                    UserId = s.EmployeeId,
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                    Money = s.Money,
                    Month = s.ApplyMonth.Month,
                    Year = s.ApplyMonth.Year,
                }).Distinct().ToList();

            var dicProjectIdToPms = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Where(s => listTBDOtherProject.Select(p => p.ProjectId).Distinct().Contains(s.ProjectId))
                .GroupBy(s => s.ProjectId)
                .Select(s => new
                {
                    ProjectId = s.Key,
                    Pms = s.Select(pm => new PmInfoTeambuildingDto
                    {
                        PmEmailAddress = pm.User.EmailAddress,
                        KomuPmId = pm.User.KomuUserId,
                    }).ToList(),
                })
                .ToDictionary(s => s.ProjectId, s => s.Pms);

            var listDataByProject = listTBDOtherProject.GroupBy(s => s.ProjectId)
                .Select(s => new
                {
                    ProjectPms = dicProjectIdToPms.ContainsKey(s.Key) ? dicProjectIdToPms[s.Key] : null,
                    Users = s.Select(u => new TeamBuildingDetailDto
                    {
                        EmployeeId = u.UserId,
                        EmployeeName = u.UserName,
                        Money = u.Money,
                        Month = u.Month,
                        Year = u.Year,
                    }).ToList(),
                }).ToList();

            var listUser = WorkScope.GetAll<User>()
                .Select(s => new
                {
                    s.IsActive,
                    s.Id,
                    s.KomuUserId,
                    s.EmailAddress,
                })
                .Where(s => s.IsActive)
                .ToList();

            var dicUserIdToKomuId = new Dictionary<long, ulong?>();

            var dicUserIdToEmailAddress = new Dictionary<long, string>();

            listUser.ForEach(item =>
            {
                dicUserIdToKomuId.Add(item.Id, item.KomuUserId);
                dicUserIdToEmailAddress.Add(item.Id, item.EmailAddress);
            });

            var listResult = new NotifyPMRequestingTeamBuildingDto
            {
                KomuPmRequestId = dicUserIdToKomuId[currentUserId],
                PmRequestEmailAddress = dicUserIdToEmailAddress[currentUserId],
                ProjectInfos =  listDataByProject.Select(s => new ProjectInfoTeamBuildingDto
                {
                    PmInfos = s.ProjectPms,
                    Users = s.Users
                }).ToList(),
            };

            var sb = new StringBuilder();
            foreach (var item in listResult.ProjectInfos)
            {
                foreach(var item2 in item.PmInfos) {
                    sb.AppendLine($"{item2.KomuAccountTag()}");
                }

                if(item.Users.Count == 1)
                {
                    sb.AppendLine($"PM {listResult.KomuAccountTagRequester()} is requesting teambuilding for **{item.Users.Count}** member from your team:");
                }
                else
                {
                    sb.AppendLine($"PM {listResult.KomuAccountTagRequester()} is requesting teambuilding for **{item.Users.Count}** members from your team:");
                }

                sb.AppendLine($"```");

                foreach (var item3 in item.Users)
                {
                    sb.AppendLine($"User: {item3.EmployeeName} - Money: {item3.Money} VNĐ ({item3.Month}-{item3.Year})");
                }

                sb.AppendLine($"```");

                if (countListChannel > 0)
                {
                    for (var i = 0; i < countListChannel; i++)
                    {
                        _komuService.NotifyToChannel(sb.ToString(), arrListChannel[i].Trim());
                    }
                }
                sb.Clear();
            }
        }
    }
}
