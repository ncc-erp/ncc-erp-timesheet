import { Component, OnInit, Inject, Injector } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog, MatSnackBar } from '@angular/material';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { UserPunishmentPaidService, UserPunishmentPaidDto } from '@app/service/api/user-punishment-paid.service';
import { TransactionHashDialogComponent } from '../transaction-hash-dialog/transaction-hash-dialog.component';
import { CalendarEvent, CalendarView } from 'angular-calendar';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import { Subject } from 'rxjs';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { AppComponentBase } from '@shared/app-component-base';

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-timesheet-confirmation-dialog',
  templateUrl: './timesheet-confirmation-dialog.component.html',
  styleUrls: ['./timesheet-confirmation-dialog.component.css'],
  providers: [
    { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})
export class TimesheetConfirmationDialogComponent extends AppComponentBase implements OnInit {
  totalErrors: number = 0;
  totalFine: number = 0;
  isPaid: boolean = false;
  weekNumber: number;
  weekRange: string;
  punishmentItems: any[] = [];
  punishmentPaidItems: UserPunishmentPaidDto[] = [];
  isLoadingPaidData: boolean = false;
  isLoadingCalendarData: boolean = false;
  selectedFund: string = 'Build School Fund';
  contributeToFund: boolean = false;

  view: CalendarView = CalendarView.Month;
  calendarView = CalendarView;
  viewDate: Date = new Date();
  events: CalendarEvent[] = [];
  activeDayIsOpen: boolean = false;
  refresh: Subject<any> = new Subject();
  timesheetData: any[] = [];

  constructor(
    injector: Injector,
    public dialogRef: MatDialogRef<TimesheetConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userPunishmentPaidService: UserPunishmentPaidService,
    private mytimesheetService: MyTimesheetService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { 
    super(injector);
  }

  ngOnInit() {    
    const punishmentItemsRaw = this.data.timekeepingData.filter(item => {
      const hasPunishment = item.moneyPunish > 0;
      return hasPunishment;
    });

    
    const groupedByDateAndType = {};
    
    punishmentItemsRaw.forEach(item => {
      const dateObj = new Date(item.date);
      const dateKey = `${dateObj.getFullYear()}-${dateObj.getMonth() + 1}-${dateObj.getDate()}`;
      const typeKey = item.userPunishmentType || item.statusPunish || 0;
      
      const key = `${dateKey}_${typeKey}`;
      
      if (!groupedByDateAndType[key]) {
        groupedByDateAndType[key] = {
          ...item,
          count: 1
        };
      } else {
        groupedByDateAndType[key].moneyPunish += item.moneyPunish;
        groupedByDateAndType[key].count += 1;
        
        if (typeKey !== 0) {
          const punishmentType = this.getPunishmentTypeName(typeKey);
          groupedByDateAndType[key].noteReply = `${punishmentType} (${groupedByDateAndType[key].count} lần)`;
        }
      }
    });
    
    this.punishmentItems = Object.keys(groupedByDateAndType).map(key => groupedByDateAndType[key]);
        
    this.totalErrors = this.punishmentItems.length;
    
    if (this.punishmentItems.length > 0) {
      const firstDate = moment(this.punishmentItems[0].date);
      const month = firstDate.month();
      const year = firstDate.year();
      
      const monthStart = moment().year(year).month(month).startOf('month');
      const monthEnd = moment().year(year).month(month).endOf('month');
      
      this.weekNumber = -1; 
      this.weekRange = `${monthStart.format('MMM DD')}-${monthEnd.format('DD, YYYY')}`;

      this.viewDate = new Date(firstDate.year(), firstDate.month(), 1);
    } else {
      const currentDate = moment();
      const monthStart = currentDate.clone().startOf('month');
      const monthEnd = currentDate.clone().endOf('month');
      
      this.weekNumber = -1; 
      this.weekRange = `${monthStart.format('MMM DD')}-${monthEnd.format('DD, YYYY')}`;

      this.viewDate = new Date(currentDate.year(), currentDate.month(), 1);
    }
    
    this.loadPunishmentPaidData();
    this.loadTimesheetData();
  }


  markAsPaid(): void {
    const dialogRef = this.dialog.open(TransactionHashDialogComponent, {
      width: '500px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(transactionHash => {
      if (transactionHash) {
        this.snackBar.open('Processing transaction...', '', { duration: 2000 });
        
        let year: number;
        let month: number;
        
        if (this.punishmentItems.length > 0) {
          const firstDate = moment(this.punishmentItems[0].date);
          month = firstDate.month() + 1; 
          year = firstDate.year();
        } else {
          const currentDate = moment();
          month = currentDate.month() + 1; 
          year = currentDate.year();
        }
        
        this.userPunishmentPaidService.markPaidTransaction(transactionHash, year, month).subscribe(
          result => {
            if (result && result.success) {
              this.isPaid = true;
              this.snackBar.open('Transaction processed successfully!', 'Close', { duration: 5000 });
              
              this.loadPunishmentPaidData();
            } else {
              this.snackBar.open(result.message || 'Failed to process transaction', 'Close', { duration: 5000 });
            }
          },
          error => {
            console.error('Error marking transaction as paid:', error);
            this.snackBar.open('Error processing transaction. Please try again.', 'Close', { duration: 5000 });
          }
        );
      }
    });
  }

  donate(): void {
    if (this.contributeToFund) {
      window.open('https://dev-mmn.nccsoft.vn/uiux/v3/donation-campaign/detail.html', '_blank');
    }
  }
  
  getPunishmentTypeName(typeId: number): string {
    const punishmentType = APP_CONSTANT.PUNISHMENT_TYPES.find(p => p.value === typeId);
    return punishmentType ? punishmentType.name : 'Unknown';
  }

  loadPunishmentPaidData(): void {
    try {
      this.isLoadingPaidData = true;
      
      let year: number;
      let month: number;
      
      if (this.punishmentItems.length > 0) {
        const firstDate = moment(this.punishmentItems[0].date);
        month = firstDate.month() + 1; 
        year = firstDate.year();
      } else {
        const currentDate = moment();
        month = currentDate.month() + 1; 
        year = currentDate.year();
      }
      
      this.userPunishmentPaidService.getForCurrentUser(year, month).subscribe(
        (result) => {
          this.punishmentPaidItems = result || [];
          
          const totalPunishmentAmount = this.data.totalMonthlyPunishment || 0;
          const totalPaidAmount = this.calculateTotalPaidAmount();
          
          this.totalFine = Math.max(0, totalPunishmentAmount - totalPaidAmount);
          
          this.isPaid = totalPaidAmount >= totalPunishmentAmount;
          this.isLoadingPaidData = false;
        },
        (error) => {
          console.error(`Error loading punishment paid data for ${month}/${year}:`, error);
          if (error.status) {
            console.error('HTTP Status:', error.status);
          }
          if (error.message) {
            console.error('Error message:', error.message);
          }
          if (error.error) {
            console.error('Error details:', error.error);
          }
          this.punishmentPaidItems = [];
          this.isLoadingPaidData = false;
        }
      );
    } catch (e) {
      console.error('Exception in loadPunishmentPaidData:', e);
      this.punishmentPaidItems = [];
      this.isLoadingPaidData = false;
    }
  }
  
  calculateTotalPaidAmount(): number {
    if (!this.punishmentPaidItems || this.punishmentPaidItems.length === 0) {
      return 0;
    }
    
    return this.punishmentPaidItems.reduce((total, item) => total + (item.amount || 0), 0);
  }
  
  loadTimesheetData(): void {
    try {
      this.isLoadingCalendarData = true;
      const currentDate = moment(this.viewDate);
      const startDate = currentDate.clone().startOf('month').format('YYYY-MM-DD');
      const endDate = currentDate.clone().endOf('month').format('YYYY-MM-DD');
            
      this.mytimesheetService.getAllTimeSheet(startDate, endDate).subscribe(result => {
        if (result && result.result) {
          this.timesheetData = result.result;
          this.generateCalendarEvents();
        } else {
          console.warn('API returned no result data');
          this.timesheetData = [];
          this.refresh.next();
          this.isLoadingCalendarData = false;
        }
      }, error => {
        console.error('Error loading timesheet data:', error);
        this.timesheetData = [];
        this.refresh.next();
        this.isLoadingCalendarData = false;
      });
    } catch (error) {
      console.error('Exception in loadTimesheetData:', error);
      this.timesheetData = [];
      this.refresh.next();
      this.isLoadingCalendarData = false;
    }
  }
  
  generateCalendarEvents(): void {
    try {
      this.events = [];
      
      if (!this.timesheetData || this.timesheetData.length === 0) {
        this.refresh.next();
        this.isLoadingCalendarData = false;
        return;
      }

      const groupedByDate = {};
      
      this.timesheetData.forEach(timesheet => {
        if (!timesheet || !timesheet.dateAt) return;
        
        const dateStr = moment(timesheet.dateAt).format('YYYY-MM-DD');
        
        if (!groupedByDate[dateStr]) {
          groupedByDate[dateStr] = [];
        }
        
        groupedByDate[dateStr].push(timesheet);
      });

      Object.keys(groupedByDate).forEach(dateStr => {
        const timesheets = groupedByDate[dateStr];
        const date = moment(dateStr).toDate();

        const totalWorkingTime = timesheets.reduce((total, ts) => {
          return total + (ts.workingTime || 0);
        }, 0);

        const rawHours = totalWorkingTime / 60;
        const totalWorkingHours = Number.isInteger(rawHours) ? Math.floor(rawHours).toString() : rawHours.toFixed(1);

        this.events.push({
          start: date,
          end: date,
          title: `${timesheets.length} timesheet(s)`,
          meta: {
            timesheets: timesheets,
            totalWorkingTime: totalWorkingTime,
            totalWorkingHours: totalWorkingHours
          }
        });
      });
      
      this.refresh.next();
      this.isLoadingCalendarData = false;
    } catch (error) {
      console.error('Error generating calendar events:', error);
      this.isLoadingCalendarData = false;
    }
  }

  getDayCellClass(day: any): string {
    if (!day || !day.events || day.events.length === 0) return '';
    const ev = day.events[0];
    const timesheets = ev.meta && ev.meta.timesheets ? ev.meta.timesheets : [];
    if (!timesheets.length) return '';
    const status = timesheets[0].status;
    if (status === 1) return 'cell-pending';
    if (status === 2) return 'cell-approved';
    return 'cell-rejected';
  }
}
