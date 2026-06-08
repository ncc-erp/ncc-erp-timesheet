import { List } from "lodash";

export const PROJECT_MANAGER = {
  searchProject: '/api/services/app/ProjectService/Filter',
};

export const BRANCH_CODES = [
  'HN1',
  'HN2',
  'SG1',
  'SG2',
  'ĐN',
  'Vinh',
  'QN',
  'HN3'
];

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
  TypeViewBranchManager: {
    All : -1,
    Day : 0,
    Week: 1,
    Month: 2,
    Year: 3
  },
  TimekeepingApiType: {
    Add: 'add',
    Snapshot: 'snapshot'
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
    Internship : 1,
    Collaborator : 2,
    Probation : 3,
    Vendor : 5
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
    'PM Reviewed': 4,
    'ReOpen' : 5,
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
    "Đi muộn Về sớm": 3,
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
  DEFAULT_OPENTALK_TASK_NAME: 'Open Talk',
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
  UserStatusFilter: {
    'All': 0,
    'Active': 1,
    'Deactive': 2,
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
  WorkLocation: {
    All: -1,
    Onsite: 1,
    Remote: 2,
    Office: 3,
  },
  OvertimeFilter: {
    All: -1,
    NonCharged: 0,
    Charged: 1
  },
  PunishRules: [
    { "name": "No Punish", "value": 0 },
    { "name": "Late", "value": 1 },
    { "name": "No CheckIn", "value": 2 },
    { "name": "No CheckOut", "value": 3 },
    { "name": "Late and No CheckOut", "value": 4 },
    { "name": "No CheckIn and No CheckOut", "value": 5 },
    { "name": "Daily", "value": 6 },
    { "name": "Mention", "value": 7 },
    { "name": "Tracker below 85% requirement", "value": 8 },
    { "name": "Tracker below 75% requirement", "value": 9 },
    { "name": "Tracker below 50% requirement", "value": 10 },
    { "name": "Tracker below 25% requirement", "value": 11 },
    { "name": "Late Intern Review", "value": 12 },
    { "name": "Report between 15:00-17:00", "value": 13 },
    { "name": "Report after 17:00 or forgot to report", "value": 14 },
    { "name": "Ant", "value": 15 },
    { "name": "Unlock Timesheet Gmail", "value": 16 },
    { "name": "Unlock Timesheet IMS", "value": 17 },
    { "name": "Unlock Timesheet PM", "value": 18 },
    { "name": "Unlock Timesheet Staff", "value": 19 },
    { "name": "PM Others", "value": 20 },
    { "name": "Early CheckOut", "value": 21 }
  ],
  PunishRulesShortName: [
    { "name": "No Punish", "value": 0 },
    { "name": "Late", "value": 1 },
    { "name": "No CheckIn", "value": 2 },
    { "name": "No CheckOut", "value": 3 },
    { "name": "Late + No CheckOut", "value": 4 },
    { "name": "No Checkin-Checkout", "value": 5 },
    { "name": "Daily", "value": 6 },
    { "name": "Mention", "value": 7 },
    { "name": "Tracker <85%", "value": 8 },
    { "name": "Tracker <75%", "value": 9 },
    { "name": "Tracker <50%", "value": 10 },
    { "name": "Tracker <25%", "value": 11 },
    { "name": "Late Intern Review", "value": 12 },
    { "name": "Report 15:00-17:00", "value": 13 },
    { "name": "Report after 17:00", "value": 14 },
    { "name": "Ant", "value": 15 },
    { "name": "UnlockTS Gmail", "value": 16 },
    { "name": "UnlockTS IMS", "value": 17 },
    { "name": "UnlockTS PM", "value": 18 },
    { "name": "UnlockTS Staff", "value": 19 },
    { "name": "PM Others", "value": 20 },
    { "name": "Early CheckOut", "value": 21 }
  ],

  PunishmentGroups: {
    NO_PUNISH: [0],
    CHECK_IN_OUT: [1, 2, 3, 4, 5, 21],
    TRACKER: [8, 9, 10, 11],
    PM_REPORT: [13, 14],
    DAILY: [6],
    MENTION: [7],  
    LATE_INTERN_REVIEW: [12],             
    ANT: [15],
    UNLOCK_TS_GMAIL: [16],
    UNLOCK_TS_IMS: [17],
    UNLOCK_TS_PM: [18],
    UNLOCK_TS_STAFF: [19],
    PM_OTHERS: [20]
  },
  
  PUNISHMENT_TYPES: [
    { value: 0, name: 'No Punish' },
    { value: 1, name: 'Late' },
    { value: 2, name: 'No Check In' },
    { value: 3, name: 'No Check Out' },
    { value: 4, name: 'Late & No CheckOut' },
    { value: 5, name: 'No CheckIn & Out' },
    { value: 6, name: 'Daily' },
    { value: 7, name: 'Mention' },
    { value: 8, name: 'Tracker 20k' },
    { value: 9, name: 'Tracker 50k' },
    { value: 10, name: 'Tracker 100k' },
    { value: 11, name: 'Tracker 200k' },
    { value: 12, name: 'Review Intern' },
    { value: 13, name: 'PM Report 20k' },
    { value: 14, name: 'PM Report 50k' },
    { value: 15, name: 'Ant' },
    { value: 16, name: 'UnlockTS Gmail' },
    { value: 17, name: 'UnlockTS IMS' },
    { value: 18, name: 'UnlockTS PM' },
    { value: 19, name: 'UnlockTS Staff' },
    { value: 20, name: 'PM Others'},
    { value: 21, name: 'Early CheckOut' }
  ],
  
  PunishmentTypeMap: {
    0: ['NO_PUNISH', 'CHECK_IN_OUT', 'DAILY', 'MENTION', 'TRACKER', 'LATE_INTERN_REVIEW', 'PM_REPORT', 'ANT', 'UNLOCK_TS'], 
    1: ['NO_PUNISH', 'CHECK_IN_OUT'], 
    2: ['NO_PUNISH', 'CHECK_IN_OUT'],
    3: ['NO_PUNISH', 'CHECK_IN_OUT'],
    4: ['NO_PUNISH', 'CHECK_IN_OUT'],
    5: ['NO_PUNISH', 'CHECK_IN_OUT'],
    6: ['NO_PUNISH', 'DAILY'],
    7: ['NO_PUNISH', 'MENTION'],
    8: ['NO_PUNISH', 'TRACKER'],
    9: ['NO_PUNISH', 'TRACKER'], 
    10: ['NO_PUNISH', 'TRACKER'], 
    11: ['NO_PUNISH', 'TRACKER'], 
    12: ['NO_PUNISH', 'LATE_INTERN_REVIEW'],
    13: ['NO_PUNISH', 'PM_REPORT'], 
    14: ['NO_PUNISH', 'PM_REPORT'],
    15: ['NO_PUNISH', 'ANT'], 
    16: ['NO_PUNISH', 'UNLOCK_TS_GMAIL'],
    17: ['NO_PUNISH', 'UNLOCK_TS_IMS'],
    18: ['NO_PUNISH', 'UNLOCK_TS_PM'],
    19: ['NO_PUNISH', 'UNLOCK_TS_STAFF'],    
    20: ['NO_PUNISH', 'PM_OTHERS'],
    21: ['NO_PUNISH', 'CHECK_IN_OUT']
  },
  GroupTypes: [
    { id: -1, name: 'All' },
    { id: 0, name: 'No Punish' },
    { id: 1, name: 'Check In/Out' },
    { id: 2, name: 'Tracker' },
    { id: 3, name: 'PM Report' },
    { id: 6, name: 'Daily' },
    { id: 7, name: 'Mention' },
    { id: 12, name: 'Late Intern Review' },
    { id: 15, name: 'Ant' },
    { id: 16, name: 'Unlock Timesheet Gmail' },
    { id: 17, name: 'Unlock Timesheet IMS' },
    { id: 18, name: 'Unlock Timesheet PM' },
    { id: 19, name: 'Unlock Timesheet Staff' },
    { id: 20, name: 'PM Others' }
  ],
  ProjectMemberType: {
    Expose: 0,
    Shadow: 1,
    Deactive: 2,
    All: 3
  },
  NotifyChannel: {
    KOMU : 0,
    Mezon : 1
  },
  TimesheetErrorCode: {
    TIMESHEET_LOCKED: 1,
    PUNISHMENT_UNPAID: 2
  }
};
