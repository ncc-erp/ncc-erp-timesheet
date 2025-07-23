import { TimekeepingService } from './../../../service/api/timekeeping.service';
import { DatePipe } from '@angular/common';
import { TimekeepingDto, UpdateTimekeepingDto } from './../../../service/api/model/report-timesheet-Dto';
import { userDTO } from './../../check-board/create-check-board/create-check-board.component';
import { CalendarView } from 'angular-calendar';
import { FormControl } from '@angular/forms';
import { PERMISSIONS_CONSTANT } from './../../../constant/permission.constant';
import { AppComponentBase } from 'shared/app-component-base';
import { Component, OnInit, Injector } from '@angular/core';
import { MatDialog } from '@angular/material';
import { ComplainDialogComponent } from './complain-dialog/complain-dialog.component';

@Component({
  selector: 'app-mytimesheet-tardiness',
  templateUrl: './mytimesheet-tardiness.component.html',
  styleUrls: ['./mytimesheet-tardiness.component.css'],
  providers: [DatePipe]
})
export class MytimesheetTardinessComponent extends AppComponentBase implements OnInit {

  EDIT_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.EditTardinessLeaveEarly;
  VIEW_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewTardinessLeaveEarly;
  Timekeeping_UserNote = PERMISSIONS_CONSTANT.Timekeeping_UserNote;
  month;
  months;
  year;
  years;
  view: CalendarView;
  viewDate: Date;
  calendarView;
  userControl: FormControl;
  listTimekeeping: TimekeepingDto[] = [];
  groupedTimekeeping: TimekeepingDto[] = [];
  userId: number;
  userName: string;
  isTableLoading: boolean = false;
  selectedDay: number = -1;
  dayList: any = []
  public countLate: number = 0;
  totalMonthlyPunishment: number = 0;
  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/];
  isBasicUser: boolean = false;

  constructor(
    private timekeepingService: TimekeepingService,
    private dialog: MatDialog,
    injector: Injector,
  ) {
    super(injector);
    this.view = CalendarView.Month;
    this.viewDate = new Date();
    this.viewDate.setMonth(new Date().getMonth());
    this.calendarView = CalendarView;
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;
    this.userId = this.appSession.userId
    this.userControl = new FormControl(this.userId);
    this.updateDay();
    this.userName = this.appSession.user.surname + ' ' + this.appSession.user.name;
    this.isBasicUser = this.appSession.user && this.appSession.user.type === 0;
  }

  ngOnInit() {
    this.getData();
  }

  getData() {
    this.isTableLoading = true;
    this.timekeepingService.getMyDetails(this.year, this.month + 1).subscribe(res => {
      this.listTimekeeping = res.result;
      if (this.listTimekeeping && this.listTimekeeping.length > 0) {
        this.totalMonthlyPunishment = this.listTimekeeping[0].totalMonthPunishmentTotal;
      }
      
      this.groupTimekeepingByDay();
      this.isTableLoading = false;
      this.countLate = this.countPunish(res.result);
    });
  }

  groupTimekeepingByDay() {
    if (!this.listTimekeeping || this.listTimekeeping.length === 0) {
      this.groupedTimekeeping = [];
      return;
    }

    const dateMap = new Map<string, TimekeepingDto>();

    this.listTimekeeping.forEach(item => {
      const dateKey = new Date(item.date).toISOString().split('T')[0]; 
      
      if (!dateMap.has(dateKey)) {
        dateMap.set(dateKey, {
          ...item,
          attendancePunish: 0,     
          dailyPunish: 0,          
          mentionPunish: 0,       
          trackerPunish: 0,        
          reviewInternPunish: 0,  
          pmReportPunish: 0,       
          antPunish: 0,            
          unlockTSPunish: 0,      
          totalDayPunishment: 0,
          structuredNoteReplies: [],
          structuredUserNotes: [],  
          showAllReplies: false,
          showAllComplaints: false  
        });
      }
      
      const record = dateMap.get(dateKey);
      const punishType = item.userPunishmentType;
      const moneyAmount = item.moneyPunish || 0;

      if (item.userNote && item.userNote.trim()) {
        const punishmentName = this.getPunishmentTypeName(punishType);

        const existingNoteIndex = record.structuredUserNotes ? record.structuredUserNotes.findIndex(
          note => note.punishmentType === punishType
        ) : -1;
        
        if (existingNoteIndex === -1) {
          if (!record.structuredUserNotes) {
            record.structuredUserNotes = [];
          }
          
          record.structuredUserNotes.push({
            punishmentType: punishType,
            punishmentName: punishmentName,
            userNote: item.userNote
          });
        } else if (record.structuredUserNotes[existingNoteIndex].userNote !== item.userNote) {
          record.structuredUserNotes[existingNoteIndex].userNote = item.userNote;
        }
        if (!record.userNote) {
          record.userNote = item.userNote;
        }
      }

      if (item.noteReply && item.noteReply.trim()) {
        const punishmentName = this.getPunishmentTypeName(punishType);
        const existingReplyIndex = record.structuredNoteReplies.findIndex(
          reply => reply.punishmentType === punishType
        );
        
        if (existingReplyIndex === -1) {
          record.structuredNoteReplies.push({
            punishmentType: punishType,
            punishmentName: punishmentName,
            noteReply: item.noteReply
          });
        } else if (record.structuredNoteReplies[existingReplyIndex].noteReply !== item.noteReply) {
          record.structuredNoteReplies[existingReplyIndex].noteReply = item.noteReply;
        }

        if (!record.noteReply) {
          record.noteReply = item.noteReply;
        }
      }

      if (punishType >= 1 && punishType <= 5) {
        record.attendancePunish = (record.attendancePunish || 0) + moneyAmount;
      } else if (punishType === 6) {
        record.dailyPunish = (record.dailyPunish || 0) + moneyAmount;
      } else if (punishType === 7) {
        record.mentionPunish = (record.mentionPunish || 0) + moneyAmount;
      } else if (punishType >= 8 && punishType <= 11) {
        record.trackerPunish = (record.trackerPunish || 0) + moneyAmount;
      } else if (punishType === 12) {
        record.reviewInternPunish = (record.reviewInternPunish || 0) + moneyAmount;
      } else if (punishType === 13 || punishType === 14) {
        record.pmReportPunish = (record.pmReportPunish || 0) + moneyAmount;
      } else if (punishType === 15) {
        record.antPunish = (record.antPunish || 0) + moneyAmount;
      } else if (punishType === 16) {
        record.unlockTSPunish = (record.unlockTSPunish || 0) + moneyAmount;
      }

      record.totalDayPunishment = (
        (record.attendancePunish || 0) + 
        (record.dailyPunish || 0) + 
        (record.mentionPunish || 0) + 
        (record.trackerPunish || 0) + 
        (record.reviewInternPunish || 0) + 
        (record.pmReportPunish || 0) + 
        (record.antPunish || 0) + 
        (record.unlockTSPunish || 0)
      );
    });

    dateMap.forEach(record => {
      if (record.structuredNoteReplies && record.structuredNoteReplies.length > 0) {
        record.structuredNoteReplies.sort((a, b) => a.punishmentType - b.punishmentType);
      }
      
      if (record.structuredUserNotes && record.structuredUserNotes.length > 0) {
        record.structuredUserNotes.sort((a, b) => a.punishmentType - b.punishmentType);
      }
    });

    this.groupedTimekeeping = Array.from(dateMap.values())
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  }

  countPunish(data) {
    return data.filter((item) => {
      return item.statusPunish != 0 || item.status == 1
    }).length;
  }

  updateDay(): void {
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
    this.getDayByMonthAndYear(this.month, this.year)
  }

  getDayByMonthAndYear(month: number, year: number) {
    let numOfday: number = new Date(year, month + 1, 0).getDate();
    this.dayList = []
    for (let i = 1; i <= numOfday; i++) {
      this.dayList.push(i)
    }
  }

  formatDate(date: string) {
    return new Date(date).toLocaleDateString("vi");
  }

  getCss1(value) {
    value = Number.parseInt(value);
    if (value > 15) {
      return "red";
    }
    return "green";
  }

  getCssClassByPunish(value?: number) {
    if(value != null) {
      if(value > 0) {
        return "red";
      }
      return "green";
    }
    return "";
  }

  formatPunishmentAmount(amount?: number): string {
    if (amount == null || amount === 0) {
      return '0';
    }
    return (amount / 1000) + 'k';
  }

  hasPunishment(item: TimekeepingDto): boolean {
    return item.totalDayPunishment > 0;
  }

  getPunishmentTypeName(type?: number): string {
    if (type == null) return 'No Punish';

    const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === type);
    return punishmentType ? punishmentType.name : `Unknown (${type})`;
  }

  onDateChange() {
    this.viewDate = new Date(this.year, this.month, this.selectedDay);
    this.getDayByMonthAndYear(this.month, this.year)
    this.getData();
    this.countLate = this.countPunish(this.listTimekeeping)
  }

  openComplainDialog(item: TimekeepingDto) {
    const currentDate = item.date;
    const userPunishmentTypes = [];

    const dayOfMonth = parseInt(item.date.split('-')[2]);
    const filteredUserPunishments = this.listTimekeeping.filter(up => {
      const punishmentDate = new Date(up.date);
      return punishmentDate.getDate() === dayOfMonth && up.userPunishmentType > 0;
    });
  
    filteredUserPunishments.forEach(up => {
      if (up.userPunishmentType > 0) {
        const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === up.userPunishmentType);
        if (punishmentType && !userPunishmentTypes.some(p => p.value === punishmentType.value)) {
          userPunishmentTypes.push(punishmentType);
        }
      }
    });

    if (userPunishmentTypes.length === 0) {
      this.notify.info('Không có loại phạt nào cho ngày này');
      return; 
    }

    const dialogRef = this.dialog.open(ComplainDialogComponent, {
      width: '650px',
      data: {
        timekeeping: item,
        punishmentTypes: userPunishmentTypes,
        structuredUserNotes: item.structuredUserNotes || [],
        userPunishments: filteredUserPunishments
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.length > 0) {
        const promises = result.map(complaint => {
          return this.timekeepingService.addComplain({
            userPunishmentId: complaint.userPunishmentId,
            userNote: complaint.userNote
          }).toPromise();
        });

        Promise.all(promises)
          .then(() => {
            this.notify.success('Complaints submitted successfully');
            this.getData();
          })
          .catch(error => {
            this.notify.error('Failed to submit complaints');
          });
      }
    });
  }
  
  toggleNoteReplies(item: TimekeepingDto) {
    item.showAllReplies = !item.showAllReplies;
  }
  
  toggleUserNotes(item: TimekeepingDto) {
    item.showAllComplaints = !item.showAllComplaints;
  }

  hasLongContent(notes: any[]): boolean {
    if (!notes || notes.length === 0) return false;
    const totalLength = notes.reduce((total, note) => {
      const titleLength = note.punishmentName ? note.punishmentName.length + 2 : 0; 
      const contentLength = note.userNote ? note.userNote.trim().length : 0;
      return total + titleLength + contentLength;
    }, 0);
    return totalLength > 57;
  }

  hasLongReplyContent(replies: any[]): boolean {
    if (!replies || replies.length === 0) return false;
    const totalLength = replies.reduce((total, reply) => {
      const titleLength = reply.punishmentName ? reply.punishmentName.length + 2 : 0; 
      const contentLength = reply.noteReply ? reply.noteReply.trim().length : 0;
      return total + titleLength + contentLength;
    }, 0);
    return totalLength > 57;
  }

  getLineContentCount(text: string): number {
    if (!text) return 0;
    return text.split('\n').length;
  }

  hasMultilineContent(notes: any[]): boolean {
    if (!notes || notes.length === 0) return false;
    return notes.some(note => note.userNote && this.getLineContentCount(note.userNote) > 3);
  }

  hasMultilineReplyContent(replies: any[]): boolean {
    if (!replies || replies.length === 0) return false;
    return replies.some(reply => reply.noteReply && this.getLineContentCount(reply.noteReply) > 3);
  }

  getAttendancePunishmentTypes(item: TimekeepingDto): string {
    const attendanceReplies = item.structuredNoteReplies ? item.structuredNoteReplies.filter(reply => 
      reply.punishmentType >= 1 && reply.punishmentType <= 5
    ) : [];
    
    if (!attendanceReplies || attendanceReplies.length === 0) {
      for (let i = 1; i <= 5; i++) {
        const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === i);
        if (punishmentType) {
          return punishmentType.name;
        }
      }
      return 'Attendance';
    }

    return attendanceReplies.map(reply => reply.punishmentName).join(', ');
  }

  getTimekeepingData() {
    this.timekeepingService.getMyDetails(this.year, this.month + 1).subscribe(res => {
      this.listTimekeeping = res.result;
      this.groupTimekeepingByDay();
      this.countLate = this.countPunish(res.result);
    });
  }

  trackerTimeFormat(time) {
    if (time == "" || time == null) {
      return "";
    }
    if (time == 0) {
      return 0;
    }
    const [hours, minutes] = time.split(":").slice(0, 2);
    const formattedHours = hours.length === 1 ? `0${hours}` : hours;
    const formattedMinutes = minutes.length === 1 ? `0${minutes}` : minutes;
    return `${formattedHours}:${formattedMinutes}`;
  }

  onSave(item: TimekeepingDto) {
    this.timekeepingService.getMyDetails(
      this.year, 
      this.month + 1
    ).subscribe(
      (response) => {
        const userPunishments = response && response.result ? response.result : [];

        const dayOfMonth = parseInt(item.date.split('-')[2]);
        const filteredUserPunishments = userPunishments.filter(up => {
          const punishmentDate = new Date(up.date);
          return punishmentDate.getDate() === dayOfMonth && up.userPunishmentType > 0;
        });
        
        if (filteredUserPunishments && filteredUserPunishments.length > 0) {
          this.openComplainDialog(item);
        } else {
          this.notify.info('Không có loại phạt nào cho ngày này');
        }
      },
      (error) => {
        this.notify.error('Failed to load punishment details');
      }
    );
  }
}