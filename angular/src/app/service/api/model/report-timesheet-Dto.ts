
export class ReportTimeSheetDto {
  userName: string;
  dateAt: string;
  typeOfWork: number;
  taskName: string;
  note: string;
  workingTime: number;
  id: number;
  roleName: string;
  targetUserName: string;
  targetUserWorkingTime: number;
  isShadow: boolean;
}

export class WorkingReportDTO {
  userId: number;
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  fullName: string;
  totalWorkingHour: number;
  totalWorkingday: number;
  totalWorkingHourOfMonth: number;
  totalOpenTalk: number;
  type: number;
  branchName: string;
  branch: number;
  avatarPath: string;
  avatarFullPath: string;
  level: number;
  sex: number;
  isPM: boolean;
  isUnlockPM: boolean;
  isUnlock: boolean;
  branchDisplayName: number;

  listWorkingHour: WorkingHourDto[];
}
export interface PunishmentNoteReply {
  punishmentType: number;
  punishmentName: string;
  noteReply: string;
}
export interface SubmitUserComplaintDto {
  userPunishmentId: number;
  userNote: string;
}
export interface UserComplaintResultDto {
  userPunishmentId: number;
  timekeepingId?: number;
  userNote: string;
  punishmentType: number;
  punishmentTypeName: string;
  success: boolean;
  message: string;
}
export class TimekeepingDto {
  timekeepingId: number;
  userId: number;
  userName: string;
  userEmail: string;
  avatarPath: string;
  avatarFullPath: string;
  branch: number;
  userCode: string;
  date: string;
  registrationTimeStart: string;
  registrationTimeEnd: string;
  checkIn: string;
  checkOut: string;
  resultCheckIn: string;
  resultCheckOut: string;
  status: number;
  editByUserName: string;
  isEditing: boolean;
  isComplain?:boolean;
  userNote:string;
  noteReply: string;
  statusPunish: number;
  moneyPunish: number;
  trackerTime: string;
  noteReplyToString: string;
  moneyPunishtmp: number;
  strTimekeepingId: string;
  dailyPunish?: number;
  mentionPunish?: number;
  complaints?: { punishmentType: string, date: string, userNote: string }[];
  userPunishmentType?: number;
   dailyLatePunish?: number;
  dailyNoCheckInPunish?: number;
  dailyNoCheckOutPunish?: number;
  dailyLateAndNoCheckOutPunish?: number;
  dailyNoCheckInAndNoCheckOutPunish?: number;
  
  
  attendancePunish?: number;    
  trackerPunish?: number;        
  reviewInternPunish?: number; 
  pmReportPunish?: number;     
  antPunish?: number;
  unlockTSPunish?: number;       
  unlockTSGmailPunish?: number;
  unlockTSIMSPunish?: number;
  unlockTSStaffPunish?: number;
  unlockTSPMPunish?: number;
  
    
  structuredUserNotes?: Array<{
    punishmentType: number;
    punishmentName: string;
    userNote: string;
  }>;

  
  totalDayPunishment?: number;
  totalMonthPunishmentTotal?: number;
  totalPaidPunishment?: number;
  
  totalAttendancePunish?: number;
  totalDailyPunish?: number;
  totalMentionPunish?: number;
  totalTrackerPunish?: number;
  totalReviewInternPunish?: number;
  totalPmReportPunish?: number;
  totalAntPunish?: number;
  totalUnlockTSPunish?: number;
  totalUnlockTSStaffPunish?: number;
  totalUnlockTSPMPunish?: number;
  
 
  structuredNoteReplies?: PunishmentNoteReply[];
  showAllReplies?: boolean; 
  showAllComplaints?: boolean;
  shouldShowMore?: boolean;

  
}

export class UpdateTimekeepingDto{
  id: number;
  userCode: string;
  userEmail: string;
  userName: string;
  checkIn: string;
  checkOut: string;
  date: string;
  registerCheckIn: string;
  registerCheckOut: string;
  userId: number;
  statusPunish: number;
  trackerTime: string;
}

export class TardinessDto{
  userId: number;
  userName: string;
  avatarPath: string;
  avatarFullPath: string;
  branch: number;
  userCode: string;
  userEmail: string;
  numberOfTardies: number;
  numberOfLeaveEarly: number;
  month: number;
  year: number;
}

export class WorkingHourDto {
  date: number;
  dayName: string;
  workingHour: number;
  isOpenTalk: boolean;
  absenceType: number;
  offHour: number;
  isLock: boolean;
  isOnsite: boolean;

  isOffDaySetting: boolean;
  isThanDefaultWorkingHourPerDay: boolean;
  listAbsenceDetaiInDay: AbsenceDetaiInDay[];
  isNoOffAndNoCheckIn: boolean;
  isNoCheckIn: boolean;
}
export class AbsenceDetaiInDay {
  absenceType: number;
  hour: number;
  absenceTime: number;
  type: number;
}
export class OffDayDTO {
  dayOff: string;
  name: string;
  coefficient: number;
  id: number;
}

export class DateOfMonthDto {
  date: number;
  day: string;
  isOffDay: boolean;
}


export class KumoTrackerDto {
  fullName: string;
  emaillAddress: string;
  avatarPath: string;
  avatarFullPath: string;
  branch: number;
  dateAt:string;
  workingMinute: number;
  computerName:string;
}

export class GetNormalWorkingHourByUserLoginDto {
  totalWorkingHour: number;
  totalOpenTalk: number;
  listWorkingHour: WorkingHourDto[];
}

export class DayProcessListWithStatusDto {
  date : string;
  status : string;
  constructor(date : string, status : string) {
    this.date = date;
    this.status = status;
  }
}