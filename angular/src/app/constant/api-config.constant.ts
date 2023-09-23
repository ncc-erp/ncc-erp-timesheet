import { APP_CONSTANT } from "./api.constants";

export const APP_CONFIG = {
    EnumProjectStatus: [
        {
            value: APP_CONSTANT.EnumProjectStatus.Active,
            name: 'Active'
        },
        {
            value: APP_CONSTANT.EnumProjectStatus.Deactive,
            name: 'Deactive'
        },
        {
            value: APP_CONSTANT.EnumProjectStatus.All,
            name: 'All'
        }
    ],
    EnumProjectType: {
        Timeandmaterials: 0,
        Fixedfee: 1,
        Nonbillable: 2
    },

    EnumUserType: [
        {
            value: APP_CONSTANT.EnumUserType.Member,
            name: 'Member'
        },
        {
            value: APP_CONSTANT.EnumUserType.PM,
            name: 'PM'
        },
        {
            value: APP_CONSTANT.EnumUserType.Shadow,
            name: 'Shadow'
        },
        {
            value: APP_CONSTANT.EnumUserType.DeActive,
            name: 'Deactive'
        },
    ],


    EnumTaskType: [
        {
            value: APP_CONSTANT.EnumTaskType.Commontask,
            name: 'Common Task'
        },
        {
            value: APP_CONSTANT.EnumTaskType.Orthertask,
            name: 'Other Task'
        }
    ],
    EnumTypeOfWork: [
        {
            value: APP_CONSTANT.EnumTypeOfWork.Normalworkinghours,
            name: 'Normal working hours'
        },
        {
            value: APP_CONSTANT.EnumTypeOfWork.Overtime,
            name: 'Overtime'
        }
    ],
    TimesheetStatus: [
        {
            value: APP_CONSTANT.TimesheetStatus.All,
            name: 'All'
        },
        {
            value: APP_CONSTANT.TimesheetStatus.Pending,
            name: 'Pending'
        },
        {
            value: APP_CONSTANT.TimesheetStatus.Approve,
            name: 'Approved'
        },
        {
            value: APP_CONSTANT.TimesheetStatus.Reject,
            name: 'Rejected'
        }
    ],
    EnumDayOfWeek: [
        {
            value: APP_CONSTANT.EnumDayOfWeek.Monday,
            name: 'Monday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Tuesday,
            name: 'Tuesday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Wednesday,
            name: 'Wednesday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Thursday,
            name: 'Thursday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Friday,
            name: 'Friday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Saturday,
            name: 'Saturday'
        },
        {
            value: APP_CONSTANT.EnumDayOfWeek.Sunday,
            name: 'Sunday'
        }
    ],
    TimesheetViewBys: [
        {
            value: APP_CONSTANT.TimesheetViewBy.Project,
            name :'Project'
        },
        {
            value: APP_CONSTANT.TimesheetViewBy.People,
            name :'People'
        }
    ],
    TimesheetSupervisiorViewBys: [
        {
            value: APP_CONSTANT.TimesheetViewBy.Project,
            name :'Project'
        },
        {
            value: APP_CONSTANT.TimesheetViewBy.People,
            name :'People'
        },
    ],
    TypeViewHomePage: [
        {
            value: APP_CONSTANT.TypeViewHomePage.Week,
            name: 'Week'
        },
        {
            value: APP_CONSTANT.TypeViewHomePage.Month,
            name: 'Month'
        },
        {
            value: APP_CONSTANT.TypeViewHomePage.Quater,
            name: 'Quarter'
        },
        {
            value: APP_CONSTANT.TypeViewHomePage.Year,
            name: 'Year'
        },
        {
            value: APP_CONSTANT.TypeViewHomePage.AllTime,
            name: 'All Time'
        },
        {
            value: APP_CONSTANT.TypeViewHomePage.CustomTime,
            name: 'Custom Time'
        }
    ],
    EnumTypeWork: [
        {
            value: APP_CONSTANT.EnumTypeWork.Temp,
            name: 'Temp'
        },
        {
            value: APP_CONSTANT.EnumTypeWork.Offical,
            name: 'Offical'
        }
    ],
}
