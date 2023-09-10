using Abp.UI;
using Ncc.Authorization.Users;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.APIs.RequestDays;
using Timesheet.Entities;
using Timesheet.Uitls;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.RequestDays
{
    /*<summary>
    14/15 functions
    24 passed test case
    Updated date: 13/01/2023
    </summary>*/

    public class RequestDayAppServiceTest : RequestDayAppServiceTestBase
    {
        private readonly RequestDayAppService _requestDayAppService;

        public RequestDayAppServiceTest() 
        {
            _requestDayAppService = InstanceRequestDayAppService();
        }

        [Fact]
        public async void CanNotGetAllRequestWithoutPMRole()
        {
            var inputRequestDto = InputRequestDto();
            var listProjectIds = new List<long> { 2, 3, 4};
            inputRequestDto.projectIds = listProjectIds;
            var expectedMessage = "You aren't the PM of the selected project";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.GetAllRequest(inputRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void GetAllRequest()
        {
            var inputRequestDto = InputRequestDto();
            var expectedCount = 7;
            var expectedBranchName = "HN1";
            var expectedUserId = 1;
            var expectedDayoffName = "Nghỉ thông thường\t";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.GetAllRequest(inputRequestDto);

                Assert.NotNull(result);
                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(request => request.AbsenceTime == null);
                result.ShouldContain(request => request.BranchDisplayName == expectedBranchName);
                result.ShouldContain(request => request.DateType == DayType.Afternoon);
                result.ShouldContain(request => request.UserId == expectedUserId);
                result.ShouldContain(request => request.Status == RequestStatus.Approved);
                result.ShouldContain(request => request.DayOffName == expectedDayoffName);
            });
        }

        [Fact]
        public async void InternCanNotSendRemoteRequest()
        {

            var myRequestDto = MyRequestDto();
            myRequestDto.Type = RequestType.Remote;
            var expectedMessage = "Intern is not allow to work REMOTE at this time";

            WithUnitOfWork(() =>
            {
                var user = workScope.Get<User>(1);
                user.Type = Usertype.Internship;

                workScope.Update<User>(user);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                   await _requestDayAppService.SubmitToPending(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotSendDuplicateRequest()
        {
            
            var myRequestDto = MyRequestDto();
            var absenceDayRequest = AbsenceDayRequest();
            var absenceDayDetail = AbsenceDayDetail();
            absenceDayDetail.DateAt = new DateTime(2022, 1, 1);
            var expectedMessage = "Some Date in the request already exist in DB. Refresh to load new data.";

            WithUnitOfWork(() =>
            {
                workScope.InsertAsync(absenceDayRequest);
                workScope.InsertAsync(absenceDayDetail);
            });

            await WithUnitOfWorkAsync(async() =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async ()  =>
                {
                    await _requestDayAppService.SubmitToPending(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotRequestInWeekendOrHoliday()
        {
            
            var myRequestDto = MyRequestDto();
            var dayOffSetting = DayOffSetting();
            var absenceDayDetailDto = AbsenceDayDetailDto();
            var expectedMessage = string.Format("{0} is not a working day.", DateTimeUtils.ToString(absenceDayDetailDto.DateAt));

            WithUnitOfWork(() =>
            {
                workScope.Insert(dayOffSetting);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.SubmitToPending(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotRequestWithInvalidHour()
        {
            
            var myRequestDto = MyRequestDto();
            var dayOffSetting = DayOffSetting();
            var absenceDayDetailDto = AbsenceDayDetailDto();
            absenceDayDetailDto.DateType = DayType.Custom;
            absenceDayDetailDto.Hour = 10;
            myRequestDto.Absences = new List<AbsenceDayDetailDto> { absenceDayDetailDto };

            var expectedMessage = string.Format("You can't submit absence hour > 7.5h  or absence hour = 0h ");

            WithUnitOfWork(() =>
            {
                //workScope.Insert(dayOffSetting);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.SubmitToPending(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void SubmitToPending()
        {
            var listRequestBeforeAction = new List<AbsenceDayRequest>();
            var listRequestDetailBeforeAction = new List<AbsenceDayDetail>();
            
            var myRequestDto = MyRequestDto();

            await WithUnitOfWorkAsync(async () =>
            {
                listRequestBeforeAction = workScope.GetAll<AbsenceDayRequest>().ToList();
                listRequestDetailBeforeAction = workScope.GetAll<AbsenceDayDetail>().ToList();

                var result = await _requestDayAppService.SubmitToPending(myRequestDto);
            });

            WithUnitOfWork(() =>
            {
                var listRequestAfterAction = workScope.GetAll<AbsenceDayRequest>().ToList();
                var listRequestDeatilAfterAction = workScope.GetAll<AbsenceDayRequest>().ToList();

                listRequestAfterAction.Count.ShouldBeGreaterThan(listRequestBeforeAction.Count);
                listRequestDeatilAfterAction.Count.ShouldBeGreaterThan(listRequestDetailBeforeAction.Count);
            });
        }

        [Fact]
        public async void InternCanNotSendNewRemoteRequest()
        {
            
            var myRequestDto = MyRequestDto();
            myRequestDto.Type = RequestType.Remote;
            var expectedMessage = "Intern is not allow to work REMOTE at this time";

            WithUnitOfWork(() =>
            {
                var user = workScope.Get<User>(1);
                user.Type = Usertype.Internship;

                workScope.Update<User>(user);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.SubmitToPendingNew(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotSendRequestWithoutReason()
        {
            
            var myRequestDto = MyRequestDto();
            myRequestDto.Reason = "";
            var expectedMessage = "Reason is require!";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.SubmitToPendingNew(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }



        [Fact]
        public async void CanNotNEwRequestInWeekendOrHoliday()
        {
            
            var myRequestDto = MyRequestDto();
            var dayOffSetting = DayOffSetting();
            var absenceDayDetailDto = AbsenceDayDetailDto();
            var expectedMessage = string.Format("{0} is not a working day.", DateTimeUtils.ToString(absenceDayDetailDto.DateAt));

            WithUnitOfWork(() =>
            {
                workScope.Insert(dayOffSetting);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.SubmitToPendingNew(myRequestDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void SubmitToPendingNew()
        {
            var listRequestBeforeAction = new List<AbsenceDayRequest>();
            var listRequestDetailBeforeAction = new List<AbsenceDayDetail>();
            
            var myRequestDto = MyRequestDto();

            await WithUnitOfWorkAsync(async () =>
            {
                listRequestBeforeAction = workScope.GetAll<AbsenceDayRequest>().ToList();
                listRequestDetailBeforeAction = workScope.GetAll<AbsenceDayDetail>().ToList();

                var result = await _requestDayAppService.SubmitToPendingNew(myRequestDto);
            });

            WithUnitOfWork(() =>
            {
                var listRequestAfterAction = workScope.GetAll<AbsenceDayRequest>().ToList();
                var listRequestDeatilAfterAction = workScope.GetAll<AbsenceDayRequest>().ToList();

                listRequestAfterAction.Count.ShouldBeGreaterThan(listRequestBeforeAction.Count);
                listRequestDeatilAfterAction.Count.ShouldBeGreaterThan(listRequestDetailBeforeAction.Count);
            });
        }

        [Fact]
        public async void getReceiverSendRequestList()
        {
            var requestId = 1;
            var expectedCount = 3;


            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.getReceiverSendRequestList(requestId);

                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(projectPM => projectPM.ProjectId == 1);
            });
        }

        [Fact]
        public async void getReceiverApproveRejectList()
        {
            var requestId = 1;
            var expectedCount = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.getReceiverApproveRejectList(requestId);

                Assert.NotNull(result);
                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(projectPM => projectPM.ProjectId == 1);
            });
        }

        [Fact]
        public async void GetAllMyRequest()
        {
            
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2023, 1, 1);
            var requestType = RequestType.Off;
            var dayType = DayType.Fullday;
            var expectedCount = 1;
            var expectedDayOffName = "Nghỉ thông thường\t";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.GetAllMyRequest(startDate, endDate, requestType, dayType);

                Assert.Equal(expectedCount, result.Count);
                result.All(request => request.UserId == AbpSession.UserId.Value);
                result.ShouldContain(request => request.DayOffName == expectedDayOffName);
                result.ShouldContain(request => request.Type == requestType);
            });
        }

        [Fact]
        public void GetAllMyRequestNew()
        {
            
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2023, 1, 1);
            var requestType = RequestType.Off;
            var expectedCount = 2;

            WithUnitOfWork(() =>
            {
                var result =  _requestDayAppService.GetAllMyRequestNew(startDate, endDate, requestType);

                Assert.Equal(expectedCount, result.Count);
                result.All(listRequest => startDate <= listRequest.DateAt && listRequest.DateAt <= endDate);
                result.All(listRequest => listRequest.Requests.All(request => request.Type == requestType));
            });
        }

        [Fact]
        public async void CanNotCancelMyRequestInThePast() 
        {
            var requestId = 1;
            var expectedMessage = "You can't cancel request in the PAST! Contact your PM to reject it";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.CancelMyRequest(requestId);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotCanceRequestThatIsNotYour() 
        {
            
            var requestId = 101;
            var requestDay = AbsenceDayRequest();
            var requestDayDetail = AbsenceDayDetail();
            requestDayDetail.DateAt = DateTimeUtils.GetNow().AddDays(1);
            requestDay.UserId = 3;
            var expectedMessage = "The request is not your";

            WithUnitOfWork(() =>
            {
                workScope.InsertAsync(requestDay);
                workScope.InsertAsync(requestDayDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _requestDayAppService.CancelMyRequest(requestId);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CancelMyRequest()
        {
            
            var requestId = 101;
            var requestDay = AbsenceDayRequest();
            var requestDayDetail = AbsenceDayDetail();
            requestDayDetail.DateAt = DateTimeUtils.GetNow().AddDays(1);
            requestDay.Status = RequestStatus.Pending;

            WithUnitOfWork(() =>
            {
                workScope.InsertAsync(requestDay);
                workScope.InsertAsync(requestDayDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                // action
                await _requestDayAppService.CancelMyRequest(requestId);

                // Get request that has been just updated
                var requestDetail = workScope.Get<AbsenceDayDetail>(requestDayDetail.Id);

                // Verify successfully approve request
                Assert.Equal(RequestStatus.Rejected, requestDetail.Request.Status);
            });
        }

        [Fact]
        public async void ApproveRequest() 
        {
            
            var requestId = 101;
            var requestDay = AbsenceDayRequest();
            var requestDayDetail = AbsenceDayDetail();
            requestDayDetail.DateAt = DateTimeUtils.GetNow().AddDays(1);
            requestDay.Status = RequestStatus.Pending;

            WithUnitOfWork(() =>
            {
                workScope.InsertAsync(requestDay);
                workScope.InsertAsync(requestDayDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                // action
                await _requestDayAppService.ApproveRequest(requestId);

                // Get request that has been just updated
                var requestDetail = workScope.Get<AbsenceDayDetail>(requestDayDetail.Id);

                // Verify successfully approve request
                Assert.Equal(RequestStatus.Approved, requestDetail.Request.Status);
            });
        }

        [Fact]
        public async void RejectRequest()
        {
            
            var requestId = 101;
            var requestDay = AbsenceDayRequest();
            var requestDayDetail = AbsenceDayDetail();
            requestDayDetail.DateAt = DateTimeUtils.GetNow().AddDays(1);
            requestDay.Status = RequestStatus.Pending;

            WithUnitOfWork(() =>
            {
                workScope.InsertAsync(requestDay);
                workScope.InsertAsync(requestDayDetail);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                // action
                await _requestDayAppService.RejectRequest(requestId);

                // Get request that has been just updated
                var requestDetail = workScope.Get<AbsenceDayDetail>(requestDayDetail.Id);

                // Verify successfully approve request
                Assert.Equal(RequestStatus.Rejected, requestDetail.Request.Status);
            });
        }

        [Fact]
        public async void GetAllRequestByUserIdForTeamMember()
        {
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2023, 1, 1);
            var userId = 17;
            var expectedCount = 3;
            var expectedDayOffName = "Nghỉ cưới bản thân (3 ngày phép)\t";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.GetAllRequestByUserIdForTeamMember(startDate, endDate, userId);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(request => request.UserId == userId));
                result.ShouldContain(request => request.DayOffName == expectedDayOffName);
            });
        }

        [Fact]
        public async void GetAllRequestByUserId()
        {
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2023, 1, 1);
            var userId = 17;
            var expectedCount = 3;
            var expectedDayOffName = "Nghỉ cưới bản thân (3 ngày phép)\t";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.GetAllRequestByUserId(startDate, endDate, userId);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(request => request.UserId == userId));
                result.ShouldContain(request => request.DayOffName == expectedDayOffName);
            });
        }

        [Fact]
        public void GetUserRequestByDate()
        { 
            var dateAt = new DateTime(2022, 12, 5);
            var userId = 6;
            var expectedCount = 1;
            var expectedDayOffName = "Nghỉ thông thường\t";
            var expectedFullName = "Hiếu Trần Trung";
            var expectedBranch = "HN3";
            var expectedType = Usertype.Collaborators;
            var expectedStatus = RequestStatus.Approved;

            WithUnitOfWork(() =>
            {
                var result = _requestDayAppService.GetUserRequestByDate(dateAt, userId);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(request => request.UserId == userId));
                Assert.True(result.All(request => request.DateAt == dateAt));
                result.ShouldContain(request => request.FullName == expectedFullName);
                result.ShouldContain(request => request.DayOffName == expectedDayOffName);
                result.ShouldContain(request => request.Branch == expectedBranch);
                result.ShouldContain(request => request.Type == expectedType);
                result.ShouldContain(request => request.Status == expectedStatus);
            });
        }

        [Fact]
        public async void GetAllRequestOfUserByDate()
        { 
            
            var dateAt = new DateTime(2022, 12, 28);
            var expectedCount = 1;
            var expectedStatus = RequestStatus.Pending;
            var expectedDayOffName = "Nghỉ thông thường\t";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _requestDayAppService.GetAllRequestOfUserByDate(dateAt);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(request => request.DateAt == dateAt));
                Assert.True(result.All(request => request.UserId == AbpSession.UserId.Value));
                result.ShouldContain(request => request.Status == expectedStatus);
                result.ShouldContain(request => request.DayOffName == expectedDayOffName);
            });
        }
    }
}
