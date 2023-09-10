using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Abp.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Word;
using Ncc.Authorization.Users;
using Ncc.Editions;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using Ncc.MultiTenancy;
using Ncc.Net.MimeTypes;
using NSubstitute;
using OfficeOpenXml;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using Timesheet.APIs.Positions;
using Timesheet.APIs.RetroDetails;
using Timesheet.APIs.RetroDetails.Dto;
using Timesheet.APIs.Retros;
using Timesheet.APIs.Retros.Dto;
using Timesheet.Constants;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Timesheets.Tasks;
using Timesheet.UploadFilesService;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.HRMV2
{

    public class RetroResultAppService_Test : TimesheetApplicationTestBase
    {
        /// <summary>
        /// 13/19 function public
        /// 36/37 test cases passed (1 fail due to null api)
        /// update day 11/01/2023
        /// </summary>
        private List<RetroResult> listNewRetroResult = new List<RetroResult>
        {
            new RetroResult
            {
                Id=99,
                RetroId = 1,
                Point = 3,
                Note="Oke",
                UserId=1,
                UserType = Usertype.Staff,
                UserLevel = UserLevel.SeniorPlus,
                PositionId = 4,
                ProjectId = 1,
                BranchId = 1
            },
            new RetroResult
            {
                Id=110,
                RetroId = 1,
                Point = 4,
                Note="Oke",
                UserId=1,
                UserType = Usertype.Staff,
                UserLevel = UserLevel.Fresher,
                PositionId = 1,
                ProjectId = 4,
                BranchId = 6
            }
        };
        private readonly RetroResultAppService _retroResultAppService;
        private readonly IWorkScope _workScope;

        public RetroResultAppService_Test()
        {
            _workScope = Resolve<IWorkScope>();

            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://www.myserver.com/");
            configuration.GetValue<string>("ProjectService:SecurityCode").Returns("secretCode");
            var logger = Resolve<ILogger<ProjectService>>();
            var _projectService = Substitute.For<ProjectService>(httpClient, configuration, logger);

            _retroResultAppService = Substitute.For<RetroResultAppService>(_projectService, _workScope);
            _retroResultAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            _retroResultAppService.ObjectMapper = Resolve<IObjectMapper>();
            _retroResultAppService.AbpSession = AbpSession;

            foreach (var rr in listNewRetroResult)
            {
                _workScope.InsertAsync(rr);
            }
        }

        [Fact]
        //Get All Users Not In The RetroResult By RetroId final
        public async void GetAllUsersNotInTheRetroResultByRetroIdTest1()
        {
            long getRetroId = 1;
            long getProjectId = 1;
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllUsersNotInTheRetroResultByRetroId(getRetroId, getProjectId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(2);
                result.Result.First().FullNameAndEmail.ShouldBe("Toàn Nguyễn Đức - toan.nguyenduc@ncc.asia");
                result.Result.First().UserId.ShouldBe(7);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users Not In The RetroResult By RetroId null retroId
        public async void GetAllUsersNotInTheRetroResultByRetroIdTest2()
        {
            var getRetroId = 1000;
            var getProjectId = 1;
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllUsersNotInTheRetroResultByRetroId(getRetroId, getProjectId);
                result.Result.Count.ShouldBe(7);
                result.Result.First().FullNameAndEmail.ShouldBe("admin admin - admin@aspnetboilerplate.com");
                result.Result.First().UserId.ShouldBe(1);
                result.Result[1].FullNameAndEmail.ShouldBe("Tiến Nguyễn Hữu - tien.nguyenhuu@ncc.asia");
                result.Result[1].UserId.ShouldBe(3);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users Not In The RetroResult By RetroId null ProjectId
        public async void GetAllUsersNotInTheRetroResultByRetroIdTest3()
        {
            var getRetroId = 1;
            var getProjectId = 1000;
            await WithUnitOfWorkAsync(() =>
            {
                var result = _retroResultAppService.GetAllUsersNotInTheRetroResultByRetroId(getRetroId, getProjectId);
                result.Result.Count.ShouldBe(0);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users Not In The RetroResult By RetroId null ProjectId & RetroId
        public async void GetAllUsersNotInTheRetroResultByRetroIdTest4()
        {
            var getRetroId = 1000;
            var getProjectId = 1000;
            await WithUnitOfWorkAsync(() =>
            {
                var result = _retroResultAppService.GetAllUsersNotInTheRetroResultByRetroId(getRetroId, getProjectId);
                result.Result.Count.ShouldBe(0);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users In The RetroResult 
        public async void GetAllUsersTest1()
        {
            var getRetroId = 1;
            var getProjectId = 1;

            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllUsers(getRetroId, getProjectId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(2);
                result.Result.First().FullNameAndEmail.ShouldBe("Toàn Nguyễn Đức - toan.nguyenduc@ncc.asia");
                result.Result.First().UserId.ShouldBe(7);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users In The RetroResult  null retroId
        public async void GetAllUsersTest2()
        {
            var getRetroId = 1000;
            var getProjectId = 1;

            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllUsers(getRetroId, getProjectId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(7);
                result.Result.First().FullNameAndEmail.ShouldBe("admin admin - admin@aspnetboilerplate.com");
                result.Result.First().UserId.ShouldBe(1);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get All Users In The RetroResult  null ProjectId
        public async void GetAllUsersTest3()
        {
            var getRetroId = 1;
            var getProjectId = 1000;

            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllUsers(getRetroId, getProjectId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(0);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }
        
        [Fact]
        //Get All Project
        public async void GetAllProjectTest1()
        {

            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetAllProject();
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(3);
                result.Result.First().Code.ShouldBe("Project UCG");
                result.Result.First().Id.ShouldBe(1);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get Project PM Retro
        public async void GetProjectPMRetroTest1()
        {
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetProjectPMRetro();
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(3);
                result.Result.First().Code.ShouldBe("NCC");
                result.Result.First().Id.ShouldBe(6);
                result.Result.First().Name.ShouldBe("Company activity");
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get Project PM Retro Result
        public async void GetProjectPMRetroResultTest1()
        {
            var getRetroId = 1;
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetProjectPMRetroResult(getRetroId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(9);
                result.Result.First().Code.ShouldBe("Project UCG");
                result.Result.First().Id.ShouldBe(1);
                result.Result.First().Name.ShouldBe("Project UCG");
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Get Project PM Retro Result null RetroId
        public async void GetProjectPMRetroResultTest2()
        {
            var getRetroId = 1000;
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.GetProjectPMRetroResult(getRetroId);
                Assert.NotNull(result);
                result.Result.Count.ShouldBe(0);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Create
        public async void CreateTest1()
        {

            var input = new RetroResultCreateDto
            {
                Id = 0,
                Point = 4,
                Note = "Ok",
                UserId = 15,
                ProjectId = 1,
                PositionId = 4,
                RetroId = 1

            };


            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.Create(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var AllRetroResult = _workScope.GetAll<RetroResult>();
                AllRetroResult.Count().ShouldBe(26);

                var createdRetroResult = await _workScope.GetAsync<RetroResult>(24);
                Assert.NotNull(createdRetroResult);
                Assert.Equal(4, createdRetroResult.Point);
                createdRetroResult.Note.ShouldBe("Ok");
                createdRetroResult.UserId.ShouldBe(15);
                createdRetroResult.ProjectId.ShouldBe(1);
                createdRetroResult.PositionId.ShouldBe(4);
                createdRetroResult.RetroId.ShouldBe(1);
            });
        }

        [Fact]
        //Create userId exsisted
        public async void CreateTest2()
        {

            var input = new RetroResultCreateDto
            {
                Id = 24,
                Point = 4,
                Note = "Ok",
                UserId = 15,
                ProjectId = 2,
                PositionId = 4,
                RetroId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroResultAppService.Create(input);
                });
                Assert.Equal($"This UserId {input.UserId} in ProjectId {input.ProjectId} already in this Month", exception.Message);
            });
        }

        [Fact]
        //Create point out of condition
        public async void CreateTest3()
        {

            var input = new RetroResultCreateDto
            {
                Id = 24,
                Point = 15,
                Note = "Ok",
                UserId = 17,
                ProjectId = 1,
                PositionId = 4,
                RetroId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroResultAppService.Create(input);
                });
                Assert.Equal($"Point can not be less than 0 or greater than 5", exception.Message);
            });
        }

        [Fact]
        //Create null userId (NullReferenceException catch the error first)
        public async void CreateTest4()
        {

            var input = new RetroResultCreateDto
            {
                Point = 3,
                Note = "Ok",
                UserId = 0,
                ProjectId = 1,
                PositionId = 4,
                RetroId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<NullReferenceException>(async () =>
                {
                    await _retroResultAppService.Create(input);
                });
            });
        }

        [Fact]
        //Create null userId (default type,level,branchId)
        public async void CreateTest5()
        {

            var input = new RetroResultCreateDto
            {
                Id = 0,
                Point = 3,
                Note = "Ok",
                UserId = 1,
                ProjectId = 2,
                PositionId = 4,
                RetroId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroResultAppService.Create(input); 
                });
                Assert.Equal($"Type or Level or Branch is null", exception.Message);
            });
        }

        [Fact]
        //Update
        public async void UpdateTest1()
        {

            var input = new RetroResultEditDto
            {
                Id = 23,
                Point = 4,
                Note = "Ok2",
                UserType = Usertype.ProbationaryStaff,
                UserLevel = UserLevel.JuniorMinus,
                ProjectId = 1,
                PositionId = 4,
                BranchId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.Update(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var createdRetroResult = await _workScope.GetAsync<RetroResult>(23);
                Assert.NotNull(createdRetroResult);
                Assert.Equal(input.Point, createdRetroResult.Point);
                createdRetroResult.Note.ShouldBe(input.Note);
                Assert.Equal(createdRetroResult.UserLevel, input.UserLevel);
                Assert.Equal(createdRetroResult.UserType, input.UserType);
                createdRetroResult.PositionId.ShouldBe(input.PositionId);
                createdRetroResult.BranchId.ShouldBe(input.BranchId);
            });
        }

        [Fact]
        //Update null Id
        public async void UpdateTest2()
        {

            var input = new RetroResultEditDto
            {
                Id = 230,
                Point = 4,
                Note = "Ok2",
                UserType = Usertype.ProbationaryStaff,
                UserLevel = UserLevel.JuniorMinus,
                ProjectId = 1,
                PositionId = 4,
                BranchId = 1

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                     await _retroResultAppService.Update(input);
                });
            });
        }

        [Fact]
        //Delete
        public async void DeleteTest1()
        {

            EntityDto<long> input = new EntityDto<long>
            {
                Id = 22
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroResultAppService.Delete(input);
            });

            await WithUnitOfWorkAsync(() =>
            {
                var AllRetroResult = _workScope.GetAll<RetroResult>();
                AllRetroResult.Count().ShouldBe(24);
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        //Delete null Id (entitynotfoundexception catch the error first)
        public async void DeleteTest2()
        {

            EntityDto<long> input = new EntityDto<long>
            {
                Id = 221
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _retroResultAppService.Delete(input);
                });
            });
        }

        /*[Fact]
        //Download Template Import Retro
        public async void DownloadTemplateImportRetroTest1()
        {
            await WithUnitOfWorkAsync(() =>
            {
                var result = _retroResultAppService.DownloadTemplateImportRetro();
                Assert.NotNull(result);
                result.Result.FileName.ShouldBe("Template");
                result.Result.FileType.ShouldBe(MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
                result.Result.Base64.ShouldNotBeNull();
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }*/
        //TODO: fail due to wrong path for template

        /*[Fact]
        //Export Retro Result
        public async void ExportRetroResultTest1()
        {
            var getRetroId = 1;
            var input = new InputMultiFilterRetroResultPagingDto
            {
                GridParam = new GridParam { },
                ProjecIds = new List<long> { 1, 4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1, 3, 7, 8, 6 },
                PositionIds = new List<long> { 4, 1 },
                LeftPoint = 5,
                RightPoint = 4
            };
            await WithUnitOfWorkAsync(() => {
                var result = _retroResultAppService.ExportRetroResult(input,getRetroId);
                Assert.NotNull(result);
                result.Result.FileName.ShouldBe("Template");
                result.Result.FileType.ShouldBe(MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
                result.Result.Base64.ShouldNotBeNull();
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }*/
        //TODO: fail due to wrong path for template

        [Fact]
        //Import Retro User From File (list already has retro which increase list warning)
        public async void ImportRetroUserFromFileTest1()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-1.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 1
            };


            await WithUnitOfWorkAsync(async() => {
                var result = await _retroResultAppService.ImportRetroUserFromFile(input);
                Assert.NotNull(result);
                var resultProperties = result.GetType().GetProperties();
                var warnlist = resultProperties.First(o => o.Name == "listWarning").GetValue(result, null) as List<string>;
                warnlist.Count.ShouldBe(4);
                warnlist.First().ShouldBe("thanh.trantien@ncc.asia [Project Training]");

            });
        }

        [Fact]
        //Import Retro User From File (input = null)
        public async void ImportRetroUserFromFileTest2()
        {
            ImportFileDto input = null;


            await WithUnitOfWorkAsync(async () => {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroResultAppService.ImportRetroUserFromFile(input);
                });
                Assert.Equal("No file upload!", exception.Message);

            });
        }

        [Fact]
        //Import Retro User From File (show errors on messageError)
        public async void ImportRetroUserFromFileTest4()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-3.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 1
            };


            await WithUnitOfWorkAsync(async () => {
                var result = await _retroResultAppService.ImportRetroUserFromFile(input);
                Assert.NotNull(result);
                var resultProperties = result.GetType().GetProperties();
                var errorlist = resultProperties.First(o => o.Name == "errorList").GetValue(result, null) as List<string>;
                errorlist.Count.ShouldBe(1);
                errorlist.First().ShouldBe("Row 2: Position, Note null  ");
            });
        }

        [Fact]
        //Import Retro User From File (invalid file upload)
        public async void ImportRetroUserFromFileTest3()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-2.xlsxs";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 1
            };


            await WithUnitOfWorkAsync(async () => {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroResultAppService.ImportRetroUserFromFile(input);
                });
                Assert.Equal("Invalid file upload.", exception.Message);

            });
        }

        [Fact]
        //Import Retro User From File (data in success list)
        public async void ImportRetroUserFromFileTest5()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-4.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 1
            };


            await WithUnitOfWorkAsync(async () => {
                var result = await _retroResultAppService.ImportRetroUserFromFile(input);
                Assert.NotNull(result);
                var resultProperties = result.GetType().GetProperties();
                var successlist = resultProperties.First(o => o.Name == "listEmailSuccess").GetValue(result, null) as List<string>;
                successlist.Count.ShouldBe(2);
                successlist.First().ShouldBe("thu.caothidieu@ncc.asia");
            });
        }

        [Fact]
        //Import Retro User From File (data in failed list)
        public async void ImportRetroUserFromFileTest6()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-5.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 5
            };


            await WithUnitOfWorkAsync(async () => {
                var result = await _retroResultAppService.ImportRetroUserFromFile(input);
                Assert.NotNull(result);
                var resultProperties = result.GetType().GetProperties();
                var failedlist = resultProperties.First(o => o.Name == "failedList").GetValue(result, null) as List<string>;
                failedlist.Count.ShouldBe(1);
                failedlist.First().ShouldBe("trang.vuquynh@ncc.asia");
            });
        }

        [Fact]
        //Import Retro User From File (show messageErrorOther in eror list)
        public async void ImportRetroUserFromFileTest7()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-6.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 5
            };


            await WithUnitOfWorkAsync(async () => {
                var result = await _retroResultAppService.ImportRetroUserFromFile(input);
                Assert.NotNull(result);
                var resultProperties = result.GetType().GetProperties();
                var errorList = resultProperties.First(o => o.Name == "errorList").GetValue(result, null) as List<string>;
                errorList.Count.ShouldBe(1);
                errorList.First().ShouldBe("Row 2:  Can not find User by Email: trang.vuquynh4@ncc.asia, Point can not be less than 0 or greater than 5");
            });
        }

        [Fact]
        //Confirm Import Retro
        public async void ConfirmImportRetroTest1()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-4.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            var input = new ImportFileDto
            {
                File = formFile,
                retroId = 1,
                projectId = 1
            };


            await WithUnitOfWorkAsync(async () => {
                var result = await _retroResultAppService.ConfirmImportRetro(input);
                Assert.NotNull(result);
                result.Count.ShouldBe(2);
                result.First().ShouldBe("thu.caothidieu@ncc.asia");
            });
        }

        [Fact]
        //Confirm Import Retro Null input
        public async void ConfirmImportRetroTest2()
        {
            var file = "../../../API/RetroResultAppServiceTest/Files/import-retrouser-test-4.xlsx";
            var stream = new MemoryStream(File.ReadAllBytes(file).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", file.Split(@"\").Last());

            ImportFileDto input = null;


            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<NullReferenceException>(async () =>
                {
                    await _retroResultAppService.ConfirmImportRetro(input);
                });
            });
        }

        [Fact]
        //Get Retro User Info
        public async void GetRetroUserInfoTest1()
        {

            await WithUnitOfWorkAsync(async () => {
                Assert.Equal(1, 1);//fail
            });
        }
        //TODO: unable to test due to null data from _project api

        [Fact]
        //get all paging no skip, no take
        //if no MaxResultCount then take = 10
        public async void GetAllPagingTest1()
        {
            var expectTotalCount = 5;
            var expectItemCount = 5;
            long getRetroId = 1;

            var input = new InputMultiFilterRetroResultPagingDto {
                GridParam = new GridParam { },
                ProjecIds = new List<long> { 1,4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1,3,7,8,6 },
                PositionIds = new List<long> { 4,1 },
                LeftPoint = 5,
                RightPoint = 4

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.GetAllPagging(input, getRetroId);
                result.Items.First().ShouldNotBeNull();
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                result.Items.First().BranchColor.ShouldBe("#f44336\t");
                result.Items.First().BranchId.ShouldBe(1);
                result.Items.First().BranchName.ShouldBe("HN1");
                result.Items.First().CreatedUserName.ShouldBe("admin admin");
                result.Items.First().CreationTime.AddTicks(-2119).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 34, 999));
                result.Items.First().EmailAddress.ShouldBe("tien.nguyenhuu@ncc.asia");
                result.Items.First().FullName.ShouldBe("Tiến Nguyễn Hữu");
                result.Items.First().Id.ShouldBe(1);
                result.Items.First().LastModifierTime.Value.AddTicks(-2714).ShouldBe(new DateTime(2022, 12, 27, 23, 20, 35, 479));
                result.Items.First().LastModifierUserName.ShouldBe("admin admin");
                result.Items.First().Level.ShouldBe(UserLevel.SeniorPlus);
                result.Items.First().Note.ShouldBe("Ok");
                result.Items.First().Point.ShouldBe(5);
                result.Items.First().PositionId.ShouldBe(4);
                result.Items.First().PositionName.ShouldBe("PM");
                result.Items.First().ProjectId.ShouldBe(1);
                result.Items.First().ProjectName.ShouldBe("Project UCG");
                result.Items.First().RetroName.ShouldBe("Retro tháng 09 - 2022");
                result.Items.First().Type.ShouldBe(Usertype.Staff);
                result.Items.First().UpdatedAt.AddTicks(-2714).ShouldBe(new DateTime(2022, 12, 27, 23, 20, 35,479));
                result.Items.First().UserBranchColor.ShouldBe("#f44336\t");
                result.Items.First().UserBranchId.ShouldBe(1);
                result.Items.First().UserBranchName.ShouldBe("HN1");
                result.Items.First().UserId.ShouldBe(3);
                result.Items.First().UserLevel.ShouldBe(UserLevel.SeniorPlus);
                result.Items.First().UserType.ShouldBe(Usertype.Staff);
            });
        }
        
        [Fact]
        //get all paging skip = 1, no take
        public async void GetAllPagingTest2()
        {
            var expectTotalCount = 5;
            var expectItemCount = 4;
            var skipCount = 1;
            var getRetroId = 1;

            var input = new InputMultiFilterRetroResultPagingDto
            {
                GridParam = new GridParam {
                    SkipCount = skipCount,
                },
                ProjecIds = new List<long> { 1, 4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1, 3, 7, 8, 6 },
                PositionIds = new List<long> { 4, 1 },
                LeftPoint = 5,
                RightPoint = 4

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.GetAllPagging(input, getRetroId);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                Assert.NotEqual(expectTotalCount, result.Items.Count);
                Assert.Equal(expectItemCount, result.TotalCount - skipCount);

                result.Items.First().BranchColor.ShouldBe("blue");
                result.Items.First().BranchId.ShouldBe(3);
                result.Items.First().BranchName.ShouldBe("HN3");
                result.Items.First().CreatedUserName.ShouldBe("admin admin");
                result.Items.First().CreationTime.AddTicks(-9404).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 35, 007));
                result.Items.First().EmailAddress.ShouldBe("hieu.trantrung@ncc.asia");
                result.Items.First().FullName.ShouldBe("Hiếu Trần Trung");
                result.Items.First().Id.ShouldBe(2);
                result.Items.First().LastModifierTime.ShouldBe(null);
                result.Items.First().LastModifierUserName.ShouldBe("");
                result.Items.First().Level.ShouldBe(UserLevel.FresherMinus);
                result.Items.First().Note.ShouldBe("Ok");
                result.Items.First().Point.ShouldBe(4.5);
                result.Items.First().PositionId.ShouldBe(1);
                result.Items.First().PositionName.ShouldBe("Dev");
                result.Items.First().ProjectId.ShouldBe(1);
                result.Items.First().ProjectName.ShouldBe("Project UCG");
                result.Items.First().RetroName.ShouldBe("Retro tháng 09 - 2022");
                result.Items.First().Type.ShouldBe(Usertype.Collaborators);
                result.Items.First().UpdatedAt.AddTicks(-9404).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 35, 007));
                result.Items.First().UserBranchColor.ShouldBe("blue");
                result.Items.First().UserBranchId.ShouldBe(3);
                result.Items.First().UserBranchName.ShouldBe("HN3");
                result.Items.First().UserId.ShouldBe(6);
                result.Items.First().UserLevel.ShouldBe(UserLevel.FresherMinus);
                result.Items.First().UserType.ShouldBe(Usertype.Collaborators);
                result.Items.First().UserType.ShouldBe(Usertype.Collaborators);
                result.Items.First().BranchId.ShouldNotBe(1);
                result.Items.First().BranchName.ShouldNotBe("HN1");
            });
        }
        
        [Fact]
        //get all paging with skip > 5 (max = 5 records) no take
        public async void GetAllPagingTest3()
        {
            var expectItemCount = 0;
            var skipCount = 6;
            var getRetroId = 1;

            var input = new InputMultiFilterRetroResultPagingDto
            {
                GridParam = new GridParam
                {
                    SkipCount = skipCount,
                },
                ProjecIds = new List<long> { 1, 4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1, 3, 7, 8, 6 },
                PositionIds = new List<long> { 4, 1 },
                LeftPoint = 5,
                RightPoint = 4

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.GetAllPagging(input,getRetroId);
                Assert.Equal(expectItemCount, result.Items.Count);
                result.Items.ShouldNotContain(RetroResult => RetroResult.Id == 1);
            });
        }

        [Fact]
        //get all paging with take = 1  
        public async void GetAllPagingTest4()
        {
            var takeCount = 1;
            var getRetroId = 1;

            var input = new InputMultiFilterRetroResultPagingDto
            {
                GridParam = new GridParam
                {
                    MaxResultCount = takeCount,
                },
                ProjecIds = new List<long> { 1, 4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1, 3, 7, 8, 6 },
                PositionIds = new List<long> { 4, 1 },
                LeftPoint = 5,
                RightPoint = 4

            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.GetAllPagging(input, getRetroId);
                result.Items.First().ShouldNotBeNull();
                Assert.Equal(5, result.TotalCount);
                Assert.Equal(1, result.Items.Count);
                result.Items.First().BranchColor.ShouldBe("#f44336\t");
                result.Items.First().BranchId.ShouldBe(1);
                result.Items.First().BranchName.ShouldBe("HN1");
                result.Items.First().CreatedUserName.ShouldBe("admin admin");
                result.Items.First().CreationTime.AddTicks(-2119).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 34, 999));
                result.Items.First().EmailAddress.ShouldBe("tien.nguyenhuu@ncc.asia");
                result.Items.First().FullName.ShouldBe("Tiến Nguyễn Hữu");
                result.Items.First().Id.ShouldBe(1);
                result.Items.First().LastModifierTime.Value.AddTicks(-2714).ShouldBe(new DateTime(2022, 12, 27, 23, 20, 35, 479));
                result.Items.First().LastModifierUserName.ShouldBe("admin admin");
                result.Items.First().Level.ShouldBe(UserLevel.SeniorPlus);
                result.Items.First().Note.ShouldBe("Ok");
                result.Items.First().Point.ShouldBe(5);
                result.Items.First().PositionId.ShouldBe(4);
                result.Items.First().PositionName.ShouldBe("PM");
                result.Items.First().ProjectId.ShouldBe(1);
                result.Items.First().ProjectName.ShouldBe("Project UCG");
                result.Items.First().RetroName.ShouldBe("Retro tháng 09 - 2022");
                result.Items.First().Type.ShouldBe(Usertype.Staff);
                result.Items.First().UpdatedAt.AddTicks(-2714).ShouldBe(new DateTime(2022, 12, 27, 23, 20, 35, 479));
                result.Items.First().UserBranchColor.ShouldBe("#f44336\t");
                result.Items.First().UserBranchId.ShouldBe(1);
                result.Items.First().UserBranchName.ShouldBe("HN1");
                result.Items.First().UserId.ShouldBe(3);
                result.Items.First().UserLevel.ShouldBe(UserLevel.SeniorPlus);
                result.Items.First().UserType.ShouldBe(Usertype.Staff);
            });
        }
        
        [Fact]
        //get all paging with skip = 1, take = 1  
        public async void GetAllPagingTest5()
        {
            var expectTotalCount = 5;
            var expectItemCount = 1;
            var skipCount = 1;
            var takeCount = 1;
            var getRetroId = 1;

            var input = new InputMultiFilterRetroResultPagingDto
            {
                GridParam = new GridParam
                {
                    MaxResultCount = takeCount,
                    SkipCount = skipCount,
                },
                ProjecIds = new List<long> { 1, 4 },
                Userlevels = new List<UserLevel> { UserLevel.SeniorPlus, UserLevel.MiddlePlus, UserLevel.FresherMinus },
                Usertypes = new List<Usertype> { Usertype.Collaborators, Usertype.Staff },
                BranchIds = new List<long> { 1, 3, 7, 8, 6 },
                PositionIds = new List<long> { 4, 1 },
                LeftPoint = 5,
                RightPoint = 4

            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroResultAppService.GetAllPagging(input, getRetroId);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.First().BranchColor.ShouldBe("blue");
                result.Items.First().BranchId.ShouldBe(3);
                result.Items.First().BranchName.ShouldBe("HN3");
                result.Items.First().CreatedUserName.ShouldBe("admin admin");
                result.Items.First().CreationTime.AddTicks(-9404).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 35, 007));
                result.Items.First().EmailAddress.ShouldBe("hieu.trantrung@ncc.asia");
                result.Items.First().FullName.ShouldBe("Hiếu Trần Trung");
                result.Items.First().Id.ShouldBe(2);
                result.Items.First().LastModifierTime.ShouldBe(null);
                result.Items.First().LastModifierUserName.ShouldBe("");
                result.Items.First().Level.ShouldBe(UserLevel.FresherMinus);
                result.Items.First().Note.ShouldBe("Ok");
                result.Items.First().Point.ShouldBe(4.5);
                result.Items.First().PositionId.ShouldBe(1);
                result.Items.First().PositionName.ShouldBe("Dev");
                result.Items.First().ProjectId.ShouldBe(1);
                result.Items.First().ProjectName.ShouldBe("Project UCG");
                result.Items.First().RetroName.ShouldBe("Retro tháng 09 - 2022");
                result.Items.First().Type.ShouldBe(Usertype.Collaborators);
                result.Items.First().UpdatedAt.AddTicks(-9404).ShouldBe(new DateTime(2022, 12, 27, 23, 19, 35, 007));
                result.Items.First().UserBranchColor.ShouldBe("blue");
                result.Items.First().UserBranchId.ShouldBe(3);
                result.Items.First().UserBranchName.ShouldBe("HN3");
                result.Items.First().UserId.ShouldBe(6);
                result.Items.First().UserLevel.ShouldBe(UserLevel.FresherMinus);
                result.Items.First().UserType.ShouldBe(Usertype.Collaborators);
                result.Items.First().UserType.ShouldBe(Usertype.Collaborators);
                result.Items.First().BranchId.ShouldNotBe(1);
                result.Items.First().BranchName.ShouldNotBe("HN1");
            });
        }
        /*
        [Fact]
        //Create Retro
        public async void CreateTest1()
        {
            var expectId = 5;
            var input = new RetroCreateDto
            {
                Id = expectId,
                Name = "retro5",
                StartDate = new DateTime(2022, 11, 01),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.Create(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allretros = _workScope.GetAll<Retro>();
                var createdretro = await _workScope.GetAsync<Retro>(expectId);

                allretros.Count().ShouldBe(5);
                createdretro.ShouldNotBeNull();
                createdretro.Name.ShouldBe(input.Name);
                createdretro.StartDate.ShouldBe(input.StartDate);
                createdretro.EndDate.ShouldBe(input.EndDate);
                createdretro.Deadline.ShouldBe(input.Deadline);
                createdretro.Status.ShouldBe(input.Status);
            });
        }

        [Fact]
        //Create Retro start date > end date
        public async void CreateTest2()
        {
            var expectId = 3;
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "retro1",
                StartDate = new DateTime(2022, 11, 28),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal("Start date > end date", exception.Message);
            });
        }

        [Fact]
        //Create Retro start date > dead line
        public async void CreateTest3()
        {
            var expectId = 3;
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "retro1",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 01),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal("Start date > deadline", exception.Message);
            });
        }

        [Fact]
        //Create Retro name already existed
        public async void CreateTest4()
        {
            var expectId = 3;
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "Retro tháng 09 - 2022",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal($"{input.Name} ({input.StartDate:MM/yyyy}) already existed", exception.Message);
            });
        }

        [Fact]
        public async void DeleteTest1()
        {
            var expectTotalCount = 3;
            var retroToDelete = new EntityDto<long>
            {
                Id = 4,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.Delete(retroToDelete);
            });

            await WithUnitOfWorkAsync(() => {
                var allretros = _workScope.GetAll<Retro>();
                var retro = _workScope.GetAll<Retro>()
                 .Where(s => s.Id == retroToDelete.Id);

                allretros.Count().ShouldBe(expectTotalCount);
                retro.ShouldBeEmpty();
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        // Delete Retro with UserFriendlyException has retro detail
        public async void DeleteTest2()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
                Assert.Equal($"Retro Id {retroToDelete.Id} has retro detail", exception.Message);
            });
        }

        [Fact]
        // Delete Retro with UserFriendlyException retro status close
        public async void DeleteTest3()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 3,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
                Assert.Equal("Cannot be deleted because the retro status close", exception.Message);
            });
        }

        [Fact]
        // Delete Retro with UserFriendlyException entity not found
        public async void DeleteTest4()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 9999,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
            });
        }

        [Fact]
        //Change Status Retro
        public async void ChangeStatusTest1()
        {
            var expectId = 1;

            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.ChangeStatus(retroToChangeStatus);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(retroToChangeStatus.Id);
                changedStatusRetro.Status.ShouldBe(StatusEnum.RetroStatus.Close);
            });
        }

        [Fact]
        //Change Status Retro status = close
        public async void ChangeStatusTest2()
        {
            var expectId = 3;

            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 3,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.ChangeStatus(retroToChangeStatus);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(retroToChangeStatus.Id);
                changedStatusRetro.Status.ShouldBe(StatusEnum.RetroStatus.Public);
            });
        }

        [Fact]
        //Change Status Retro id not found
        public async void ChangeStatusTest3()
        {
            var expectId = 1;

            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 1000,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _retroAppService.ChangeStatus(retroToChangeStatus);
                });
            });
        }

        [Fact]
        //Update Retro
        public async void UpdateTest1()
        {
            var expectId = 1;

            var input = new RetroEditDto
            {
                Id = expectId,
                Name = "retro2",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 30),
                Deadline = new DateTime(2022, 11, 30),
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.Update(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(input.Id);
                changedStatusRetro.StartDate.ShouldBe(input.StartDate);
                changedStatusRetro.EndDate.ShouldBe(input.EndDate);
                changedStatusRetro.Deadline.ShouldBe(input.Deadline);
            });
        }

        [Fact]
        //Update Retro  retro existed
        public async void UpdateTest2()
        {
            var expectId = 2;

            var input = new RetroEditDto
            {
                Id = expectId,
                Name = "Retro tháng 09 - 2022",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 30),
                Deadline = new DateTime(2022, 11, 30),
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    var result = await _retroAppService.Update(input);
                });
                Assert.Equal($"Retro {input.Name} already existed", exception.Message);
            });

        }*/
    }
}
