using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Ncc.Entities.Enum
{
    public class StatusEnum
    {
        public enum ProjectStatus
        {
            Active = 0,
            Deactive = 1,
        }

        public enum ProjectType
        {
            TimeAndMaterials = 0,
            FixedFee = 1,
            NoneBillable = 2,
            ODC = 3,
            Product = 4,
            Training = 5,
            NoSalary = 6
        }

        public enum TaskType
        {
            CommonTask = 0,
            OrtherTask = 1
        }

        public enum TypeOfWork
        {
            NormalWorkingHours = 0,
            OverTime = 1
        }

        public enum TimesheetStatus
        {
            All = -1,
            None = 0,
            Pending = 1,
            Approve = 2,
            Reject = 3
        }

        public enum MyTimesheetFilterStatus
        {
            All = -1,
            New = 0,
            PendingApproved = 1,
            Pending = 2,
            Approved = 3,
            Rejected = 4
        }

        public enum TimesheetSort
        {
            Project = 0,
            People = 1,
            Week = 2
        }

        public enum Usertype : byte
        {
            Staff = 0,
            Internship = 1,
            Collaborators = 2,
            ProbationaryStaff = 3,
            Vendor = 5

            //Internship = 0,
            //Collaborators = 1,
            //Staff = 2,
            //ProbationaryStaff = 3
        }

        public enum UserLevel : byte
        {
            Intern_0 = 0,
            Intern_1 = 1,
            Intern_2 = 2,
            Intern_3 = 3,
            FresherMinus = 4,
            Fresher = 5,
            FresherPlus = 6,
            JuniorMinus = 7,
            Junior = 8,
            JuniorPlus = 9,
            MiddleMinus = 10,
            Middle = 11,
            MiddlePlus = 12,
            SeniorMinus = 13,
            Senior = 14,
            SeniorPlus = 15,
        }

        public enum RequestStatus
        {
            All = 0,
            Pending = 1,
            Approved = 2,
            Rejected = 3,
        }

        public enum RequestType
        {
            Off = 0,
            Onsite = 1,
            Remote = 2,
        }

        public enum Branch : byte
        {
            HaNoi = 0,
            DaNang = 1,
            HoChiMinh = 2,
            Vinh = 3
        }

        public enum Sex : byte
        {
            Male = 0,
            Female = 1
        }

        public enum DayType : byte
        {
            Fullday = 1,
            Morning = 2,
            Afternoon = 3,
            Custom = 4,
        }

        public enum OnDayType : byte
        {
            DiMuon = 1,
            MiddleOfDay = 2,
            VeSom = 3,
        }

        public enum OffTypeStatus : byte
        {
            CoPhep = 1,
            KhongPhep = 2
        }

        public enum SalaryMonthStatus : byte
        {
            None = 0,
            SendEmail = 1,
            Confirmed = 2
        }

        public enum SalaryStatus : byte
        {
            Active = 0,
            Deactive = 1,
        }

        public enum SalaryType : byte
        {
            BasicSalary = 0,
            Allowances = 1,
            Normal = 2
        }

        public enum LockUnlockTimesheetType : byte
        {
            MyTimesheet = 0,
            ApproveRejectTimesheet = 1
        }

        public enum PunishmentStatus : byte
        {
            Punish = 1,
            Normal = 0
        }

        public enum FundStatus : byte
        {
            Proceeds = 0,
            Payment = 1
        }

        public enum ReviewInternStatus
        {
            Draft = 0,
            Reviewed = 1,
            Approved = 2,
            SentEmail = 3,
            Rejected = -1
        }

        public enum EnumRequest
        {
            OffFull = 1,
            OffMorning = 2,
            OffAfternoon = 3,
            RemoteFull = 4,
            RemoteMorning = 5,
            RemoteAfternoon = 6,
            Tardiness = 7,
            EarlyLeave = 8
        }

        public enum TsStatusFilter
        {
            Approved = 1,
            PendingAndApproved = 2
        }

        public enum CheckInFilter
        {
            NoCheckIn = 1,
            NoCheckOut = 2,
            NoCheckInAndNoCheckOut = 3,
            NoCheckInAndNoCheckOutButHaveTs = 4,
        }

        public enum HaveCheckInFilter
        {
            HaveCheckIn = 1,
            HaveCheckOut = 2,
            HaveCheckInAndHaveCheckOut = 3,
            HaveCheckInOrHaveCheckOut = 4,
            NoCheckInAndNoCheckOut = 5,
        }
        public enum RetroStatus : byte
        {
            Public = 0,
            Close = 1,
        }
        public enum InsuranceStatus
        {
            BHXH = 1,
            PVI = 2,
            NONE = 3
        }

        public enum CapabilityType
        {
            Point = 0,
            Note = 1,
        }
        public enum CheckInCheckOutPunishmentType
        {
            NoPunish = 0,
            Late = 1,
            NoCheckIn = 2,
            NoCheckOut = 3,
            LateAndNoCheckOut = 4,
            NoCheckInAndNoCheckOut = 5
        }
        public enum TeamBuildingStatus
        {
            Open = 0,
            Requested = 1,
            Done = 2
        }

        public enum TeamBuildingRequestStatus : byte
        {
            Pending = 0,
            Done = 1,
            Rejected = 2,
            Cancelled = 3,
        }

        public enum RemainingMoneyStatus : byte
        {
            Remaining = 0,
            Done = 1
        }
    }
}