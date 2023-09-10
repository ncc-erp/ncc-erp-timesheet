using Abp.BackgroundJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Timesheet.APIs.ReviewInterns;
using Timesheet.Services.HRM;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project;
using Ncc.IoC;
using Xunit;
using System.Threading.Tasks;
using Timesheet.APIs.ReviewInterns.Dto;
using Shouldly;
using Abp.Configuration;
using Abp.Runtime.Session;
using Timesheet.Entities;
using System.Linq.Dynamic.Core;
using System.Linq;
using Abp.UI;
using Abp.Application.Services.Dto;
using Timesheet.Extension;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Domain.Uow;
using System.Collections.Generic;
using Timesheet.DynamicFilter;
using Timesheet.Paging;
using Timesheet.APIs.ReviewDetails;
using Timesheet.Services.File;
using Microsoft.AspNetCore.Hosting;

namespace Timesheet.Application.Tests.API.ReviewInterns
{
    public class ReviewInternAppService_Tests : TimesheetApplicationTestBase
    {
        /// <summary>
        /// 9/17 functions
        /// 14/14 test cases passed
        /// update day 12/01/2023
        /// </summary>
        private ReviewInternAppService _reviewInternAppService;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly HRMService _hRMService;
        private readonly HRMv2Service _hRMv2Service;
        private readonly ProjectService _projectService;
        private readonly IWorkScope _workScope;
        private readonly ReviewDetailAppService _reviewDetailAppService;
        private readonly ExportFileService _fileService;

        public ReviewInternAppService_Tests()
        {
            var _httpClient = Resolve<HttpClient>();
            var _configuration = Substitute.For<IConfiguration>();

            _backgroundJobManager = Resolve<IBackgroundJobManager>();

            var _loggerHRMService = Resolve<ILogger<HRMService>>();
            _configuration.GetValue<string>("HRMService:BaseAddress").Returns("http://www.myserver.com");
            _configuration.GetValue<string>("HRMService:SecurityCode").Returns("secretCode");
            _hRMService = new HRMService(_httpClient, _configuration, _loggerHRMService);

            var _loggerHRMv2Service = Resolve<ILogger<HRMv2Service>>();
            _configuration.GetValue<string>("HRMv2Service:BaseAddress").Returns("http://www.myserver.com");
            _configuration.GetValue<string>("HRMv2Service:SecurityCode").Returns("secretCode");
            _hRMv2Service = new HRMv2Service(_httpClient, _configuration, _loggerHRMv2Service);

            var _loggerProjectService = Resolve<ILogger<ProjectService>>();
            _configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://www.myserver.com");
            _configuration.GetValue<string>("ProjectService:SecurityCode").Returns("secretCode");
            _projectService = new ProjectService(_httpClient, _configuration, _loggerProjectService);

            var _mockEnvironment = Substitute.For<IHostingEnvironment>();
            _mockEnvironment.EnvironmentName.Returns("Hosting:UnitTestEnviroment");
            _mockEnvironment.WebRootPath.Returns("http://www.myserver.com");

            var _loggerFileService = Resolve<ILogger<ExportFileService>>();
            _fileService = new ExportFileService(_loggerFileService);

            _workScope = Resolve<IWorkScope>();

            _reviewDetailAppService = new ReviewDetailAppService(_backgroundJobManager, _projectService, _hRMService, _mockEnvironment, _fileService, _workScope);
            _reviewDetailAppService.ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>();
            _reviewDetailAppService.SettingManager = Resolve<ISettingManager>();
            _reviewDetailAppService.AbpSession = Resolve<IAbpSession>();

            _reviewInternAppService = new ReviewInternAppService(_backgroundJobManager, _hRMv2Service, _workScope, _projectService, _hRMService, _reviewDetailAppService);
            _reviewInternAppService.ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>();
            _reviewInternAppService.SettingManager = Resolve<ISettingManager>();
            _reviewInternAppService.AbpSession = Resolve<IAbpSession>();
            _reviewInternAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
        }

        // Test funtion Create
        [Fact]
        public async Task Should_Create_A_Valid_Review_Intern()
        {
            var expectReviewIntern = new ReviewInternDto
            {
                Id = 5,
                Month = 12,
                Year = 2022,
                IsActive= true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewInternAppService.Create(new ReviewInternDto
                {
                    Month = 12,
                    Year = 2022,
                    IsActive = true,
                });

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectReviewIntern.Id);
                result.Month.ShouldBe(expectReviewIntern.Month);
                result.IsActive.ShouldBe(expectReviewIntern.IsActive);
                result.Year.ShouldBe(expectReviewIntern.Year);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewIntern = await _workScope.GetAsync<ReviewIntern>(expectReviewIntern.Id);

                reviewIntern.ShouldNotBeNull();
                reviewIntern.Id.ShouldBe(expectReviewIntern.Id);
                reviewIntern.Month.ShouldBe(expectReviewIntern.Month);
                reviewIntern.IsActive.ShouldBe(expectReviewIntern.IsActive);
                reviewIntern.Year.ShouldBe(expectReviewIntern.Year);
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Create_Review_Intern()
        {
            var input = new ReviewInternDto
            {
                Month = 10,
                Year = 2022,
                IsActive = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewInternAppService.Create(input);
                });

                exception.Message.ShouldBe("Review Intern already exist row at Year = " + input.Year + "  month = " + input.Month);
            });
        }

        // Test funtion GetAll
        [Fact]
        public async Task Should_Get_All_Review_Intern()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var year = 2022;

                var result = await _reviewInternAppService.GetAll(year);

                // Những bản ghi có trường isDelete = true sẽ không get được
                result.Count.ShouldBeGreaterThanOrEqualTo(2);
                foreach(var item in result)
                {
                    item.Year.ShouldBe(year);
                };
            });   
        }

        // Test funtion Delete
        [Fact]
        public async Task Should_Delete_A_Valid_Review_Intern()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new EntityDto<long>
                {
                    Id = 4,
                };

                await _reviewInternAppService.Delete(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allReviewDetail = _workScope.GetAll<ReviewIntern>();

                allReviewDetail.ToList().Find(item => item.Id == 4).ShouldBeNull();
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Delete_A_Review_Intern()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new EntityDto<long>
                {
                    Id = 3,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewInternAppService.Delete(input);
                });

                exception.Message.ShouldBe("Bạn không thể xóa đợt review tts này vì đã có tts được review");
            });
        }

        //Test funtion GetLevelSetting
        [Fact]
        public async Task Should_Get_Level_Setting()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewInternAppService.GetLevelSetting();

                result.ShouldNotBeNull();
                result.Count.ShouldBeGreaterThanOrEqualTo(7);
                result.ShouldContain(item => item.Id == UserLevel.Intern_0);
                result.ShouldContain(item => item.Id == UserLevel.Intern_1);
                result.ShouldContain(item => item.Id == UserLevel.Intern_2);
                result.ShouldContain(item => item.Id == UserLevel.Intern_3);
                result.ShouldContain(item => item.Id == UserLevel.FresherMinus);
                result.ShouldContain(item => item.Id == UserLevel.Fresher);
                result.ShouldContain(item => item.Id == UserLevel.FresherPlus);
            });
        }

        //Test funtion Active
        [Fact]
        public async Task Should_Active_Review_Intern()
        {
            var reviewInternId = 1;
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewInternAppService.Active(reviewInternId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewIntern = await _workScope.GetAsync<ReviewIntern>(reviewInternId);

                reviewIntern.IsActive.ShouldBeTrue();
            });
        }

        //Test funtion DeActive
        [Fact]
        public async Task Should_DeActive_Review_Intern()
        {
            var reviewInternId = 3;
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewInternAppService.DeActive(reviewInternId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewIntern = await _workScope.GetAsync<ReviewIntern>(reviewInternId);

                reviewIntern.IsActive.ShouldBeFalse();
            });
        }

        //Test funtion ApproveAll
        [Fact]
        public async Task Should_Approve_All_Review_Detail()
        {
            List<long> internShiptIds = new List<long>(3);
            long reviewId = 3;
            var inputFilter = new GridParam()
            {
                FilterItems = new List<ExpressionFilter>(),
                SearchText = "",
                SkipCount = 0
            };
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewInternAppService.ApproveAll(reviewId, inputFilter, null);
            });

            WithUnitOfWork(() =>
            {
                var details = _workScope.GetAll<ReviewDetail>().Where(x => internShiptIds.Contains(x.InternshipId));
                details.ShouldNotContain(item => item.Status == ReviewInternStatus.Reviewed);
            });
        }

        //Test funtion GetReport
        [Fact]
        public async Task Should_Get_Report_With_Level_Up()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var year = 2022;
                var month = 10;
                var keyword = "";
                var level = "level-up";
                var isCurrentInternOnly = false;

                var result = await _reviewInternAppService.GetReport(year, month, keyword, level, isCurrentInternOnly);

                var monthsOfYear = 12;

                result.ShouldNotBeNull();
                result.listMonth.Count.ShouldBe(monthsOfYear);
                result.listInternLevel.Count.ShouldBeGreaterThanOrEqualTo(3);
                result.listInternLevel.ShouldContain(item => item.InternName == "Thành Trần Tiến");
                result.listInternLevel.ShouldContain(item => item.Level == UserLevel.FresherMinus);
                result.listInternLevel.ShouldContain(item => item.Type == Usertype.Collaborators);
            });
        }

        [Fact]
        public async Task Should_Get_Report_With_Level_Not_Change()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var year = 2022;
                var month = 10;
                var keyword = "";
                var level = "level-not-change";
                var isCurrentInternOnly = false;

                var result = await _reviewInternAppService.GetReport(year, month, keyword, level, isCurrentInternOnly);

                var monthsOfYear = 12;

                result.ShouldNotBeNull();
                result.listMonth.Count.ShouldBe(monthsOfYear);
                result.listInternLevel.Count.ShouldBeGreaterThanOrEqualTo(2);
                result.listInternLevel.ShouldContain(item => item.InternName == "Toàn Nguyễn Đức");
                result.listInternLevel.ShouldContain(item => item.Level == UserLevel.Intern_3);
                result.listInternLevel.ShouldContain(item => item.Type == Usertype.Internship);
            });
        }

        //Test funtion CreateInternCapability
        [Fact]
        public async Task Should_Create_A_Valid_Intern_Capability()
        {
            var expectReviewIntern = new ReviewInternDto
            {
                Month = 12,
                Year = 2022,
                IsActive= true,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewInternAppService.CreateInternCapability(expectReviewIntern);

                result.Count.ShouldBe(0);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewIntern = await _workScope.GetAsync<ReviewIntern>(5);

                reviewIntern.ShouldNotBeNull();
                reviewIntern.Id.ShouldBe(expectReviewIntern.Id);
                reviewIntern.Month.ShouldBe(expectReviewIntern.Month);
                reviewIntern.Year.ShouldBe(expectReviewIntern.Year);
                reviewIntern.IsActive.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Create_Intern_Capability()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new ReviewInternDto
                {
                    Month = 10,
                    Year = 2022,
                    IsActive = true,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewInternAppService.CreateInternCapability(input);
                });

                exception.Message.ShouldBe("Review Intern already exist row at Year = " + input.Year + "  month = " + input.Month);
            });
        }

        //Test funtion DeleteInternCapability
        [Fact]
        public async Task Should_Delete_A_Valid_Intern_Capability()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new EntityDto<long>
                {
                    Id = 4,
                };
                Should.NotThrow(async () => { 
                    await _reviewInternAppService.DeleteInternCapability(input);
                });
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Delete_Intern_Capability()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new EntityDto<long>
                {
                    Id = 3,
                };
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewInternAppService.DeleteInternCapability(input);
                });

                exception.Message.ShouldBe("Bạn không thể xóa đợt review tts này vì đã có tts được review");
            });
        }

    }
}
