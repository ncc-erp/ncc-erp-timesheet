using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using Amazon.Util.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Ncc.Configuration;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Positions;
using Timesheet.APIs.RequestDays;
using Timesheet.APIs.TeamBuildingRequestHistories;
using Timesheet.APIs.TeamBuildingRequestHistories.dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;

namespace Timesheet.Application.Tests.API.TeamBuildingRequestHistories
{
    public class TeamBuildingRequestHistoriesAppService_Test : TimesheetApplicationTestBase
    {
        private TeamBuildingRequestHistoriesAppService _requestHistoryAppService;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;

        public TeamBuildingRequestHistoriesAppService_Test()
        {
            _settingManager = Substitute.For<ISettingManager>();
            _workScope = Resolve<IWorkScope>();
            _requestHistoryAppService = new TeamBuildingRequestHistoriesAppService(_workScope);
            _requestHistoryAppService.AbpSession = AbpSession;
            _requestHistoryAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();

            _settingManager.GetSettingValueForApplicationAsync(AppSettingNames.BillPercentage).Returns(Task.FromResult("90"));

            _requestHistoryAppService.SettingManager = _settingManager;
        }

        [Fact]
        public async Task GetAllPaging_Test1()
        {
            var expectTotalCount = 11;
            var expectItemCount = 10;

            await WithUnitOfWorkAsync(async () =>
            {
                var input = new InputMultiFilterRequestHistoryDto
                {
                    GridParam = new Paging.GridParam { },
                    Year = 2023,
                };

                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test2()
        {
            var expectTotalCount = 11;
            var expectItemCount = 8;
            var skipCount = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var input = new InputMultiFilterRequestHistoryDto
                {
                    GridParam = new Paging.GridParam
                    {
                        SkipCount = skipCount,
                    },
                    Year = 2023,
                };

                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                Assert.NotEqual(expectTotalCount, result.Items.Count);
                Assert.Equal(expectItemCount, result.TotalCount - skipCount);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test3()
        {
            var expectItemCount = 3;
            var skipCount = 8;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { SkipCount = skipCount, },
                Year = 2023,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test4()
        {
            var takeCount = 3;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { MaxResultCount = takeCount, },
                Year = 2023,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test5()
        {
            var skipCount = 2;
            var takeCount = 3;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam
                {
                    MaxResultCount = takeCount,
                    SkipCount = skipCount,
                },
                Year = 2023,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test6()
        {
            var expectTotalCount = 4;
            var expectItemCount = 4;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { },
                Year = 2023,
                Status = Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Done
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test7()
        {
            var expectTotalCount = 5;
            var expectItemCount = 5;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { },
                Year = 2023,
                Status = Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test8()
        {
            var expectTotalCount = 1;
            var expectItemCount = 1;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { },
                Year = 2023,
                Status = Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Rejected
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test9()
        {
            var expectTotalCount = 1;
            var expectItemCount = 1;

            var input = new InputMultiFilterRequestHistoryDto
            {
                GridParam = new GridParam { },
                Year = 2023,
                Status = Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Cancelled
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllPagging(input);

                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetEmailPMInRequest_Test1()
        {
            var expectTotalCount = 11;
            var expectItemCount = 11;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetEmailPMInRequest();

                Assert.Equal(expectTotalCount, result.Count);
                Assert.Equal(expectItemCount, result.Count);
            });
        }

        [Fact]
        public async Task RejectRequest_Test1()
        {
            long requestId = 6;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.RejectRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been cancelled", exception.Message);
            });
        }

        [Fact]
        public async Task RejectRequest_Test2()
        {
            long requestId = 5;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.RejectRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been rejected", exception.Message);
            });
        }

        [Fact]
        public async Task RejectRequest_Test3()
        {
            long requestId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.RejectRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been disbursed", exception.Message);
            });
        }

        [Fact]
        public async Task RejectRequest_Test4()
        {
            long requestId = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(requestId);
                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == requestId);

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(requestId);
                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == requestId);

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }

                await _requestHistoryAppService.RejectRequest(requestId);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Rejected);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Open);
                }
            });
        }

        [Fact]
        public async Task CancelRequest_Test1()
        {
            long requestId = 6;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.CancelRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been cancelled", exception.Message);
            });
        }

        [Fact]
        public async Task CancelRequest_Test2()
        {
            long requestId = 5;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.CancelRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been rejected", exception.Message);
            });
        }

        [Fact]
        public async Task CancelRequest_Test3()
        {
            long requestId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.CancelRequest(requestId);
                });
                Assert.Equal($"This request {requestId} has been disbursed", exception.Message);
            });
        }

        [Fact]
        public async Task CancelRequest_Test4()
        {
            long requestId = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(requestId);
                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == requestId);

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(requestId);
                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == requestId);

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }

                await _requestHistoryAppService.CancelRequest(requestId);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Cancelled);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Open);
                }
            });
        }

        [Fact]
        public async Task DisburseRequest_Test1()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 7,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };
            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 1,
                DisburseMoney = 20000,
                RequestId = 6,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.DisburseRequest(input);
                });
                Assert.Equal($"This request {input.RequestId} has been cancelled", exception.Message);
            });
        }

        [Fact]
        public async Task DisburseRequest_Test2()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 6,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };
            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 1,
                DisburseMoney = 20000,
                RequestId = 5,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.DisburseRequest(input);
                });
                Assert.Equal($"This request {input.RequestId} has been rejected", exception.Message);
            });
        }

        [Fact]
        public async Task DisburseRequest_Test3()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 1,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };
            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 1,
                DisburseMoney = 20000,
                RequestId = 1,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestHistoryAppService.DisburseRequest(input);
                });
                Assert.Equal($"This request {input.RequestId} has been disbursed", exception.Message);
            });
        }

        [Fact]
        // RequestMoney = DisburseMoney, không có RemainingMoney
        public async Task DisburseRequest_Test4()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 8,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };

            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 17,
                DisburseMoney = 200000f,
                RequestId = 7,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);
                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);
                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestBeforeChange.RequestMoney.ShouldBe(200000f);
                requestBeforeChange.DisbursedMoney.ShouldBe(0f);
                requestBeforeChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }

                await _requestHistoryAppService.DisburseRequest(input);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Done);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                requestAfterChange.RequestMoney.ShouldBe(200000f);
                requestAfterChange.DisbursedMoney.ShouldBe(200000f);
                requestAfterChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Done);
                }
            });
        }

        [Fact]
        // RequestMoney = DisburseMoney, có RemainingMoney
        public async Task DisburseRequest_Test5()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 9,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };

            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 4,
                DisburseMoney = 150000f,
                RequestId = 9,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var listRequestByRequesterId = _workScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == input.RequesterId);

                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);

                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var listRequestWithoutLastBeforeChange = listRequestByRequesterId.Take(listRequestByRequesterId.Count() - 1).ToList();

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);

                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var listRequestByRequesterIdWithoutLastAfterChange = _workScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == input.RequesterId).Take(listRequestByRequesterId.Count() - 1);

                var listRequestWithoutLastAfterChange = listRequestByRequesterId.Take(listRequestByRequesterId.Count() - 1).ToList();

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestBeforeChange.RequestMoney.ShouldBe(150000f);
                requestBeforeChange.DisbursedMoney.ShouldBe(0f);
                requestBeforeChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }
                foreach (var item in listRequestWithoutLastBeforeChange)
                {
                    item.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                }

                await _requestHistoryAppService.DisburseRequest(input);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Done);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                requestAfterChange.RequestMoney.ShouldBe(150000f);
                requestAfterChange.DisbursedMoney.ShouldBe(150000f);
                requestAfterChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Done);
                }
                foreach (var item in listRequestWithoutLastAfterChange)
                {
                    item.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                }
            });
        }

        [Fact]
        // RequestMoney > DisburseMoney, không có RemainingMoney
        public async Task DisburseRequest_Test6()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 10,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };

            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 17,
                DisburseMoney = 170000f,
                RequestId = 10,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);
                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);
                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestBeforeChange.RequestMoney.ShouldBe(200000f);
                requestBeforeChange.DisbursedMoney.ShouldBe(0f);
                requestBeforeChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }

                await _requestHistoryAppService.DisburseRequest(input);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Done);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestAfterChange.RequestMoney.ShouldBe(200000f);
                requestAfterChange.DisbursedMoney.ShouldBe(170000f);
                requestAfterChange.RemainingMoney.ShouldBe(requestAfterChange.RequestMoney - requestAfterChange.DisbursedMoney);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Done);
                }
            });
        }

        [Fact]
        // RequestMoney > DisburseMoney, có RemainingMoney
        public async Task DisburseRequest_Test7()
        {
            var invoiceInput1 = new DisburseTeamBuildingInvoiceRequestDto
            {
                InvoiceId = 11,
                HasVAT = true
            };
            var invoiceInputList = new List<DisburseTeamBuildingInvoiceRequestDto>
            {
                invoiceInput1,
            };

            var input = new DisburseTeamBuildingRequestDto
            {
                RequesterId = 4,
                DisburseMoney = 170000f,
                RequestId = 11,
                InvoiceDisburseList = invoiceInputList
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var listRequestByRequesterId = _workScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == input.RequesterId);

                var requestBeforeChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);

                var listDetailBeforeChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var listRequestWithoutLastBeforeChange = listRequestByRequesterId.Take(listRequestByRequesterId.Count() - 1).ToList();

                var requestAfterChange = await _workScope.GetAsync<TeamBuildingRequestHistory>(input.RequestId);

                var listDetailAfterChange = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.TeamBuildingRequestHistory.RequesterId == input.RequestId);

                var listRequestByRequesterIdWithoutLastAfterChange = _workScope.GetAll<TeamBuildingRequestHistory>()
                .Where(s => s.RequesterId == input.RequesterId).Take(listRequestByRequesterId.Count() - 1);

                var listRequestWithoutLastAfterChange = listRequestByRequesterId.Take(listRequestByRequesterId.Count() - 1).ToList();

                requestBeforeChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Pending);
                requestBeforeChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestBeforeChange.RequestMoney.ShouldBe(200000f);
                requestBeforeChange.DisbursedMoney.ShouldBe(0f);
                requestBeforeChange.RemainingMoney.ShouldBe(0f);
                foreach (var item in listDetailBeforeChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Requested);
                }
                foreach (var item in listRequestWithoutLastBeforeChange)
                {
                    item.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                }

                await _requestHistoryAppService.DisburseRequest(input);

                requestAfterChange.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingRequestStatus.Done);
                requestAfterChange.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Remaining);
                requestAfterChange.RequestMoney.ShouldBe(200000f);
                requestAfterChange.DisbursedMoney.ShouldBe(input.DisburseMoney);
                requestAfterChange.RemainingMoney.ShouldBe(requestAfterChange.RequestMoney - requestAfterChange.DisbursedMoney);
                foreach (var item in listDetailAfterChange)
                {
                    item.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Done);
                }
                foreach (var item in listRequestWithoutLastAfterChange)
                {
                    item.RemainingMoneyStatus.ShouldBe(Ncc.Entities.Enum.StatusEnum.RemainingMoneyStatus.Done);
                }
            });
        }

        [Fact]
        // request status = Rejected
        public async Task GetDetailOfHistory_Test1()
        {
            long requestId = 5;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetDetailOfHistory(requestId);

                result.ShouldBe(null);
            });
        }

        [Fact]
        // request status = Cancelled
        public async Task GetDetailOfHistory_Test2()
        {
            long requestId = 6;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetDetailOfHistory(requestId);

                result.ShouldBe(null);
            });
        }

        [Fact]
        // request status = Pending
        public async Task GetDetailOfHistory_Test3()
        {
            long requestId = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetDetailOfHistory(requestId);

                result.TeamBuildingDetailDtos.Count().ShouldBe(3);
                result.LastRemainMoney.ShouldBe(0f);
                result.Note.ShouldBe(null);
            });
        }

        [Fact]
        // request status = Done
        public async Task GetDetailOfHistory_Test4()
        {
            long requestId = 4;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetDetailOfHistory(requestId);

                result.TeamBuildingDetailDtos.Count().ShouldBe(1);
                result.LastRemainMoney.ShouldBe(0f);
                result.Note.ShouldBe(null);
            });
        }

        [Fact]
        public async Task GetAllRequestHistoryFileById_Test1()
        {
            long requestId = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestHistoryAppService.GetAllRequestHistoryFileById(requestId);

                result.Count().ShouldBe(1);
            });
        }
    }
}
