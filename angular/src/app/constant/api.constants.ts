import { List } from "lodash";

export const PROJECT_MANAGER = {
  searchProject: '/api/services/app/ProjectService/Filter',
};

export const APP_CONSTANT = {
  EnumProjectStatus: {
    Active: 0,
    Deactive: 1,
    All: 2
  },
  EnumProjectType: {
    Timeandmaterials: 0,
    Fixedfee: 1,
    Nonbillable: 2,
    ODC: 3,
    Product: 4,
    Training: 5,
    NoSalary: 6
  },
  EnumTaskType: {
    Commontask: 0,
    Orthertask: 1
  },
  EnumUserType: {
    Member: 0,
    PM: 1,
    Shadow: 2,
    DeActive: 3
  },
  EnumTypeOfWork: {
    All: -1,
    Normalworkinghours: 0,
    Overtime: 1
  },
  TimesheetStatus: {
    All: -1,
    Draft: 0,
    Pending: 1,
    Approve: 2,
    Reject: 3
  },
  EnumDayOfWeek: {
    Monday: 0,
    Tuesday: 1,
    Wednesday: 2,
    Thursday: 3,
    Friday: 4,
    Saturday: 5,
    Sunday: 6
  },

  EnumDayOfWeekByGetDay: {
    Monday: 1,
    Tuesday: 2,
    Wednesday: 3,
    Thursday: 4,
    Friday: 5,
    Saturday: 6,
    Sunday: 0
  },

  TimesheetViewBy: {
    Project: 0,
    People: 1
  },
  TypeViewHomePage: {
    Week: 0,
    Month: 1,
    Quater: 2,
    Year: 3,
    AllTime: 4,
    CustomTime: 5
  },
  MyTimesheetView: {
    Day: 0,
    Week: 1
  },
  MAX_WORKING_TIME: 960,
  CHECK_STATUS: {
    CHECKED_NONE: 0,
    CHECKED_SOME: 1,
    CHECKED_ALL: 2,
  },
  BRANCH: {
    HN: 0,
    DN: 1,
    HCM: 2,
    Vinh: 3
  },
  LEVEL: {
    Intern_0 : 0,
    Intern_1 : 1,
    Intern_2 : 2,
    Intern_3 : 3,
    "Fresher-" : 4,
    "Fresher" : 5,
    "Fresher+" : 6,
  },
  TYPE: {
    Staff : 0,
    Collaborator : 2,
  },
  HISTORYLEVEL: {
    Intern_0 : 0,
    Intern_1 : 1,
    Intern_2 : 2,
    Intern_3 : 3,
    "Fresher-" : 4,
    "Fresher" : 5,
    "Fresher+" : 6,
  },
  AbsenceStatus: {
    New:0,
    Pending: 1,
    Approved: 2,
    Rejected: 3
  },
  ReviewStatus: {
    'Draft': 0,
    'Reviewed': 1,
    'Approved': 2,
    'Sent Email': 3,
    'Rejected':-1,
  },
  AbsenceType: {
    FullDay: 1,
    Morning: 2,
    Afternoon: 3,
    Custom: 4
  },
  DayAbsenceType: {
    Off: 0,
    Onsite: 1,
    Remote: 2,
  },
  ListYear: [
    new Date().getFullYear() - 5,
    new Date().getFullYear() - 4,
    new Date().getFullYear() - 3,
    new Date().getFullYear() - 2,
    new Date().getFullYear() - 1,
    new Date().getFullYear(),
    new Date().getFullYear() + 1,
  ],
  CHANGE_LEVEL:[
    {value: 1, text: 'Level up'},
    {value: 2, text: 'Level not change'}
  ],
  EnumTypeWork: {
    Temp: true,
    Offical: false
  },
  AbsenceStatusFilter: {
    'All': -1,
    'Pending or Approved': 0,
    'Pending': 1,
    'Approved': 2,
    'Rejected': 3
  },
  MyTimesheetStatusFilter: {
    'All': -1,
    'New': 0,
    'Pending or Approved': 1,
    'Pending': 2,
    'Approved': 3,
    'Rejected': 4
  },
  FILTER_DEFAULT: {
    'All': -1,
  },

  EnumUserStatus: {
    Active: true,
    InActive: false
  },
  OnDayType: {
    BeginOfDay: 1,
    EndOfDay: 3
  },
  CellColor : {
    Normal : 0,
    Begin : 1,
    Staff : 2,
    End : 3,
    BeginHasRivew : 4,
    EndHasRivew : 5,
    BeginAndEnd : 6,
    BeginAndStaff : 7,
  },
  TsStatusFilter: {
    'Approved': 1,
    'Pending and Approved': 2,
  },
  CheckInFilter: {
    'All': -1,
    'No Check In': 1,
    'No Check Out': 2,
    'No Check In & No Check Out': 3,
    'No Check In & No Check Out but have TS': 4
  },

  HaveCheckInFilter: {
    'All': -1,
    'Have Check In': 1,
    'Have Check Out': 2,
    'Have Check In & Have Check Out': 3,
    'Have Check In or Have Check Out': 4,
    'No Check In & No Check Out': 5
  },
    PunishRules: [
    { "name": "Không phạt", "value": 0 },
    { "name": "Đi muộn", "value": 1 },
    { "name": "Không CheckIn", "value": 2 },
    { "name": "Không CheckOut", "value": 3 },
    { "name": "Đi muộn và Không CheckOut", "value": 4 },
    { "name": "Không CheckIn và không CheckOut", "value": 5 }
  ]
};