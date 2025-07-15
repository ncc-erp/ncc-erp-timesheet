import { TimekeepingService } from './../../../service/api/timekeeping.service';
import { DatePipe } from '@angular/common';
import { TimekeepingDto, UpdateTimekeepingDto } from './../../../service/api/model/report-timesheet-Dto';
import { userDTO } from './../../check-board/create-check-board/create-check-board.component';
import { CalendarView } from 'angular-calendar';
import { FormControl } from '@angular/forms';
import { PERMISSIONS_CONSTANT } from './../../../constant/permission.constant';
import { AppComponentBase } from 'shared/app-component-base';
import { Component, OnInit, Injector, AfterViewChecked, ElementRef, ViewChildren, QueryList, ChangeDetectorRef } from '@angular/core';
import { MatDialog } from '@angular/material';
import { ComplainDialogComponent } from './complain-dialog.component';

@Component({
  selector: 'app-mytimesheet-tardiness',
  templateUrl: './mytimesheet-tardiness.component.html',
  styleUrls: ['./mytimesheet-tardiness.component.css'],
  providers: [DatePipe]
})
export class MytimesheetTardinessComponent extends AppComponentBase implements OnInit, AfterViewChecked {

  EDIT_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.EditTardinessLeaveEarly;
  VIEW_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewTardinessLeaveEarly;
  Timekeeping_UserNote = PERMISSIONS_CONSTANT.Timekeeping_UserNote;
  // listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  // listYear = APP_CONSTANT.ListYear;
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
  public maskTime = [/\d/, /\d/, ':', /\d/, /\d/];
  @ViewChildren('complaintContent') complaintContents: QueryList<ElementRef>;

  constructor(
    private timekeepingService: TimekeepingService,
    private dialog: MatDialog,
    injector: Injector,
    private cdr: ChangeDetectorRef,
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
  }

  ngOnInit() {
    this.getData();
  }

  ngAfterViewChecked() {
    if (this.complaintContents && this.groupedTimekeeping) {
      let changed = false;
      this.complaintContents.forEach((elRef: ElementRef) => {
        const id = elRef.nativeElement.getAttribute('data-id');
        const item = this.groupedTimekeeping.find(x => x.timekeepingId == id);
        if (elRef && elRef.nativeElement && item) {
          // Chỉ đo khi chưa expanded
          if (!item.showAllComplaints) {
            const lineHeight = parseFloat(getComputedStyle(elRef.nativeElement).lineHeight) || 20;
            const height = elRef.nativeElement.offsetHeight;
            const lines = Math.round(height / lineHeight);
            const shouldShow = lines > 5;
            if (item.shouldShowMore !== shouldShow) {
              item.shouldShowMore = shouldShow;
              changed = true;
            }
          }
        }
      });
      if (changed) {
        this.cdr.detectChanges();
      }
    }
  }

  getData() {
    this.isTableLoading = true;
    this.timekeepingService.getMyDetails(this.year, this.month + 1).subscribe(res => {
      this.listTimekeeping = res.result;
      
      // Calculate monthly totals for each punishment category
      if (this.listTimekeeping && this.listTimekeeping.length > 0) {
        // Initialize totals
        let totalAttendancePunish = 0;
        let totalDailyPunish = 0;
        let totalMentionPunish = 0;
        let totalTrackerPunish = 0;
        let totalReviewInternPunish = 0;
        let totalPmReportPunish = 0;
        let totalAntPunish = 0;
        let totalUnlockTSPunish = 0;
        
        // Sum up all punishments by type
        this.listTimekeeping.forEach(item => {
          const punishType = item.userPunishmentType;
          const moneyAmount = item.moneyPunish || 0;
          
          if (punishType >= 1 && punishType <= 5) {
            // Attendance-related punishments
            totalAttendancePunish += moneyAmount;
          } else if (punishType === 6) {
            // Daily punishment
            totalDailyPunish += moneyAmount;
          } else if (punishType === 7) {
            // Mention punishment
            totalMentionPunish += moneyAmount;
          } else if (punishType >= 8 && punishType <= 11) {
            // Tracker punishments
            totalTrackerPunish += moneyAmount;
          } else if (punishType === 12) {
            // Review Intern punishment
            totalReviewInternPunish += moneyAmount;
          } else if (punishType === 13 || punishType === 14) {
            // PM Report punishments
            totalPmReportPunish += moneyAmount;
          } else if (punishType === 15) {
            // Ant punishment
            totalAntPunish += moneyAmount;
          } else if (punishType === 16) {
            // UnlockTS punishment
            totalUnlockTSPunish += moneyAmount;
          }
        });
        
        // Store totals in the first record for easy access in the template
        if (this.listTimekeeping[0]) {
          this.listTimekeeping[0].totalAttendancePunish = totalAttendancePunish;
          this.listTimekeeping[0].totalDailyPunish = totalDailyPunish;
          this.listTimekeeping[0].totalMentionPunish = totalMentionPunish;
          this.listTimekeeping[0].totalTrackerPunish = totalTrackerPunish;
          this.listTimekeeping[0].totalReviewInternPunish = totalReviewInternPunish;
          this.listTimekeeping[0].totalPmReportPunish = totalPmReportPunish;
          this.listTimekeeping[0].totalAntPunish = totalAntPunish;
          this.listTimekeeping[0].totalUnlockTSPunish = totalUnlockTSPunish;
        }
        
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

    // Create a map to group entries by date
    const dateMap = new Map<string, TimekeepingDto>();

    // Process each record
    this.listTimekeeping.forEach(item => {
      const dateKey = new Date(item.date).toISOString().split('T')[0]; // Format: YYYY-MM-DD
      
      if (!dateMap.has(dateKey)) {
        // First entry for this date - create a base record
        dateMap.set(dateKey, {
          ...item,
          // Initialize all punishment fields
          attendancePunish: 0,     // For types 1-5 (Late, No Check In, etc.)
          dailyPunish: 0,          // For type 6
          mentionPunish: 0,        // For type 7
          trackerPunish: 0,        // For types 8-11
          reviewInternPunish: 0,   // For type 12
          pmReportPunish: 0,       // For types 13-14
          antPunish: 0,            // For type 15
          unlockTSPunish: 0,       // For type 16
          totalDayPunishment: 0,
          structuredNoteReplies: [],
          structuredUserNotes: [],  // Store multiple user notes (complaints) by punishment type
          showAllReplies: false,
          showAllComplaints: false  // Toggle for showing all complaints
        });
      }
      
      // Add specific punishment type amounts based on UserPunishmentType
      const record = dateMap.get(dateKey);
      const punishType = item.userPunishmentType;
      const moneyAmount = item.moneyPunish || 0;
      
      // Process user notes (complaints) by punishment type
      if (item.userNote && item.userNote.trim()) {
        // Get the punishment name from the constants
        const punishmentName = this.getPunishmentTypeName(punishType);
        
        // Check if we already have a complaint for this punishment type
        const existingNoteIndex = record.structuredUserNotes ? record.structuredUserNotes.findIndex(
          note => note.punishmentType === punishType
        ) : -1;
        
        if (existingNoteIndex === -1) {
          // Add new complaint
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
    this.groupedTimekeeping.forEach(item => { item.showAllComplaints = false; });
      console.log(this.groupedTimekeeping.map(i => i.showAllComplaints));
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
    
    // Use the constants from API constants
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
            console.error('Error submitting complaints:', error);
            this.notify.error('Failed to submit complaints');
          });
      }
    });
  }
  
  toggleNoteReplies(item: TimekeepingDto) {
    item.showAllReplies = !item.showAllReplies;
  }
  
  toggleUserNotes(item: any) {
    item.showAllComplaints = !item.showAllComplaints;
    if (!item.showAllComplaints) {
      setTimeout(() => this.ngAfterViewChecked());
    }
  }

  getAttendancePunishmentTypes(item: TimekeepingDto): string {
    const attendanceReplies = item.structuredNoteReplies ? item.structuredNoteReplies.filter(reply => 
      reply.punishmentType >= 1 && reply.punishmentType <= 5
    ) : [];
    
    if (attendanceReplies && attendanceReplies.length > 0) {
      return attendanceReplies.map(reply => reply.punishmentName).join(', ');
    }
    
    if (item.userPunishmentType && item.userPunishmentType >= 1 && item.userPunishmentType <= 5) {
      const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === item.userPunishmentType);
      return punishmentType ? punishmentType.name : 'Attendance';
    }
    
    return 'Attendance';
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
        console.error('Error fetching user punishments:', error);
        this.notify.error('Failed to load punishment details');
      }
    );
  }
}