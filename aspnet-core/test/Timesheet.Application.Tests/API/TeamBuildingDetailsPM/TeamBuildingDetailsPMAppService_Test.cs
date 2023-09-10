using Abp.Extensions;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Amazon.S3;
using AutoMapper.Configuration;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Word;
using Ncc.Entities;
using Ncc.IoC;
using Ncc.MultiTenancy;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Timesheet.APIs.TeamBuildingDetailsPM;
using Timesheet.APIs.TeamBuildingRequestMoney.dto;
using Timesheet.Constants;
using Timesheet.Entities;
using Timesheet.Services.HRM;
using Timesheet.Services.Project;
using Timesheet.Timesheets.Projects;
using Timesheet.UploadFilesService;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.UI;
using Timesheet.APIs.ReviewDetails;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Amazon.Runtime.CredentialManagement;
using Amazon;
using Amazon.Runtime;
using System.Security.Policy;
using System.IO;
using Abp.TestBase.Runtime.Session;
using Timesheet.APIs.RetroDetails.Dto;
using Timesheet.Paging;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.APIs.RetroDetails;
using Abp.Domain.Uow;
using Abp.Collections.Extensions;
using NSubstitute.Extensions;
using Abp.Configuration;
using Ncc.Configuration;
using Timesheet.Services.Komu;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Text.Json;

namespace Timesheet.Application.Tests.API.TeamBuildingDetailsPM
{
    public class TeamBuildingDetailsPMAppService_Test : TimesheetApplicationTestBase
    {

        private TeamBuildingDetailsPMAppService _detailPMAppService;
        private readonly IWorkScope _workScope;
        private readonly UploadTeamBuildingService _uploadTeamBuildingService;
        private readonly ProjectService _projectService;

        public TeamBuildingDetailsPMAppService_Test()
        {
            ConstantAmazonS3.Profile = "timesheet-dev-aws-s3-profile";
            ConstantAmazonS3.AccessKeyId = "AKIASXE66I4T4T6776XN";
            ConstantAmazonS3.SecretKeyId = "Vv+mL4kV7mvH0yZEHQcX3HJv/BNXU0hJe4Wof1bf";
            ConstantAmazonS3.Region = "ap-southeast-1";
            ConstantAmazonS3.BucketName = "ncc-erp";
            ConstantAmazonS3.Prefix = "dev";
            ConstantAmazonS3.CloudFront = "http://example.com";

            ConstantTeamBuildingFile.AllowFileTypes = "jpg,jpeg,png,pdf,docx".Split(",");
            ConstantTeamBuildingFile.ParentFolder = "timesheet";
            ConstantTeamBuildingFile.FileFolder = "teambuildings";
            /////
            var options = new CredentialProfileOptions
            {
                AccessKey = ConstantAmazonS3.AccessKeyId,
                SecretKey = ConstantAmazonS3.SecretKeyId
            };
            var profile = new CredentialProfile(ConstantAmazonS3.Profile, options);
            profile.Region = RegionEndpoint.GetBySystemName(ConstantAmazonS3.Region);

            var sharedFile = new SharedCredentialsFile();
            sharedFile.RegisterProfile(profile);

            CredentialProfile basicProfile;
            AWSCredentials awsCredentials;

            sharedFile.TryGetProfile(ConstantAmazonS3.Profile, out basicProfile);
            AWSCredentialsFactory.TryGetAWSCredentials(basicProfile, sharedFile, out awsCredentials);
            var s3lient = new AmazonS3Client(awsCredentials, basicProfile.Region);

            //////////////////

            var loggerFileService = Resolve<ILogger<AmazonS3Service>>();
            var uploadFileService = new AmazonS3Service(loggerFileService, s3lient);

            var loggerUploadTeamBuildingService = Resolve<ILogger<UploadTeamBuildingService>>();
            var tenantManager = Resolve<TenantManager>();
            var abpSession = Resolve<IAbpSession>();

            _uploadTeamBuildingService = new UploadTeamBuildingService(uploadFileService, loggerUploadTeamBuildingService, tenantManager, abpSession);

            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://localhost/");
            configuration.GetValue<string>("ProjectService:SecurityCode").Returns("SecurityCode");


            var httpClient = Resolve<HttpClient>();
            var loggerProjectService = Resolve<ILogger<ProjectService>>();

            var loggerKomuService = Resolve<ILogger<KomuService>>();
            var _config = Substitute.For<IConfiguration>();
            _config.GetValue<string>("KomuService:DevModeChannelId").Returns("DevModeChannelId");
            _config.GetValue<string>("KomuService:EnableKomuNotification").Returns("true");
            _config.GetValue<string>("KomuService:BaseAddress").Returns("http://example.com/");
            _config.GetValue<string>("KomuService:SecurityCode").Returns("SecurityCode");
            var _settingManager = Substitute.For<ISettingManager>();
            var _komuService = Substitute.For<KomuService>(httpClient, loggerKomuService, _config, _settingManager);

            _projectService = new ProjectService(httpClient, configuration, loggerProjectService);
            _workScope = Resolve<IWorkScope>();
            _detailPMAppService = new TeamBuildingDetailsPMAppService(_uploadTeamBuildingService, _projectService, _komuService, _workScope);
            _detailPMAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            _detailPMAppService.AbpSession = AbpSession;
            _detailPMAppService.ObjectMapper = Resolve<IObjectMapper>();
            _detailPMAppService.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        //should return remaining money of last done request history
        // the status of teambuilding must be Open or Requested
        public async Task GetRequestMoney_Test()
        {
            var expectedRemainMoney = 10000;
            var expectedTeamBuildingDetailCount = 14;
            var listAllowedDetailStatus = new List<TeamBuildingStatus> { TeamBuildingStatus.Open, TeamBuildingStatus.Requested };
            await WithUnitOfWorkAsync(async () =>
            {
                var requestResult = await _detailPMAppService.GetRequestMoney(null, null);

                Assert.Equal(expectedRemainMoney, requestResult.LastRemainMoney);
                Assert.Equal(expectedTeamBuildingDetailCount, requestResult.TeamBuildingDetailDtos.Count);
                foreach (var teamBuildingDetail in requestResult.TeamBuildingDetailDtos)
                {
                    Assert.Contains(teamBuildingDetail.Status, listAllowedDetailStatus);
                }
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithJsonFormData_ShouldReturnOK()
        {
            InvoiceRequestDto invoiceRequestDto1 = new InvoiceRequestDto();
            invoiceRequestDto1.Amount = 300000;
            invoiceRequestDto1.HasVat = true;
            invoiceRequestDto1.InvoiceImageName = "0.jpg";
            invoiceRequestDto1.InvoiceUrl = null;
            InvoiceRequestDto invoiceRequestDto2 = new InvoiceRequestDto();
            invoiceRequestDto2.Amount = 400000;
            invoiceRequestDto2.HasVat = false;
            invoiceRequestDto2.InvoiceImageName = null;
            invoiceRequestDto2.InvoiceUrl = "https://www.tutorialsteacher.com/articles/convert-object-to-json-in-csharp";
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 700000;
            pMRequestDto.ListDetailId = new List<long> { 1, 2, 3 };
            pMRequestDto.ListInvoiceRequestDto = new List<InvoiceRequestDto> { invoiceRequestDto1, invoiceRequestDto2 };
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = new List<IFormFile> { mockFile1 }
                };

                var returnRequestHistory = await _detailPMAppService.SubmitRequestMoney(input);
                List<TeamBuildingDetail> listDetail = _workScope
                  .GetAll<TeamBuildingDetail>()
                  .Where(s => pMRequestDto.ListDetailId.Contains(s.Id))
                  .ToList();

                returnRequestHistory.ShouldNotBeNull();
                returnRequestHistory.Id.ShouldBe(12);
                returnRequestHistory.RequesterId.ShouldBe(1);
                returnRequestHistory.TitleRequest.ShouldBe("admin admin - HN1");
                returnRequestHistory.RequestMoney.ShouldBe(130000f);
                returnRequestHistory.RemainingMoneyStatus.ShouldBe(RemainingMoneyStatus.Remaining);
                returnRequestHistory.Status.ShouldBe(TeamBuildingRequestStatus.Pending);
                returnRequestHistory.TeamBuildingRequestHistoryFileDtos.Count.ShouldBe(2);
                listDetail.ForEach(s => s.Status.ShouldBe(TeamBuildingStatus.Requested));
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithEmptyInvoiceRequest_ShouldThrowException()
        {
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 0;
            pMRequestDto.ListDetailId = new List<long> { 1, 2, 3 };
            pMRequestDto.ListInvoiceRequestDto = null;
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = null
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });
                exception.Message.ShouldBe("Invoice Request cannot be null!");
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithOneInvoiceRequestHaveEmptyInvoiceImageNameAndInvoiceUrl_ShouldThrowException()
        {
            InvoiceRequestDto invoiceRequestDto1 = new InvoiceRequestDto();
            invoiceRequestDto1.Amount = 300000;
            invoiceRequestDto1.HasVat = true;
            invoiceRequestDto1.InvoiceImageName = "0.jpg";
            invoiceRequestDto1.InvoiceUrl = null;
            InvoiceRequestDto invoiceRequestDto2 = new InvoiceRequestDto();
            invoiceRequestDto2.Amount = 400000;
            invoiceRequestDto2.HasVat = false;
            invoiceRequestDto2.InvoiceImageName = null;
            invoiceRequestDto2.InvoiceUrl = null;
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 0;
            pMRequestDto.ListDetailId = new List<long> { 1, 2, 3 };
            pMRequestDto.ListInvoiceRequestDto = new List<InvoiceRequestDto> { invoiceRequestDto1, invoiceRequestDto2 };
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = new List<IFormFile> { mockFile1 }
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });
                exception.Message.ShouldBe("One invoice request must have invoice file or invoice url!");
            });
        }

        [Fact]
        public async Task GetAllPagging_WithNoProjectFoundByCurrentUser_ShouldThrowException()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                AbpSession.UserId = 13;
                var input = new InputFilterTeamBuildingDetailPagingDto
                {
                    GridParam = new GridParam { },
                    Year = 2023
                };
                var expectedMessage = "No project found";
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.GetAllPagging(input);
                });
                exception.Message.ShouldBe(expectedMessage);
            });
        }

        [Fact]
        public async Task GetAllPagging_WithNoStatus_OK()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new InputFilterTeamBuildingDetailPagingDto
                {
                    GridParam = new GridParam { },
                    Year = 2022
                };
                var result = await _detailPMAppService.GetAllPagging(input);

                result.TotalCount.ShouldBe(15);
                result.Items.All(s => s.ApplyMonth.Year == 2022).ShouldBeTrue();
                result.Items.Where(s => s.Status == TeamBuildingStatus.Done)
                .All(s => s.RequesterId == AbpSession.UserId.Value).ShouldBeTrue();
                result.Items.All(s => s.ProjectId == 1 || s.ProjectId == 5).ShouldBeTrue();
            });
        }

        [Fact]
        public async Task GetAllPagging_WithStatus_OK()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new InputFilterTeamBuildingDetailPagingDto
                {
                    GridParam = new GridParam { },
                    Year = 2022,
                    Status = TeamBuildingStatus.Open
                };
                var result = await _detailPMAppService.GetAllPagging(input);

                result.TotalCount.ShouldBe(11);
                result.Items.All(s => s.ApplyMonth.Year == 2022).ShouldBeTrue();
                result.Items.All(s => s.Status == TeamBuildingStatus.Open).ShouldBeTrue();
                result.Items.All(s => s.ProjectId == 1 || s.ProjectId == 5).ShouldBeTrue();
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithOneInvoiceRequestHaveInvoiceUrlNotValid_ShouldThrowException()
        {

            InvoiceRequestDto invoiceRequestDto1 = new InvoiceRequestDto();
            invoiceRequestDto1.Amount = 300000;
            invoiceRequestDto1.HasVat = true;
            invoiceRequestDto1.InvoiceImageName = "0.jpg";
            invoiceRequestDto1.InvoiceUrl = null;
            InvoiceRequestDto invoiceRequestDto2 = new InvoiceRequestDto();
            invoiceRequestDto2.Amount = 400000;
            invoiceRequestDto2.HasVat = false;
            invoiceRequestDto2.InvoiceImageName = null;
            invoiceRequestDto2.InvoiceUrl = "thumbs.dreamstime.com/b";
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 0;
            pMRequestDto.ListDetailId = new List<long> { 1, 2, 3 };
            pMRequestDto.ListInvoiceRequestDto = new List<InvoiceRequestDto> { invoiceRequestDto1, invoiceRequestDto2 };
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = new List<IFormFile> { mockFile1 }
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });
                exception.Message.ShouldBe("Invalid Url");
            });
        }

        //[Fact]
        //public async Task SubmitRequestMoney_WithNotValidFileContentTypeUrl_ShouldThrowException()
        //{
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        string url = "https://thumbs.dreamstime.com/abc.jgk";

        //        SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
        //        {
        //            ListDetailId = new List<long> { 1, 2, 3 },
        //            ListUrl = new List<string> { url }
        //        };
        //        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        //        {
        //            await _detailPMAppService.SubmitRequestMoney(input);
        //        });
        //        string urlExt = Path.GetExtension(url);
        //        urlExt = urlExt.Substring(1).ToLower();
        //        string expectedMessage = $"Wrong file type {urlExt}. Allow file types: {string.Join(", ", ConstantTeamBuildingFile.AllowFileTypes)}";
        //        exception.Message.ShouldBe(expectedMessage);
        //    });
        //}
        [Fact]
        public async Task SubmitRequestMoney_WithOneInvoiceRequestHaveImageNotValidFileContentype_ShouldThrowException()
        {

            InvoiceRequestDto invoiceRequestDto1 = new InvoiceRequestDto();
            invoiceRequestDto1.Amount = 300000;
            invoiceRequestDto1.HasVat = true;
            invoiceRequestDto1.InvoiceImageName = "0.gif";
            invoiceRequestDto1.InvoiceUrl = null;
            InvoiceRequestDto invoiceRequestDto2 = new InvoiceRequestDto();
            invoiceRequestDto2.Amount = 400000;
            invoiceRequestDto2.HasVat = false;
            invoiceRequestDto2.InvoiceImageName = null;
            invoiceRequestDto2.InvoiceUrl = "http://localhost:21023/swagger/index.html";
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 0;
            pMRequestDto.ListDetailId = new List<long> { 1, 2, 3 };
            pMRequestDto.ListInvoiceRequestDto = new List<InvoiceRequestDto> { invoiceRequestDto1, invoiceRequestDto2 };
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            await WithUnitOfWorkAsync(async () =>
            {
                // Create a mock IFormFile object
                var mockFile1 = Substitute.For<IFormFile>();
                mockFile1.FileName.Returns("0.gif");
                mockFile1.Length.Returns(1024);
                mockFile1.ContentType.Returns("image/gif");
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = new List<IFormFile> { mockFile1 }
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });

                string expectedMessage = $"Wrong file type {mockFile1.ContentType}. Allow file types: {string.Join(", ", ConstantTeamBuildingFile.AllowFileTypes)}";
                exception.Message.ShouldBe(expectedMessage);
            });
        }
        //[Fact]
        //public async Task SubmitRequestMoney_WithHaveAlreadyPendingRequest_ShoulldThrowException()
        //{
        //    List<long> listDetailId1 = new List<long>() { 1 };
        //    List<long> listDetailId2 = new List<long>() { 2, 3 };
        //    SubmitRequestMoneyDto input1 = new SubmitRequestMoneyDto
        //    {
        //        ListDetailId = listDetailId1,
        //        ListUrl = new List<string> {
        //                "https://thumbs.dreamstime.com/b/environment-earth-day-hands-trees-growing-seedlings-bokeh-green-background-female-hand-holding-tree-nature-field-gra-130247647.jpg",
        //            }
        //    };
        //    SubmitRequestMoneyDto input2 = new SubmitRequestMoneyDto
        //    {
        //        ListDetailId = listDetailId2,
        //        ListUrl = new List<string> {
        //                 "https://www.simplilearn.com/ice9/free_resources_article_thumb/what_is_image_Processing.jpg",
        //                "https://thumbs.dreamstime.com/b/beautiful-rain-forest-ang-ka-nature-trail-doi-inthanon-national-park-thailand-36703721.jpg"
        //            }
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        await _detailPMAppService.SubmitRequestMoney(input1);

        //    });
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        //        {

        //            await _detailPMAppService.SubmitRequestMoney(input2);
        //        });

        //        string expectedMessage = "Already have a pending request";
        //        exception.Message.ShouldBe(expectedMessage);
        //    });
        //}
        [Fact]
        public async Task SubmitRequestMoney_WithNoDetailRecordSelect_ShouldThrowException()
        {

            InvoiceRequestDto invoiceRequestDto1 = new InvoiceRequestDto();
            invoiceRequestDto1.Amount = 300000;
            invoiceRequestDto1.HasVat = true;
            invoiceRequestDto1.InvoiceImageName = "0.jpg";
            invoiceRequestDto1.InvoiceUrl = null;
            InvoiceRequestDto invoiceRequestDto2 = new InvoiceRequestDto();
            invoiceRequestDto2.Amount = 400000;
            invoiceRequestDto2.HasVat = false;
            invoiceRequestDto2.InvoiceImageName = null;
            invoiceRequestDto2.InvoiceUrl = "https://www.tutorialsteacher.com/articles/convert-object-to-json-in-csharp";
            PMRequestDto pMRequestDto = new PMRequestDto();
            pMRequestDto.InvoiceAmount = 700000;
            pMRequestDto.ListDetailId = new List<long>();
            pMRequestDto.ListInvoiceRequestDto = new List<InvoiceRequestDto> { invoiceRequestDto1, invoiceRequestDto2 };
            pMRequestDto.Note = "Note 22/08/2023";
            pMRequestDto.TitleRequest = "My Title Request";

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = JsonSerializer.Serialize<PMRequestDto>(pMRequestDto),
                    ListFile = new List<IFormFile> { mockFile1 },
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });
                exception.Message.ShouldBe("You need to select at least one record!");
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithNoPMRequestDto_ShouldThrowException()
        {

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = null,
                    ListFile = new List<IFormFile> { mockFile1 },
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });
                exception.Message.ShouldBe("Request is Invalid!");
            });
        }

        [Fact]
        public async Task SubmitRequestMoney_WithInvalidFormatPMRequestDto_ShouldThrowException()
        {

            // Create a mock IFormFile object
            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("0.jpg");
            mockFile1.Length.Returns(1024);
            mockFile1.ContentType.Returns("image/jpeg");

            await WithUnitOfWorkAsync(async () =>
            {
                SubmitRequestMoneyDto input = new SubmitRequestMoneyDto
                {
                    PMRequest = "PMRequest",
                    ListFile = new List<IFormFile> { mockFile1 },
                };
                var exception = await Assert.ThrowsAsync<Newtonsoft.Json.JsonSerializationException>(async () =>
                {
                    await _detailPMAppService.SubmitRequestMoney(input);
                });

                exception.Message.ShouldBe("Request DTO Format is Invalid!");
            });
        }

    }
}