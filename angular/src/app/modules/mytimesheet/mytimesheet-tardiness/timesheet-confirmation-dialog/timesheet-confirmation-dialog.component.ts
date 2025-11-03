import { Component, OnInit, Inject, Injector } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatSnackBar } from '@angular/material';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { UserPunishmentPaidService, UserPunishmentPaidDto } from '@app/service/api/user-punishment-paid.service';
import { TransactionHashDialogComponent } from '../transaction-hash-dialog/transaction-hash-dialog.component';
import { CalendarEvent, CalendarView } from 'angular-calendar';
import { Subject } from 'rxjs';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { ConfigurationService } from '@app/service/api/configuration.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-timesheet-confirmation-dialog',
  templateUrl: './timesheet-confirmation-dialog.component.html',
  styleUrls: ['./timesheet-confirmation-dialog.component.css']
})
export class TimesheetConfirmationDialogComponent extends AppComponentBase implements OnInit {
  totalErrors: number = 0;
  totalFine: number = 0;
  isPaid: boolean = false;
  weekNumber: number;
  weekRange: string;
  indexerUrl: string = '';
  donationUrl: string = '';
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
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userPunishmentPaidService: UserPunishmentPaidService,
    private mytimesheetService: MyTimesheetService,
    private configService: ConfigurationService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { 
    super(injector);
  }

  ngOnInit() {    
    this.loadConfiguration();
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

  private loadConfiguration(): void {
    this.configService.getDonationUrl().subscribe(
      (data) => {
        if (data && data.result) {
          if (data.result.indexerUrl) {
            const url = new URL(data.result.indexerUrl);
            this.indexerUrl = `${url.protocol}//${url.hostname}/`;
          }
          if (data.result.donationUrl) {
            this.donationUrl = data.result.donationUrl;
          }
        }
      },
      () => {
        this.snackBar.open('Error loading configuration. Please try again later.', 'Close', { duration: 5000 });
      }
    );
  }

  donate(): void {
    if (this.contributeToFund && this.donationUrl) {
      window.open(this.donationUrl, '_blank');
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

        this.events.push({
          start: date,
          end: date,
          title: `${timesheets.length} timesheet(s)`,
          meta: {
            timesheets: timesheets
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

  formatWorkingHours(minutes: number): string {
    if (!minutes || minutes <= 0) { return '0'; }
    const hours = minutes / 60;
    return Number.isInteger(hours) ? Math.floor(hours).toString() : hours.toFixed(1);
  }

  getWorkingHoursClass(status: number): string {
    if (status === 1) { return 'wh-pending'; }
    if (status === 2) { return 'wh-approved'; }
    return 'wh-rejected';
  }
  openTransaction(hash: string, event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (hash && this.indexerUrl) {
      const url = `${this.indexerUrl}transactions/${hash}`.replace(/([^:]\/)\/+/g, '$1');
      window.open(url, '_blank');
    }
  }
}
