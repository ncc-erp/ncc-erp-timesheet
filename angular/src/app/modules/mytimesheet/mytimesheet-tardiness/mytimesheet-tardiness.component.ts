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
import { TimesheetConfirmationDialogComponent } from './timesheet-confirmation-dialog/timesheet-confirmation-dialog.component';
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { UserPunishmentPaidService, GetUserPunishmentBalanceDto, PreviewAndApplyPunishmentPointsDto } from '@app/service/api/user-punishment-paid.service';

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
  isBasicUser: boolean = false;
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
  isConfirmLoading: boolean = false;
  selectedDay: number = -1;
  dayList: any = []
  public countLate: number = 0;
  totalMonthlyPunishment: number = 0;
  totalPaidPunishment: number = 0;
  userBalance: GetUserPunishmentBalanceDto | null = null;
  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/];

  constructor(
    private timekeepingService: TimekeepingService,
    private dialog: MatDialog,
    private userService: UserServiceProxy,
    private userPunishmentPaidService: UserPunishmentPaidService,
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
    this.isBasicUser = false;
  }

  ngOnInit() {
    this.userService.get(this.userId).subscribe(user => {
      const hasOnlyBasicRole = user.roleNames && 
                             user.roleNames.length === 1 && 
                             user.roleNames[0].toUpperCase() === 'BASICUSER';
      this.isBasicUser = hasOnlyBasicRole;
      this.getData();
      this.loadUserBalance();
    });
  }

  getRemainingPunishment(): number {
    return this.userBalance && this.userBalance.totalPunishmentMoney ? this.userBalance.totalPunishmentMoney : 0;
  }

  loadUserBalance(): void {
    this.userPunishmentPaidService.getCurrentUserBalance().subscribe(
      (result) => {
        this.userBalance = result;
      },
      (error) => {
        console.error('Error loading user balance:', error);
        this.userBalance = null;
      }
    );
  }

  getData() {
    this.isTableLoading = true;
    this.timekeepingService.getMyDetails(this.year, this.month + 1).subscribe(res => {
      this.listTimekeeping = res.result;
      this.totalMonthlyPunishment = 0;
      this.totalPaidPunishment = 0;

      if (this.listTimekeeping && this.listTimekeeping.length > 0) {
        this.totalMonthlyPunishment = this.listTimekeeping[0].totalMonthPunishmentTotal || 0;
        this.totalPaidPunishment = this.listTimekeeping[0].totalPaidPunishment || 0;
      }
      
      this.groupTimekeepingByDay();
      this.isTableLoading = false;
      this.countLate = this.countPunish(res.result);
      this.loadUserBalance(); 
    });
  }

  groupTimekeepingByDay() {
    if (!this.listTimekeeping || this.listTimekeeping.length === 0) {
      this.groupedTimekeeping = [];
      return;
    }

    const dateMap = new Map<string, TimekeepingDto>();

    this.listTimekeeping.forEach(item => {
      const itemDate = new Date(item.date);
      const month = (itemDate.getMonth() + 1) < 10 ? '0' + (itemDate.getMonth() + 1) : (itemDate.getMonth() + 1);
      const day = itemDate.getDate() < 10 ? '0' + itemDate.getDate() : itemDate.getDate();
      const dateKey = `${itemDate.getFullYear()}-${month}-${day}`;
      
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
          unlockTSGmailPunish: 0,      
          unlockTSIMSPunish: 0,      
          unlockTSStaffPunish: 0,
          unlockTSPMPunish: 0,
          totalDayPunishment: 0,
          structuredNoteReplies: [],
          structuredUserNotes: [],  
          showAllReplies: false,
          showAllComplaints: false  
        });
      }
      
      const record = dateMap.get(dateKey);
      let punishType = item.userPunishmentType;
      const moneyAmount = item.moneyPunish || 0;
      
      if (item.statusPunish >= 1 && item.statusPunish <= 5 && punishType === 0) {
        punishType = item.statusPunish; 
      }

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
        record.unlockTSGmailPunish = (record.unlockTSGmailPunish || 0) + moneyAmount;
      } else if (punishType === 17) {
        record.unlockTSIMSPunish = (record.unlockTSIMSPunish || 0) + moneyAmount;
      } else if (punishType === 18) {
        record.unlockTSPMPunish = (record.unlockTSPMPunish || 0) + moneyAmount;
      } else if (punishType === 19) {
        record.unlockTSStaffPunish = (record.unlockTSStaffPunish || 0) + moneyAmount;
      }

      record.totalDayPunishment = (
        (record.attendancePunish || 0) + 
        (record.dailyPunish || 0) + 
        (record.mentionPunish || 0) + 
        (record.trackerPunish || 0) + 
        (record.reviewInternPunish || 0) + 
        (record.pmReportPunish || 0) + 
        (record.antPunish || 0) + 
        (record.unlockTSGmailPunish || 0) + 
        (record.unlockTSIMSPunish || 0) +
        (record.unlockTSStaffPunish || 0) +
        (record.unlockTSPMPunish || 0)
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
    this.loadUserBalance();
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

  console.log('=== OPENING DIALOG ===');
  console.log('Item date:', item.date);
  console.log('Day of month:', dayOfMonth);
  console.log('Filtered user punishments:', filteredUserPunishments);

  filteredUserPunishments.forEach(up => {
    if (up.userPunishmentType > 0) {
      const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === up.userPunishmentType);
      if (punishmentType && !userPunishmentTypes.some(p => p.value === punishmentType.value)) {
        userPunishmentTypes.push(punishmentType);
      }
    }
  });

  console.log('User punishment types:', userPunishmentTypes);
  console.log('Structured user notes:', item.structuredUserNotes);

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

    if (result) {
      const complaintsToDelete: number[] = (result.complaintsToDelete || []).filter((id: any) => id !== null && id !== undefined && id !== 0);
      const complaintsToAdd = (result.complaints || []).filter((c: any) => c && c.userPunishmentId);


      const promises: Promise<any>[] = [];
      complaintsToDelete.forEach((userPunishmentId: number) => {
        promises.push(
          this.timekeepingService.deleteComplain(userPunishmentId).toPromise()
            .then(res => {
              return res;
            })
            .catch(err => {
              console.error('Delete error for ID:', userPunishmentId, err);
              throw err;
            })
        );
      });
      complaintsToAdd.forEach((complaint: any) => {
        promises.push(
          this.timekeepingService.addComplain({
            userPunishmentId: complaint.userPunishmentId,
            userNote: complaint.userNote
          }).toPromise()
            .then(res => {
              return res;
            })
            .catch(err => {
              console.error('Add/Update error:', err);
              throw err;
            })
        );
      });


      if (promises.length > 0) {
        Promise.all(promises)
          .then(() => {
            this.notify.success('Complain updated successfully.');
            this.getData();
            this.loadUserBalance();
          })
          .catch((error: any) => {
            console.error('❌ Error in operations:', error);
            console.error('Error response:', error && error.error);
            console.error('Error status:', error && error.status);

            let msg = 'Lỗi không xác định';
            try {
              if (error && error.error && error.error.message) {
                msg = error.error.message;
              } else if (error && error.message) {
                msg = error.message;
              } else {
                msg = JSON.stringify(error);
              }
            } catch (e) {
              msg = String(error);
            }

            this.notify.error('Không thể cập nhật khiếu nại: ' + msg);
          });
      } else {
      }
    } else {
    }
  });
}
  
  toggleNoteReplies(item: TimekeepingDto) {
    item.showAllReplies = !item.showAllReplies;
  }
  
  toggleUserNotes(item: TimekeepingDto) {
    item.showAllComplaints = !item.showAllComplaints;
  }

  hasMultipleLines(item: any, type: 'complaint' | 'reply'): boolean {
    const items = type === 'complaint' 
      ? (item.structuredUserNotes || []) 
      : (item.structuredNoteReplies || []);
    
    if (items.length === 0) {
      return false;
    }

    if (items.length >= 2) {
      return true;
    }
    
    const note = items[0];
    const text = type === 'complaint' 
      ? (note.userNote || '') 
      : (note.noteReply || '');
    
    const totalLength = (note.punishmentName + ': ' + text).length;
    return totalLength > 30 || text.includes('\n');
  }

  getAttendancePunishmentTypes(item: TimekeepingDto): string {
    if (item.structuredNoteReplies && item.structuredNoteReplies.length > 0) {
      const attendanceReplies = item.structuredNoteReplies.filter(reply => 
        reply.punishmentType >= 1 && reply.punishmentType <= 5
      );
      
      if (attendanceReplies.length > 0) {
        return attendanceReplies.map(reply => reply.punishmentName).join(', ');
      }
    }

    let punishType = item.userPunishmentType || item.statusPunish;
    
    if (!punishType && item.statusPunish) {
      punishType = item.statusPunish;
    }

    if (punishType) {
      const punishmentType = this.APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === punishType);
      if (punishmentType) {
        return punishmentType.name;
      }
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
        this.notify.error('Failed to load punishment details');
      }
    );
  }

  openConfirmationDialog() {
    if (this.isConfirmLoading) {
      return;
    }

    this.isConfirmLoading = true;

    this.userPunishmentPaidService
      .getTotalRemainPointsUsedInMonth(this.year, this.month + 1)
      .toPromise()
      .then((totalUsedRemainPoints) => {
        const dialogRef = this.dialog.open(TimesheetConfirmationDialogComponent, {
          width: '800px',
          data: {
            timekeepingData: this.listTimekeeping,
            totalMonthlyPunishment: this.totalMonthlyPunishment,
            selectedDate: new Date(this.year, this.month, 1),
            totalUsedRemainPoints: totalUsedRemainPoints,
            onPaidSuccess: () => {
              this.notify.success('Paid Successfully');
              this.getData();
              this.loadUserBalance();
            }
          }
        });

        dialogRef.componentInstance.remainPointsUsed.subscribe(() => {
          this.userPunishmentPaidService
            .getTotalRemainPointsUsedInMonth(this.year, this.month + 1)
            .subscribe((updatedTotalUsed) => {
              dialogRef.componentInstance.totalUsedRemainPoints = updatedTotalUsed;
            }, (error) => {
              console.error('Error reloading total used RemainPoints:', error);
            });

          this.loadUserBalance();
        });

        dialogRef.afterClosed().subscribe(() => {
          this.isConfirmLoading = false;
        });
      })
      .catch((error) => {
        console.error('Error getting total used RemainPoints:', error);
        const dialogRef = this.dialog.open(TimesheetConfirmationDialogComponent, {
          width: '800px',
          data: {
            timekeepingData: this.listTimekeeping,
            totalMonthlyPunishment: this.totalMonthlyPunishment,
            selectedDate: new Date(this.year, this.month, 1),
            totalUsedRemainPoints: 0,
            onPaidSuccess: () => {
              this.notify.success('Paid Successfully');
              this.getData();
              this.loadUserBalance();
            }
          }
        });

        dialogRef.componentInstance.remainPointsUsed.subscribe(() => {
          this.userPunishmentPaidService
            .getTotalRemainPointsUsedInMonth(this.year, this.month + 1)
            .subscribe((updatedTotalUsed) => {
              dialogRef.componentInstance.totalUsedRemainPoints = updatedTotalUsed;
            }, (error) => {
              console.error('Error reloading total used RemainPoints:', error);
            });
          this.loadUserBalance();
        });

        dialogRef.afterClosed().subscribe(() => {
          this.isConfirmLoading = false;
        });
      });
  }
}