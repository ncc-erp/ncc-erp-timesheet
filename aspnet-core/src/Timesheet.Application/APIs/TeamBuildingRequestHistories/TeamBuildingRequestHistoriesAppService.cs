using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.TeamBuildingDetailsPM.dto;
using Timesheet.APIs.TeamBuildingDetailsPM.Dto;
using Timesheet.APIs.TeamBuildingRequestHistories.dto;
using Timesheet.APIs.TeamBuildingRequestMoney.dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingRequestHistories
{
    public class TeamBuildingRequestHistoriesAppService : AppServiceBase
    {
        public TeamBuildingRequestHistoriesAppService(IWorkScope workScope) : base(workScope) { }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_ViewAllRequest, Ncc.Authorization.PermissionNames.TeamBuilding_Request_ViewMyRequest)]
        public async Task<GridResult<TeamBuildingRequestHistoryDto>> GetAllPagging(InputMultiFilterRequestHistoryDto input)
        {
            var isVewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.TeamBuilding_Request_ViewAllRequest);
            var myRequest = await GetEmailPMInRequest();
            var myRequestPMId = myRequest.Select(s => s.RequesterId).ToList();

            var requestHistoryQuery = WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => !input.Status.HasValue || s.Status == input.Status)
                .Where(s => s.CreationTime.Year == input.Year)
                .WhereIf(input.Month.HasValue && input.Month != -1, s => s.CreationTime.Month == input.Month)
                .WhereIf(!isVewAll, s => myRequestPMId.Contains(s.RequesterId))
                .Select(s => new TeamBuildingRequestHistoryDto
                {
                    Id = s.Id,
                    TitleRequest = s.TitleRequest,
                    RequesterId = s.RequesterId,
                    FullNameRequester = s.Requester.FullName,
                    EmailRequester = s.Requester.EmailAddress,
                    RequestMoney = s.RequestMoney,
                    DisbursedMoney = s.DisbursedMoney,
                    RemainingMoney = s.RemainingMoney,
                    RemainingMoneyStatus = s.RemainingMoneyStatus,
                    Status = s.Status,
                    CreationTime = s.CreationTime,
                    InvoiceAmount = s.InvoiceAmount,
                    VATMoney = s.VATMoney,
                })
                .OrderByDescending(item => item.CreationTime)
                .ThenBy(item => item.Status == TeamBuildingRequestStatus.Pending ? 0 : 1)
                .ThenBy(item => item.CreationTime);
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return await requestHistoryQuery.GetGridResult(requestHistoryQuery, input.GridParam);
            }
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_DisburseRequest)]
        public async Task<DisburseTeamBuildingRequestInfoDto> GetTeamBuildingRequestForDisburse(long teamBuildingRequestId, long requesterId)
        {
            var disburseRequestHistoryInfo = await WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Include(item => item.Requester)
                .Where(s => s.Id == teamBuildingRequestId)
                .Select(s => new DisburseTeamBuildingRequestInfoDto
                {
                    RequestId = s.Id,
                    RequesterEmail = s.Requester.EmailAddress,
                    RequesterName = s.Requester.Name,
                    RequestMoney = s.RequestMoney
                }).FirstOrDefaultAsync();

            var invoiceRequestsFordisburseRequestHistoryInfo = WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(s => s.TeamBuildingRequestHistoryId == teamBuildingRequestId)
                .Select(s => new InvoiceRequestOfDisburseTeamBuidingRequestDto
                {
                    InvoiceId = s.Id,
                    InvoiceResourceName = s.FileName,
                    InvoiceResourceUrl = UrlUtils.FormatUrlWithFullDomain(s.Url),
                    InvoiceMoney = s.InvoiceAmount,
                    HasVAT = s.IsVAT
                }).ToList();
            disburseRequestHistoryInfo.InvoiceRequests = invoiceRequestsFordisburseRequestHistoryInfo;

            var remainingMoney = WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == requesterId
                         && s.RemainingMoneyStatus == RemainingMoneyStatus.Remaining
                         && s.RemainingMoney.HasValue)
                .OrderByDescending(s => s.CreationTime)
                .Select(s => s.RemainingMoney)
                .FirstOrDefault();
            disburseRequestHistoryInfo.RemainingMoney = remainingMoney;

            return disburseRequestHistoryInfo;
        }

        [HttpGet]
        public async Task<List<GetAllEmailRequesterInRequestDto>> GetEmailPMInRequest()
        {
            var userId = AbpSession.UserId.Value;

            var isVewAll = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.TeamBuilding_Request_ViewAllRequest);
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return await WorkScope.GetAll<TeamBuildingRequestHistory>()
                        .WhereIf(!isVewAll, s => s.RequesterId == userId)
                        .Select(s => new GetAllEmailRequesterInRequestDto
                        {
                            RequesterId = s.RequesterId,
                            EmailAddress = s.Requester.EmailAddress,
                        })
                        .Distinct()
                        .OrderBy(s => s.EmailAddress)
                        .ToListAsync();
            }
        }

        private async Task<TeamBuildingRequestHistory> GetTeamBuildingRequestById(long requestId)
        {
            return await WorkScope.GetAsync<TeamBuildingRequestHistory>(requestId);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_DisburseRequest)]
        public async System.Threading.Tasks.Task DisburseRequest(DisburseTeamBuildingRequestDto input)
        {
            var request = await GetTeamBuildingRequestById(input.RequestId);
            var listRequestByRequesterId = WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == input.RequesterId).ToList();
            var listRequestByRequesterIdWithoutLast = listRequestByRequesterId.Take(listRequestByRequesterId.Count() - 1).ToList();
            var listInvoice = WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(s => s.TeamBuildingRequestHistoryId == input.RequestId)
                .Select(s => new TeamBuildingRequestHistoryFileDTO
                {
                    InvoiceAmount= s.InvoiceAmount,
                    IsVAT= s.IsVAT
                }).ToList();

            if (input.InvoiceDisburseList.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Please provide the invoice data!");
            }
            else if (input.DisburseMoney <= 0)
            {
                throw new UserFriendlyException("The requested disbursement amount is greater than 0!");
            }
            else if (request.Status == TeamBuildingRequestStatus.Cancelled)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been cancelled", input.RequestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Rejected)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been rejected", input.RequestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Done)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been disbursed", input.RequestId));
            }
            else if (request.RequestMoney < input.DisburseMoney)
            {
                throw new UserFriendlyException(string.Format("Disbursement money cannot be more than request money"));
            }
            else
            {

                float VAT = GetVATConfig() / 100f;
                float totalVAT = 0;
                float totalInvoiceAmountNoVAT = 0;
                float totalInvoiceAmountVAT = 0;

                foreach (var item in listInvoice)
                {
                    if (item.IsVAT == false && item.InvoiceAmount.HasValue)
                    {
                        totalVAT += item.InvoiceAmount.Value * VAT;
                        totalInvoiceAmountNoVAT += item.InvoiceAmount.Value;
                    }
                }
                totalInvoiceAmountVAT = request.InvoiceAmount.Value - totalInvoiceAmountNoVAT;

                float InvoiceAmountAfterTaxing = (float)(totalInvoiceAmountNoVAT * (1 + VAT)) + totalInvoiceAmountVAT;

                if (request.RequestMoney > InvoiceAmountAfterTaxing)
                {
                    request.DisbursedMoney = input.DisburseMoney;
                    request.VATMoney = totalVAT;
                    request.RemainingMoney = request.RequestMoney - InvoiceAmountAfterTaxing;
                    request.RemainingMoneyStatus = RemainingMoneyStatus.Remaining;
                    request.Status = TeamBuildingRequestStatus.Done;
                }
                else
                {
                    float totalRequestMoneyForNoVAT = request.RequestMoney - totalInvoiceAmountVAT;
                    request.DisbursedMoney = input.DisburseMoney;
                    request.VATMoney = (totalVAT == 0 || totalInvoiceAmountVAT > request.RequestMoney) ? 0: (float)(totalRequestMoneyForNoVAT - totalRequestMoneyForNoVAT / (1 + VAT)); 
                    request.RemainingMoney = 0;
                    request.RemainingMoneyStatus = RemainingMoneyStatus.Done;
                    request.Status = TeamBuildingRequestStatus.Done;
                }

                var listTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                    .Where(s => s.TeamBuildingRequestHistoryId == input.RequestId)
                    .Where(s => s.Status == TeamBuildingStatus.Requested)
                    .ToList();

                foreach (var item in listTeamBuildingDetail)
                {
                    item.Status = TeamBuildingStatus.Done;

                    await WorkScope.UpdateAsync(item);
                }
            }

            if (!listRequestByRequesterIdWithoutLast.IsNullOrEmpty())
            {
                foreach (var item in listRequestByRequesterIdWithoutLast)
                {
                    if (item.RemainingMoneyStatus == RemainingMoneyStatus.Remaining)
                    {
                        item.RemainingMoneyStatus = RemainingMoneyStatus.Done;
                    }
                    await WorkScope.UpdateAsync(item);
                }
            }

            await WorkScope.UpdateAsync(request);
            // Saving VAT status for each Invoice Request in Team Building Request
            foreach (var item in input.InvoiceDisburseList)
            {
                var invoiceRequest = await WorkScope.GetAsync<TeamBuildingRequestHistoryFile>(item.InvoiceId);
                if (invoiceRequest != null)
                {
                    invoiceRequest.IsVAT = item.HasVAT;
                    await WorkScope.UpdateAsync<TeamBuildingRequestHistoryFile>(invoiceRequest);
                }
            }
        }


        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_RejectRequest)]
        public async System.Threading.Tasks.Task RejectRequest(long requestId)
        {
            var request = await GetTeamBuildingRequestById(requestId);
            if (request.Status == TeamBuildingRequestStatus.Cancelled)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been cancelled", requestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Rejected)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been rejected", requestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Done)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been disbursed", requestId));
            }
            else
            {
                request.Status = TeamBuildingRequestStatus.Rejected;
                request.RemainingMoneyStatus = RemainingMoneyStatus.Done;

                var qTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                    .Where(s => s.TeamBuildingRequestHistoryId == requestId)
                    .Where(s => s.Status == TeamBuildingStatus.Requested)
                    .ToList();

                foreach (var item in qTeamBuildingDetail)
                {
                    item.Status = TeamBuildingStatus.Open;

                    await WorkScope.UpdateAsync<TeamBuildingDetail>(item);
                }
            }
            await WorkScope.UpdateAsync<TeamBuildingRequestHistory>(request);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_CancelRequest)]
        public async System.Threading.Tasks.Task CancelRequest(long requestId)
        {
            var request = await GetTeamBuildingRequestById(requestId);
            if (request.Status == TeamBuildingRequestStatus.Cancelled)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been cancelled", requestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Rejected)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been rejected", requestId));
            }
            else if (request.Status == TeamBuildingRequestStatus.Done)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been disbursed", requestId));
            }
            else
            {
                request.Status = TeamBuildingRequestStatus.Cancelled;
                request.RemainingMoneyStatus = RemainingMoneyStatus.Done;

                var qTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                    .Where(s => s.TeamBuildingRequestHistoryId == requestId)
                    .Where(s => s.Status == TeamBuildingStatus.Requested)
                    .ToList();

                foreach (var item in qTeamBuildingDetail)
                {
                    item.Status = TeamBuildingStatus.Open;

                    await WorkScope.UpdateAsync<TeamBuildingDetail>(item);
                }
            }
            await WorkScope.UpdateAsync<TeamBuildingRequestHistory>(request);
        }

        [HttpGet]
        public float GetVATConfig()
        {
            var strVAT = SettingManager.GetSettingValueForApplication(AppSettingNames.VAT);
            return float.TryParse(strVAT, out float VAT) ? VAT : 10f;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_ViewDetailRequest)]
        public async Task<SelectDetailPopupDto> GetDetailOfHistory(long teamBuildingHistoryId)
        {
            var teamBuildingDetailQuery = WorkScope.GetAll<TeamBuildingDetail>()
                .Include(x => x.Employee)
                .Where(x => x.TeamBuildingRequestHistoryId == teamBuildingHistoryId)
                .Where(x => x.Status == TeamBuildingStatus.Done || x.Status == TeamBuildingStatus.Requested)
                .Select(s => new
                {
                    s.Id,
                    s.EmployeeId,
                    EmployeeUserName = s.Employee.UserName,
                    EmployeeName = s.Employee.FullName,
                    BranchName = s.Employee != null && s.Employee.Branch != null ? s.Employee.Branch.Name : "",
                    BranchColor = s.Employee.Branch.Color,
                    BranchId = s.Employee.BranchId,
                    ProjectId = s.Project.Id,
                    ProjectName = s.Project.Name,
                    s.Money,
                    s.CreationTime,
                    s.ApplyMonth,
                    s.TeamBuildingRequestHistory.RequesterId
                })
                .OrderBy(x => x.EmployeeUserName)
                .ThenBy(x => x.ApplyMonth)
                .ToList();

            var requesterId = teamBuildingDetailQuery.Select(s => s.RequesterId).FirstOrDefault();

            var requesterProjectIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == requesterId)
                .Select(x => x.ProjectId)
                .Distinct()
                .ToListAsync();

            var requestInfo = await WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.Id == teamBuildingHistoryId)
                .Select(s => new
                {
                    RemainingMoney = s.RemainingMoney,
                    Note = s.Note,
                    DisburseMoney = s.DisbursedMoney
                }).FirstOrDefaultAsync();

            var teamBuildingDetailDtos = teamBuildingDetailQuery.Select(s =>
                new SelectTeamBuildingDetailDto
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeUserName = s.EmployeeUserName,
                    EmployeeName = s.EmployeeName,
                    ProjectId = s.ProjectId,
                    ProjectName = s.ProjectName,
                    Money = s.Money,
                    CreationTime = s.CreationTime,
                    ApplyMonth = s.ApplyMonth,
                    BranchId = s.BranchId,
                    BranchName = s.BranchName,
                    BranchColor = s.BranchColor,
                    IsWarning = !requesterProjectIds.Contains(s.ProjectId)
                })
                .ToList();

            var request = await GetTeamBuildingRequestById(teamBuildingHistoryId);

            if (request.Status == TeamBuildingRequestStatus.Rejected || request.Status == TeamBuildingRequestStatus.Cancelled)
            {
                return null;
            }

            return new SelectDetailPopupDto
            {
                TeamBuildingDetailDtos = teamBuildingDetailDtos,
                LastRemainMoney = requestInfo.RemainingMoney ?? 0,
                Note = requestInfo.Note,
                DisburseMoney = requestInfo.DisburseMoney ?? 0,
            };
        }

        [HttpGet]
        public Task<List<RequestHistoryFileDto>> GetAllRequestHistoryFileById(long historyId)
        {
            return WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                  .Where(s => s.TeamBuildingRequestHistoryId == historyId)
                  .Select(s => new RequestHistoryFileDto
                  {
                      Id = s.Id,
                      FileName = s.FileName,
                      Url = UrlUtils.FormatUrlWithFullDomain(s.Url),
                      RequestHistoryId = s.TeamBuildingRequestHistoryId
                  }).ToListAsync();
        }

        [HttpPost]
        public async Task<ViewDetailRequestDto> GetAllDetailByHistoryId(InputGetAllDetailByRequestIdDto input)
        {
            var userId = AbpSession.UserId.Value;

            var listProjectIdIsAllowTBByUserId = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.Type == ProjectUserType.PM)
                .WhereIf(input.ProjectId.HasValue, s => s.ProjectId == input.ProjectId)
                .Where(s => !s.Project.IsDeleted && s.Project.IsAllowTeamBuilding)
                .Select(s => s.ProjectId)
                .ToListAsync();

            // Các bản ghi đã được PM chọn
            var listRequested = await WorkScope.GetAll<TeamBuildingDetail>()
                .Include(s => s.Employee)
                .Where(s => s.TeamBuildingRequestHistoryId == input.TeamBuildingHistoryId)
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Requested)
                .WhereIf(input.Month.HasValue, s => s.ApplyMonth.Month == input.Month)
                .WhereIf(input.BranchId.HasValue, s => s.Employee.BranchId == input.BranchId)
                .Select(s => new SelectTeamBuildingDetailDto
                {
                    EmployeeId = s.EmployeeId,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    EmployeeFullName = s.Employee.FullName,
                    EmployeeName = s.Employee.Name,
                    EmployeeUserName = s.Employee.UserName,
                    RequesterEmailAddress = s.TeamBuildingRequestHistory.Requester.EmailAddress,
                    RequesterFullName = s.TeamBuildingRequestHistory.Requester.FullName,
                    BranchName = s.Employee != null && s.Employee.Branch != null ? s.Employee.Branch.Name : "",
                    BranchColor = s.Employee.Branch.Color,
                    BranchId = s.Employee.BranchId,
                    ApplyMonth = s.ApplyMonth,
                    Money = s.Money,
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                    Status = s.Status,
                    Id = s.Id,
                    TeamBuildingRequestHistoryId = s.TeamBuildingRequestHistoryId,
                    IsWarning = !listProjectIdIsAllowTBByUserId.Contains(s.ProjectId)
                })
                .OrderBy(s => s.ApplyMonth.Month)
                .OrderBy(s => s.IsWarning)
                .ToListAsync();

            // Các bản ghi có thể chọn trong dự án của PM đó
            var listOpen = await WorkScope.GetAll<TeamBuildingDetail>()
                .Include(s => s.Employee)
                .Where(s => listProjectIdIsAllowTBByUserId.Contains(s.ProjectId))
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Open)
                .WhereIf(input.Month.HasValue, s => s.ApplyMonth.Month == input.Month)
                .WhereIf(input.BranchId.HasValue, s => s.Employee.BranchId == input.BranchId)
                .Select(s => new SelectTeamBuildingDetailDto
                {
                    EmployeeId = s.EmployeeId,
                    EmployeeEmailAddress = s.Employee.EmailAddress,
                    EmployeeFullName = s.Employee.FullName,
                    EmployeeName = s.Employee.Name,
                    EmployeeUserName = s.Employee.UserName,
                    RequesterEmailAddress = s.TeamBuildingRequestHistory.Requester.EmailAddress,
                    RequesterFullName = s.TeamBuildingRequestHistory.Requester.FullName,
                    BranchName = s.Employee != null && s.Employee.Branch != null ? s.Employee.Branch.Name : "",
                    BranchColor = s.Employee.Branch.Color,
                    BranchId = s.Employee.BranchId,
                    ApplyMonth = s.ApplyMonth,
                    Money = s.Money,
                    ProjectId = s.ProjectId,
                    ProjectName = s.Project.Name,
                    Status = s.Status,
                    Id = s.Id,
                    IsWarning = !listProjectIdIsAllowTBByUserId.Contains(s.ProjectId)
                })
                .OrderBy(s => s.ApplyMonth.Month)
                .ToListAsync();

            foreach (var item in listOpen)
            {
                listRequested.Add(item);
            }

            listRequested
                .OrderByDescending(x => x.Status).ThenBy(x => x.ApplyMonth.Month);

            var fileAndUrl = await WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(x => x.TeamBuildingRequestHistoryId == input.TeamBuildingHistoryId)
                .Select(s => new
                {
                    s.TeamBuildingRequestHistory.RemainingMoney,
                    s.TeamBuildingRequestHistory.Note,
                    s.Url,
                    s.FileName
                })
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

            return new ViewDetailRequestDto
            {
                TeamBuildingDetailDtos = listRequested,
                LastRemainMoney = lastRequestRemainingMoney,
                Note = fileAndUrl.FirstOrDefault()?.Note,
                Files = fileAndUrl.Select(s => s.FileName).ToList(),
                ListUrl = fileAndUrl.Select(s => s.Url).ToList(),
            };
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_EditRequest)]
        public async Task<EditRequestDto> EditRequest(EditRequestDto input)
        {
            var userId = AbpSession.UserId.Value;
            float totalMoney = 0;

            var LastRequestHistory = WorkScope.GetAll<TeamBuildingRequestHistory>()
                  .Where(s => s.RequesterId == userId)
                  .Where(s => s.Status == StatusEnum.TeamBuildingRequestStatus.Done)
                  .OrderByDescending(s => s.CreationTime)
                  .FirstOrDefault();

            if (LastRequestHistory != null && LastRequestHistory.RemainingMoneyStatus == StatusEnum.RemainingMoneyStatus.Remaining)
            {
                totalMoney += LastRequestHistory?.RemainingMoney ?? 0;
            }

            // danh sách các bản ghi đã chọn trước đó
            var listDetailIsExist = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.Status == TeamBuildingStatus.Requested)
                .Where(s => s.TeamBuildingRequestHistoryId == input.Id).ToListAsync();

            // danh sách các bản ghi có thể bỏ bớt
            var listDetailRemove = listDetailIsExist
                .Where(s => !input.ListDetailId.Contains(s.Id))
                .ToList();

            listDetailRemove.ForEach(s =>
            {
                s.TeamBuildingRequestHistoryId = null;
                s.Status = TeamBuildingStatus.Open;
                WorkScope.UpdateAsync(s);
            });

            // danh sách các bản ghi chọn lúc edit
            var listDetail = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => input.ListDetailId.Contains(s.Id))
                .ToListAsync();

            listDetail.ForEach(
                s =>
                {
                    totalMoney += s.Money;
                    s.Status = StatusEnum.TeamBuildingStatus.Requested;
                    s.TeamBuildingRequestHistoryId = input.Id;
                    WorkScope.UpdateAsync(s);
                });

            var itemRequest = await WorkScope.GetAsync<TeamBuildingRequestHistory>(input.Id);
            itemRequest.Id = input.Id;
            itemRequest.RequestMoney = totalMoney;
            await WorkScope.UpdateAsync(itemRequest);

            return input;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TeamBuilding_Request_ReOpenRequest)]
        public async System.Threading.Tasks.Task ReOpenRequest(long requestId)
        {
            var request = await GetTeamBuildingRequestById(requestId);

            if (request.Status == TeamBuildingRequestStatus.Rejected || request.Status == TeamBuildingRequestStatus.Cancelled)
            {
                throw new UserFriendlyException(string.Format("This request {0} has been Rejected or Cancelled", requestId));
            }

            request.DisbursedMoney = null;
            request.RemainingMoney = null;
            request.VATMoney = null;
            request.RemainingMoneyStatus = RemainingMoneyStatus.Remaining;
            request.Status = TeamBuildingRequestStatus.Pending;

            var listTeamBuildingDetail = WorkScope.GetAll<TeamBuildingDetail>()
                    .Where(s => s.TeamBuildingRequestHistoryId == requestId)
                    .Where(s => s.Status == TeamBuildingStatus.Done)
                    .ToList();

            double totalMoneyByTBDetail = 0;

            foreach (var item in listTeamBuildingDetail)
            {
                totalMoneyByTBDetail += item.Money;
                item.Status = TeamBuildingStatus.Requested;

                await WorkScope.UpdateAsync(item);
            }

            // có sử dụng remainingMoney của request khác của PM đó
            if (totalMoneyByTBDetail < request.RequestMoney)
            {
                var lastRequestByRequesterIdWithoutCurrent = WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == request.RequesterId)
                .Where(s => s.Id != requestId)
                .Where(s => s.Status == TeamBuildingRequestStatus.Done)
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

                if (lastRequestByRequesterIdWithoutCurrent != null && lastRequestByRequesterIdWithoutCurrent.Id > 0)
                {
                    lastRequestByRequesterIdWithoutCurrent.RemainingMoneyStatus = RemainingMoneyStatus.Remaining;

                    await WorkScope.UpdateAsync(lastRequestByRequesterIdWithoutCurrent);
                }

            }

            await WorkScope.UpdateAsync(request);
        }

        [HttpPost]
        public async Task<List<GetAllUserRequestMoneyDto>> GetUserNotPaggingRequestMoneyEdit(InputGetUserOtherProjectDto input)
        {
            var listUserIdByTeamBuildingDetail = await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(x => !input.Ids.Contains(x.Id))
                .Where(x => x.Employee.IsActive)
                .Where(x => x.Status == StatusEnum.TeamBuildingStatus.Open)
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
        public async Task<List<SelectTeamBuildingDetailDto>> GetRequestMoneyInfoUserEdit(List<long> listEmpIds, long teamBuildingRequestId)
        {
            var input = new InputGetAllDetailByRequestIdDto
            {
                TeamBuildingHistoryId = teamBuildingRequestId,
                ProjectId = null,
                BranchId = null,
                Month = null,
            };
            var listUserInRequest = (await GetAllDetailByHistoryId(input)).TeamBuildingDetailDtos.Select(s => s.Id).ToList();

            var listDetailByEmployeeId = WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => listEmpIds.Contains(s.EmployeeId))
                .Where(s => !listUserInRequest.Contains(s.Id))
                .Select(s => s.Id).ToList();

            var userId = AbpSession.UserId.Value;

            var listProjectIdByUserId = await WorkScope.GetAll<ProjectUser>()
               .Where(s => s.UserId == userId)
               .Where(s => s.Type == ProjectUserType.PM)
               .Select(s => s.ProjectId)
               .ToListAsync();

            return await WorkScope.GetAll<TeamBuildingDetail>()
                .Where(s => listDetailByEmployeeId.Contains(s.Id))
                .Where(s => !listProjectIdByUserId.Contains(s.ProjectId))
                .Where(s => s.Status == StatusEnum.TeamBuildingStatus.Open)
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
        public async Task<EditOneInvoiceDto> EditOneInvoice(EditOneInvoiceDto input)
        {
            var item = await WorkScope.GetAsync<TeamBuildingRequestHistoryFile>(input.Id);
            item.IsVAT = input.HasVAT;
            ObjectMapper.Map(input, item);
            await WorkScope.UpdateAsync(item);

            var teamBuildingHistoryId = await WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(s => s.Id == input.Id)
                .Select(s => s.TeamBuildingRequestHistoryId)
                .FirstOrDefaultAsync();

            var teamBuildingHistory = await WorkScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.Id == teamBuildingHistoryId)
                .FirstOrDefaultAsync();

            var totalInvoiceAmount = await WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(s => s.TeamBuildingRequestHistoryId == teamBuildingHistory.Id)
                .Where(s => s.Id != input.Id)
                .SumAsync(s => s.InvoiceAmount);

            teamBuildingHistory.InvoiceAmount = totalInvoiceAmount + input.InvoiceAmount;
     
            await WorkScope.UpdateAsync(teamBuildingHistory);

            return input;
        }

        [HttpGet]
        public async Task<EditOneInvoiceDto> GetOneInvoice(long id)
        {
            return await WorkScope.GetAll<TeamBuildingRequestHistoryFile>()
                .Where(s => s.Id == id)
                .Select(s => new EditOneInvoiceDto
                {
                    Id = s.Id,
                    InvoiceAmount = s.InvoiceAmount,
                    HasVAT = s.IsVAT
                })
                .FirstOrDefaultAsync();
        }
    }
}
