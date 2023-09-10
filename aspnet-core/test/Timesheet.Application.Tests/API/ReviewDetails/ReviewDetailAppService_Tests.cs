using Abp.BackgroundJobs;
using Ncc.IoC;
using Timesheet.APIs.ReviewDetails;
using Timesheet.Services.File;
using Timesheet.Services.HRM;
using Timesheet.Services.Project;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Xunit;
using Timesheet.Paging;
using Shouldly;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Timesheet.APIs.ReviewDetails.Dto;
using Ncc.Entities.Enum;
using Timesheet.Entities;
using System.Linq;
using Abp.UI;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Runtime.Session;
using Timesheet.APIs.ReviewInternCapabilities.Dto;

namespace Timesheet.Application.Tests.API.ReviewDetails
{
    public class ReviewDetailAppService_Tests : TimesheetApplicationTestBase
    {
        //Summary: 18/21 function, 50 test case passed
        private readonly ReviewDetailAppService _reviewDetailAppService;
        private readonly IWorkScope _workScope;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ExportFileService _fileService;
        private readonly ProjectService _projectService;
        private readonly HRMService _hRMService;

        public ReviewDetailAppService_Tests()
        {
            var _configuration = Substitute.For<IConfiguration>();
            var _httpClient = Resolve<HttpClient>();

            _workScope = Resolve<IWorkScope>();

            _backgroundJobManager = Resolve<IBackgroundJobManager>();

            var _mockEnvironment = Substitute.For<IHostingEnvironment>();
            _mockEnvironment.EnvironmentName.Returns("Hosting:UnitTestEnviroment");
            _mockEnvironment.WebRootPath.Returns("http://www.myserver.com");

            var _loggerFileService = Resolve<ILogger<ExportFileService>>();
            _fileService = new ExportFileService(_loggerFileService);

            var _loggerProjectService = Resolve<ILogger<ProjectService>>();
            _configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://www.myserver.com");
            _configuration.GetValue<string>("ProjectService:SecurityCode").Returns("secretCode");
            _projectService = new ProjectService(_httpClient, _configuration, _loggerProjectService);

            var _loggerHRMService = Resolve<ILogger<HRMService>>();
            _configuration.GetValue<string>("HRMService:BaseAddress").Returns("http://www.myserver.com");
            _configuration.GetValue<string>("HRMService:SecurityCode").Returns("secretCode");
            _hRMService = new HRMService(_httpClient, _configuration, _loggerHRMService);

            _reviewDetailAppService = new ReviewDetailAppService(_backgroundJobManager, _projectService, _hRMService, _mockEnvironment, _fileService, _workScope);

            _reviewDetailAppService.ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>();
            _reviewDetailAppService.SettingManager = Resolve<ISettingManager>();
            _reviewDetailAppService.AbpSession = Resolve<IAbpSession>();

        }

        //Test GetAllDetails funtion
        [Fact]
        public async Task Should_Get_All_Details_By_ReviewId_And_BranchId()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new GridParam
                {
                    SkipCount = 0
                };
                var reviewId = 1;
                var branchId = 4;

                var result = await _reviewDetailAppService.GetAllDetails(input, reviewId, branchId);

                result.ShouldNotBeNull();
                result.TotalCount.ShouldBe(2);
                result.Items.Count.ShouldBe(2);
                foreach (var item in result.Items)
                {
                    item.BranchId.ShouldBe(branchId);
                    item.ReviewId.ShouldBe(reviewId);
                }
            });
        }

        [Fact]
        public async Task Should_Get_All_Details_By_GridPrams()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new GridParam
                {
                    SkipCount = 1,
                };
                var reviewId = 1;
                var branchId = 4;

                var result = await _reviewDetailAppService.GetAllDetails(input, reviewId, branchId);

                result.ShouldNotBeNull();
                result.TotalCount.ShouldBe(2);
                result.Items.Count.ShouldBe(1);
                foreach (var item in result.Items)
                {
                    item.BranchId.ShouldBe(branchId);
                    item.ReviewId.ShouldBe(reviewId);
                }
            });
        }

        [Fact]
        public async Task Should_Get_All_Details_By_SearchText()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new GridParam
                {
                    SearchText = "toan"
                };
                var reviewId = 1;
                var branchId = 4;

                var result = await _reviewDetailAppService.GetAllDetails(input, reviewId, branchId);

                result.ShouldNotBeNull();
                result.TotalCount.ShouldBe(1);
                result.Items.Count.ShouldBe(1);
                foreach (var item in result.Items)
                {
                    item.BranchId.ShouldBe(branchId);
                    item.ReviewId.ShouldBe(reviewId);
                    item.InternName.ShouldContain("Toàn");
                }
            });
        }

        //Test Create funtion
        [Fact]
        public async Task Should_Create_A_Valid_Review_Detail()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.Create(new ReviewDetailInputDto
                {
                    ReviewId = 3,
                    InternshipId = 16,
                    CurrentLevel = StatusEnum.UserLevel.Intern_1,
                    NewLevel = StatusEnum.UserLevel.Intern_2,
                    Status = StatusEnum.ReviewInternStatus.SentEmail,
                    Note = "note test",
                    ReviewerId = 3,
                    PositionId = 1,
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var expectReviewDetail = new ReviewDetail
                {
                    Id = 18,
                    ReviewId = 3,
                    InternshipId = 16,
                    CurrentLevel = StatusEnum.UserLevel.Intern_1,
                    NewLevel = StatusEnum.UserLevel.Intern_2,
                    Status = StatusEnum.ReviewInternStatus.SentEmail,
                    Note = "note test",
                    ReviewerId = 3,
                };

                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(18);
                var allReviewDetail = _workScope.GetAll<ReviewDetail>();

                allReviewDetail.ToList().Count.ShouldBe(18);
                allReviewDetail.ToList().Find(item => item.Id == 18).ShouldNotBeNull();
                reviewDetail.Id.ShouldBe(expectReviewDetail.Id);
                reviewDetail.ReviewId.ShouldBe(expectReviewDetail.ReviewId);
                reviewDetail.ReviewerId.ShouldBe(expectReviewDetail.ReviewerId);
                reviewDetail.InternshipId.ShouldBe(expectReviewDetail.InternshipId);
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Save_A_Review_Detail_With_Not_IsActive()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Create(new ReviewDetailInputDto
                    {
                        Id = -1,
                        ReviewId = 1,
                        InternshipId = 6,
                        CurrentLevel = StatusEnum.UserLevel.Intern_1,
                        NewLevel = StatusEnum.UserLevel.Intern_2,
                        Status = StatusEnum.ReviewInternStatus.SentEmail,
                        Note = "note test",
                        ReviewerId = 3,
                        PositionId = 1,
                    });
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Save_A_Review_Detail_Exsting_InternshipId_Or_ReviewId()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new ReviewDetailInputDto
                {
                    Id = -1,
                    ReviewId = 3,
                    InternshipId = 7,
                    CurrentLevel = StatusEnum.UserLevel.Intern_1,
                    NewLevel = StatusEnum.UserLevel.Intern_2,
                    Status = StatusEnum.ReviewInternStatus.SentEmail,
                    Note = "note test",
                    ReviewerId = 3,
                    PositionId = 1,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Create(input);
                });

                exception.Message.ShouldBe("Already exist Intership Id = " + input.InternshipId + " in the review");
            });
        }

        //Test GetUnReviewIntership funtion
        [Fact]
        public async Task Should_Get_Review_Internship()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var reviewId = 2;
                var result = await _reviewDetailAppService.GetUnReviewIntership(reviewId);

                //funtion chính đang return kiểu object nên chưa test được nhiều 
                result.ShouldNotBeNull();
            });
        }

        //Test Delete funtion
        [Fact]
        public async Task Should_Delete_A_Valid_Review_Detail()
        {
            var reviewId = 15;

            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.Delete(new EntityDto<long>
                {
                    Id = reviewId
                });
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Delete_A_Review_Detail_With_Not_IsActive()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var reviewId = 1;
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Delete(new EntityDto<long>
                    {
                        Id = reviewId
                    });
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Thorw_Exception_When_Delete_A_Review_Detail_With_ReviewInternStatus_Be_Draft()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var reviewId = 10;
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Delete(new EntityDto<long>
                    {
                        Id = reviewId
                    });
                });

                exception.Message.ShouldBe("Bạn không thể xóa tts này vì tts này đã được review");
            });
        }

        //Test Update funtion 
        [Fact]
        public async Task Should_Update_A_Valid_Review_Detail()
        {
            var reviewDetail = new ReviewDetailInputDto
            {
                Id = 15,
                ReviewId = 3,
                InternshipId = 7,
                CurrentLevel = StatusEnum.UserLevel.Intern_1,
                NewLevel = StatusEnum.UserLevel.Intern_2,
                Status = StatusEnum.ReviewInternStatus.SentEmail,
                Note = "note test",
                ReviewerId = 3,
                PositionId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.Update(reviewDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetailResponse = await _workScope.GetAsync<ReviewDetail>(reviewDetail.Id);

                reviewDetailResponse.Id.ShouldBe(reviewDetail.Id);
                reviewDetailResponse.ReviewId.ShouldBe(reviewDetail.ReviewId);
                reviewDetailResponse.ReviewerId.ShouldBe(reviewDetail.ReviewerId);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Update_A_Review_Detail_With_Invalid_ReviewInternStatus()
        {
            var reviewDetail = new ReviewDetailInputDto
            {
                Id = 10,
                ReviewId = 3,
                InternshipId = 7,
                CurrentLevel = StatusEnum.UserLevel.Intern_1,
                NewLevel = StatusEnum.UserLevel.Intern_2,
                Status = StatusEnum.ReviewInternStatus.SentEmail,
                Note = "note test",
                ReviewerId = 3,
                PositionId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Update(reviewDetail);
                });

                exception.Message.ShouldBe("Bạn không thể sửa vì trạng thái là " + reviewDetail.Status);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Update_A_Review_Detail_With_Not_Active()
        {
            var reviewDetail = new ReviewDetailInputDto
            {
                Id = 10,
                ReviewId = 1,
                InternshipId = 7,
                CurrentLevel = StatusEnum.UserLevel.Intern_1,
                NewLevel = StatusEnum.UserLevel.Intern_2,
                Status = StatusEnum.ReviewInternStatus.SentEmail,
                Note = "note test",
                ReviewerId = 3,
                PositionId = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Update(reviewDetail);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        //Test Get funtion
        [Fact]
        public async Task Should_Get_A_Review_Detail_With_IsFirst4M()
        {
            var reviewDetail = new ReviewDetailDto
            {
                Id = 3,
                ReviewId = 1,
                InternshipId = 12,
                ReviewerId = 3,
                IsFirst4M = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.Get(3);

                result.ShouldNotBeNull();
                result[0].ReviewId.ShouldBe(reviewDetail.ReviewId);
                result[0].ReviewerId.ShouldBe(reviewDetail.ReviewerId);
                result[0].InternshipId.ShouldBe(reviewDetail.InternshipId);
                result[0].IsFirst4M.ShouldBeTrue();
            });
        }

        //Test GetHistories funtion
        [Fact]
        public async Task Should_Get_Histories()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var internshipId = 11;
                var reviewId = 3;
                var result = await _reviewDetailAppService.GetHistories(internshipId, reviewId);

                result.ShouldNotBeNull();
                result.ShouldContain(item => item.InternshipId == internshipId);
                result.ShouldContain(item => item.FromLevel == StatusEnum.UserLevel.Intern_0);
            });
        }

        //Test SendEmail funtion
        [Fact]
        public async Task Should_Send_Email()
        {
            var reviewDetailId = 16;

            await WithUnitOfWorkAsync(async () =>
            {

                var result = await _reviewDetailAppService.SendEmail(reviewDetailId);

                result.ShouldNotBeNull();
                result.GetType().GetProperty("Id").GetValue(result, null).ShouldBe(reviewDetailId);
                result.GetType().GetProperty("InternEmail").GetValue(result, null).ShouldBe("toan.nguyenduc@ncc.asia");
                result.GetType().GetProperty("ReviewerName").GetValue(result, null).ShouldBe("Tiến Nguyễn Hữu");
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(reviewDetailId);

                reviewDetail.Status.ShouldBe(StatusEnum.ReviewInternStatus.SentEmail);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Send_Email_Test_Case_1()
        {
            var reviewDetailId = -100;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.SendEmail(reviewDetailId);
                });

                exception.Message.ShouldBe("Review detail Id = " + reviewDetailId + " not exist");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Send_Email_Test_Case_2()
        {
            var reviewDetailId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.SendEmail(reviewDetailId);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Send_Email_Test_Case_3()
        {
            var reviewDetailId = 10;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.SendEmail(reviewDetailId);
                });

                exception.Message.ShouldBe("TTS này chưa được approve review hoặc đã gửi mail");
            });
        }

        //Test Approve funtion
        [Fact]
        public async Task Should_Approve_Valid_Review_Detail()
        {
            var reviewDetailId = 17;
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.Approve(reviewDetailId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(reviewDetailId);

                reviewDetail.Status.ShouldBe(StatusEnum.ReviewInternStatus.Approved);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Approve_Review_Detail()
        {
            var reviewDetailId = 10;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Approve(reviewDetailId);
                });

                exception.Message.ShouldBe("Review này chưa được review hoặc đã review xong");
            });
        }

        //Test Reject funtion
        [Fact]
        public async Task Should_Reject_Valid_Review_Detail()
        {
            var reviewDetailId = 17;
            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.Reject(reviewDetailId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(reviewDetailId);

                reviewDetail.Status.ShouldBe(StatusEnum.ReviewInternStatus.Rejected);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Reject_Review_Detail()
        {
            var reviewDetailId = 10;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.Reject(reviewDetailId);
                });

                exception.Message.ShouldBe("Review này chưa được review hoặc đã review xong");
            });
        }

        //Test RejectSentMail funtion
        [Fact]
        public async Task Should_Reject_Send_Email()
        {
            var reviewDetailId = 10;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.RejectSentMail(reviewDetailId);

                result.ShouldNotBeNull();
                result.GetType().GetProperty("Id").GetValue(result, null).ShouldBe(reviewDetailId);
                result.GetType().GetProperty("InternEmail").GetValue(result, null).ShouldBe("toan.nguyenduc@ncc.asia");
                result.GetType().GetProperty("ReviewerName").GetValue(result, null).ShouldBe("Tiến Nguyễn Hữu");
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(reviewDetailId);
                reviewDetail.Status.ShouldBe(StatusEnum.ReviewInternStatus.Rejected);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Reject_Send_Email_Test_Case_1()
        {
            var reviewDetailId = -100;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.RejectSentMail(reviewDetailId);
                });

                exception.Message.ShouldBe("Review detail Id = " + reviewDetailId + " not exist");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Reject_Send_Email_Test_Case_2()
        {
            var reviewDetailId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.RejectSentMail(reviewDetailId);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Reject_Send_Email_Test_Case_3()
        {
            var reviewDetailId = 15;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.RejectSentMail(reviewDetailId);
                });

                exception.Message.ShouldBe("TTS này chưa được gửi mail");
            });
        }

        //Test funtion GetLevelSetting
        [Fact]
        public async Task Should_Get_Level_Setting()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.GetLevelSetting();
                result.ShouldNotBeNull();
                result.Count.ShouldBeGreaterThanOrEqualTo(7);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.Intern_0);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.Intern_1);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.Intern_2);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.Intern_3);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.FresherMinus);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.Fresher);
                result.ShouldContain(item => item.Id == StatusEnum.UserLevel.FresherPlus);
            });
        }

        //Test GetInternCapability funtion
        [Fact]
        public async Task Should_Get_A_Intern_Capability()
        {
            var expectReviewDetail = new NewReviewDetailDto
            {
                Id = 10,
                ReviewId = 3,
                InternshipId = 7,
                ReviewerId = 3,
                IsFirst4M = true,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetailId = 10;
                var result = await _reviewDetailAppService.GetInternCapability(reviewDetailId);

                result.ShouldNotBeNull();
                result[0].Id.ShouldBe(expectReviewDetail.Id);
                result[0].ReviewId.ShouldBe(expectReviewDetail.ReviewId);
                result[0].InternshipId.ShouldBe(expectReviewDetail.InternshipId);
                result[0].ReviewerId.ShouldBe(expectReviewDetail.ReviewerId);
                result[0].IsFirst4M.ShouldBeTrue();
                foreach(var item in result[0].ReviewInternCapabilities)
                {
                    item.ReviewDetailId.ShouldBe(expectReviewDetail.Id);
                };
            });
        }

        //Test DeleteInternCapability funtion
        [Fact]
        public async Task Should_Delete_A_Valid_Intern_Capability()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var reviewDetailId = new EntityDto<long>
                {
                    Id = 15,
                };

                await _reviewDetailAppService.DeleteInternCapability(reviewDetailId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var reviewInternCapability = _workScope.GetAll<ReviewInternCapability>();

                reviewInternCapability.ToList().Find(item => item.ReviewDetailId == 15).ShouldBeNull();
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Delete_Intern_Capability_Test_Case_1()
        {
            var reviewDetailId = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.DeleteInternCapability(reviewDetailId);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Delete_Intern_Capability_Test_Case_2()
        {
            var reviewDetailId = new EntityDto<long>
            {
                Id = 10,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.DeleteInternCapability(reviewDetailId);
                });

                exception.Message.ShouldBe("Không thể xoá internship có trạng thái khác draft hoặc rejected");
            });
        }

        //Test CreateInternCapability funtion
        [Fact]
        public async Task Should_Create_A_Valid_Intern_Capability()
        {
            var expectReviewDetail = new ReviewDetailInputDto
            {
                ReviewId = 3,
                InternshipId = 8,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _reviewDetailAppService.CreateInternCapability(expectReviewDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var insertedReviewId = 18;
                var reviewDetail = await _workScope.GetAsync<ReviewDetail>(insertedReviewId);
                var allReviewInternCapability = _workScope.GetAll<ReviewInternCapability>();


                reviewDetail.ReviewId.ShouldBe(expectReviewDetail.ReviewId);
                reviewDetail.InternshipId.ShouldBe(expectReviewDetail.InternshipId);
                allReviewInternCapability.ShouldContain(item => item.ReviewDetailId == 18);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Create_Intern_Capability_Test_Case_1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.CreateInternCapability(new ReviewDetailInputDto
                    {
                        ReviewId = 1,
                    });
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Create_Intern_Capability_Test_Case_2()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new ReviewDetailInputDto
                {
                    ReviewId = 3,
                    InternshipId = 7,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.CreateInternCapability(input);
                });

                exception.Message.ShouldBe("Already exist Intership Id = " + input.InternshipId + " in the review");
            });
        }

        //Test PMReview funtion
        [Fact]
        public async Task Should_PM_Review_With_New_Level_Less_FresherMinus()
        {
            var expectPMReviewDetail = new PMReviewDetailDto
            {
                Id = 18,
                NewLevel = StatusEnum.UserLevel.Intern_3,
                Note = "Note test",
                Type = StatusEnum.Usertype.Internship,
            };

            //Insert thêm 1 bản ghi để test 
            await WithUnitOfWorkAsync(async () =>
            {
                await _workScope.InsertAndGetIdAsync(new ReviewDetail
                {
                    ReviewId  = 3,
                    InternshipId = 6,
                    ReviewerId = 1,
                    CurrentLevel = StatusEnum.UserLevel.Intern_0,
                    NewLevel= StatusEnum.UserLevel.Intern_1,
                    Status = StatusEnum.ReviewInternStatus.Reviewed,
                    Note = "Note ok",
                    Type = StatusEnum.Usertype.Internship
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.PMReview(expectPMReviewDetail);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
                result.Type.ShouldBe(expectPMReviewDetail.Type);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<ReviewDetail>(expectPMReviewDetail.Id);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
                result.Type.ShouldBe(expectPMReviewDetail.Type);
            });
        }

        [Fact]
        public async Task Should_PM_Review_With_New_Level_Greater_Or_Equal_FresherMinus()
        {
            var expectPMReviewDetail = new PMReviewDetailDto
            {
                Id = 18,
                NewLevel = StatusEnum.UserLevel.FresherMinus,
                SubLevel = 2,
                Note = "Note test",
                Type = StatusEnum.Usertype.Internship,
                IsFullSalary = true,
            };

            //Insert thêm 1 bản ghi để test 
            await WithUnitOfWorkAsync(async () =>
            {
                await _workScope.InsertAndGetIdAsync(new ReviewDetail
                {
                    ReviewId = 3,
                    InternshipId = 6,
                    ReviewerId = 1,
                    CurrentLevel = StatusEnum.UserLevel.Intern_0,
                    NewLevel = StatusEnum.UserLevel.Intern_1,
                    Status = StatusEnum.ReviewInternStatus.Reviewed,
                    Note = "Note ok",
                    Type = StatusEnum.Usertype.Internship
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.PMReview(expectPMReviewDetail);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(expectPMReviewDetail.SubLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
                result.Type.ShouldBe(expectPMReviewDetail.Type);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<ReviewDetail>(expectPMReviewDetail.Id);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(expectPMReviewDetail.SubLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
                result.Type.ShouldBe(expectPMReviewDetail.Type);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_PM_Review_Test_Case_1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = -100,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Review detail Id = " + input.Id + " not exist");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_PM_Review_Test_Case_2()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = 1,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_PM_Review_Test_Case_3()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = 17,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Bạn không thể review TTS của PM khác");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_PM_Review_Test_Case_4()
        {
            //Insert thêm 1 bản ghi để test 
            await WithUnitOfWorkAsync(async () =>
            {
                await _workScope.InsertAndGetIdAsync(new ReviewDetail
                {
                    ReviewId = 3,
                    InternshipId = 6,
                    ReviewerId = 1,
                    CurrentLevel = StatusEnum.UserLevel.Intern_0,
                    NewLevel = StatusEnum.UserLevel.Intern_1,
                    Status = StatusEnum.ReviewInternStatus.Approved,
                    Note = "Note ok",
                    Type = StatusEnum.Usertype.Internship
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = 18,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Bạn không thể sửa vì kết quả review cho tts này đã được gửi mail");
            });
        }

        //Test ConfirmApproveSalary funtion
        [Fact]
        public async Task Should_Confirm_Approve_Salary()
        {
            var expectPMReviewDetail = new ReviewDetailDto
            {
                Id = 18,
                Note = "Note test",
                Type = StatusEnum.Usertype.Collaborators,
                NewLevel = StatusEnum.UserLevel.FresherMinus,
                SubLevel = 2,
                IsFullSalary = true,
            };

            //Insert thêm 1 bản ghi để test 
            await WithUnitOfWorkAsync(async () =>
            {
                await _workScope.InsertAndGetIdAsync(new ReviewDetail
                {
                    ReviewId = 3,
                    InternshipId = 6,
                    ReviewerId = 1,
                    CurrentLevel = StatusEnum.UserLevel.Intern_3,
                    NewLevel = StatusEnum.UserLevel.FresherMinus,
                    Status = StatusEnum.ReviewInternStatus.Reviewed,
                    Note = "Note ok",
                    Type = StatusEnum.Usertype.Collaborators
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.ConfirmApproveSalary(expectPMReviewDetail);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(expectPMReviewDetail.SubLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
                result.Type.ShouldBe(expectPMReviewDetail.Type);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<ReviewDetail>(expectPMReviewDetail.Id);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(expectPMReviewDetail.Id);
                result.NewLevel.ShouldBe(expectPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(expectPMReviewDetail.SubLevel);
                result.Note.ShouldBe(expectPMReviewDetail.Note);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Confirm_Approve_Salary_Test_Case_1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = -100,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Review detail Id = " + input.Id + " not exist");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Confirm_Approve_Salary_Test_Case_2()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new PMReviewDetailDto
                {
                    Id = 1,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                    Note = "Note test",
                    Type = StatusEnum.Usertype.Internship,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReview(input);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        //Test PMReviewInternCapability funtion
        [Fact]
        public async Task Should_PM_Review_Intern_Capability_With_New_Level_Less_FresherMinus()
        {
            var newPMReviewDetail = new NewPMReviewDetailDto
            {
                Id = 14,
                NewLevel = StatusEnum.UserLevel.Intern_3,
                reviewInternCapabilities = new System.Collections.Generic.List<ReviewInterCapabilityDto>
                {
                    new ReviewInterCapabilityDto
                    {
                        Id = 54,
                        CapabilityName = "Thời gian làm việc",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 55,
                        CapabilityName = "English",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 56,
                        CapabilityName = "Kinh nghiệm",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 57,
                        CapabilityName = "Angular",
                        CapabilityType = StatusEnum.CapabilityType.Note,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.PMReviewInternCapability(newPMReviewDetail);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(newPMReviewDetail.Id);
                result.NewLevel.ShouldBe(newPMReviewDetail.NewLevel);
                result.reviewInternCapabilities.Count.ShouldBe(newPMReviewDetail.reviewInternCapabilities.Count);
                result.reviewInternCapabilities.ShouldAllBe(item => item.ReviewDetailId == newPMReviewDetail.Id);
                result.reviewInternCapabilities.ShouldContain(item => item.Id == newPMReviewDetail.reviewInternCapabilities[0].Id);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<ReviewInternCapability>().Where(s => s.ReviewDetailId == newPMReviewDetail.Id).ToList();

                result.Count.ShouldBe(newPMReviewDetail.reviewInternCapabilities.Count);
                result.ShouldAllBe(item => item.ReviewDetailId == newPMReviewDetail.Id);
                result.ShouldContain(item => item.Id == newPMReviewDetail.reviewInternCapabilities[0].Id);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<ReviewDetail>(newPMReviewDetail.Id);

                var expectSalary = 4000000;

                result.Id.ShouldBe(newPMReviewDetail.Id);
                result.NewLevel.ShouldBe(newPMReviewDetail.NewLevel);
                result.Salary.ShouldBe(expectSalary);
            });
        }

        [Fact]
        public async Task Should_PM_Review_Intern_Capability_With_New_Level_Greater_Or_Equal_Fresher_Minus()
        {
            var newPMReviewDetail = new NewPMReviewDetailDto
            {
                Id = 14,
                NewLevel = StatusEnum.UserLevel.FresherMinus,
                SubLevel = 2,
                IsFullSalary = true,
                reviewInternCapabilities = new System.Collections.Generic.List<ReviewInterCapabilityDto>
                {
                    new ReviewInterCapabilityDto
                    {
                        Id = 54,
                        CapabilityName = "Thời gian làm việc",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 55,
                        CapabilityName = "English",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 56,
                        CapabilityName = "Kinh nghiệm",
                        CapabilityType = StatusEnum.CapabilityType.Point,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                    new ReviewInterCapabilityDto
                    {
                        Id = 57,
                        CapabilityName = "Angular",
                        CapabilityType = StatusEnum.CapabilityType.Note,
                        ReviewDetailId = 14,
                        Note = "Note test"
                    },
                }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _reviewDetailAppService.PMReviewInternCapability(newPMReviewDetail);

                result.ShouldNotBeNull();
                result.Id.ShouldBe(newPMReviewDetail.Id);
                result.NewLevel.ShouldBe(newPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(newPMReviewDetail.SubLevel);
                result.reviewInternCapabilities.Count.ShouldBe(newPMReviewDetail.reviewInternCapabilities.Count);
                result.reviewInternCapabilities.ShouldAllBe(item => item.ReviewDetailId == newPMReviewDetail.Id);
                result.reviewInternCapabilities.ShouldContain(item => item.Id == newPMReviewDetail.reviewInternCapabilities[0].Id);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<ReviewInternCapability>().Where(s => s.ReviewDetailId == newPMReviewDetail.Id).ToList();

                result.Count.ShouldBe(newPMReviewDetail.reviewInternCapabilities.Count);
                result.ShouldAllBe(item => item.ReviewDetailId == newPMReviewDetail.Id);
                result.ShouldContain(item => item.Id == newPMReviewDetail.reviewInternCapabilities[0].Id);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<ReviewDetail>(newPMReviewDetail.Id);

                var expectSalary = 7000000;

                result.Id.ShouldBe(newPMReviewDetail.Id);
                result.NewLevel.ShouldBe(newPMReviewDetail.NewLevel);
                result.SubLevel.ShouldBe(newPMReviewDetail.SubLevel);
                result.Salary.ShouldBe(expectSalary);
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Review_Intern_Capability_Test_Case_1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NewPMReviewDetailDto
                {
                    Id = -100,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReviewInternCapability(input);
                });

                exception.Message.ShouldBe("Review detail Id = " + input.Id + " not exist");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Review_Intern_Capability_Test_Case_2()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NewPMReviewDetailDto
                {
                    Id = 1,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReviewInternCapability(input);
                });

                exception.Message.ShouldBe("Review Phase not Active");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Review_Intern_Capability_Test_Case_3()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NewPMReviewDetailDto
                {
                    Id = 10,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReviewInternCapability(input);
                });

                exception.Message.ShouldBe("Bạn không thể review TTS của PM khác");
            });
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Review_Intern_Capability_Test_Case_4()
        {
            //Insert thêm 1 bản ghi để test 
            await WithUnitOfWorkAsync(async () =>
            {
                await _workScope.InsertAndGetIdAsync(new ReviewDetail
                {
                    ReviewId = 3,
                    InternshipId = 6,
                    ReviewerId = 1,
                    CurrentLevel = StatusEnum.UserLevel.Intern_0,
                    NewLevel = StatusEnum.UserLevel.Intern_1,
                    Status = StatusEnum.ReviewInternStatus.Approved,
                    Note = "Note ok",
                    Type = StatusEnum.Usertype.Internship
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var input = new NewPMReviewDetailDto
                {
                    Id = 18,
                    NewLevel = StatusEnum.UserLevel.Intern_3,
                };

                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _reviewDetailAppService.PMReviewInternCapability(input);
                });

                exception.Message.ShouldBe("Bạn không thể sửa vì kết quả review cho tts này đã được gửi mail");
            });
        }
    }
}
