import { Component, OnInit, Inject, Injector, Output, EventEmitter } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatSnackBar } from '@angular/material';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { UserPunishmentPaidService, UserPunishmentPaidDto, GetUserPunishmentBalanceDto, MarkPaidTransactionResultDto } from '@app/service/api/user-punishment-paid.service';
import { TransactionHashDialogComponent } from '../transaction-hash-dialog/transaction-hash-dialog.component';
import { CalendarEvent, CalendarView } from 'angular-calendar';
import { Subject } from 'rxjs';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { ConfigurationService } from '@app/service/api/configuration.service';
import { AppComponentBase } from '@shared/app-component-base';
import { TranferDialogComponent } from '../tranfer-from-timesheet-dialog/tranfer-from-timesheet-dialog.component';
import { TransactionSuccessDialogComponent } from '../transaction-success-dialog/transaction-success-dialog.component';
import { STORAGE_KEYS } from '@app/constant/storage-keys.constant';

@Component({
  selector: 'app-timesheet-confirmation-dialog',
  templateUrl: './timesheet-confirmation-dialog.component.html',
  styleUrls: ['./timesheet-confirmation-dialog.component.css']
})
export class TimesheetConfirmationDialogComponent extends AppComponentBase implements OnInit {
  @Output() remainPointsUsed = new EventEmitter<void>();
  totalErrors: number = 0;
  totalFine: number = 0;
  totalRemainPointsUsedInMonth: number = 0;
  owedAmount: number = 0;
  weekNumber: number;
  weekRange: string;
  indexerUrl: string = '';
  donationUrl: string = '';
  donationWallet: string = '';
  punishmentItems: any[] = [];
  punishmentPaidItems: UserPunishmentPaidDto[] = [];
  userBalance: GetUserPunishmentBalanceDto | null = null;
  isLoadingPaidData: boolean = false;
  totalPaidPunishmentInMonth: number = 0;
  isLoadingCalendarData: boolean = false;
  selectedFund: string = 'Build School Fund';
  contributeToFund: boolean = false;
  totalUsedRemainPoints: number = 0;
  isCurrentMonth: boolean = true;

  view: CalendarView = CalendarView.Month;
  calendarView = CalendarView;
  viewDate: Date = new Date();
  events: CalendarEvent[] = [];
  activeDayIsOpen: boolean = false;
  refresh: Subject<any> = new Subject();
  timesheetData: any[] = [];
  usePointsTooltip: string = '';
  zkProofAvailable: boolean = false;

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
    
    try {
    const zkProof = localStorage.getItem(STORAGE_KEYS.ZK_PROOF);
    this.zkProofAvailable = !!(zkProof && zkProof !== 'undefined');
    } catch (e) {
    this.zkProofAvailable = false;
    }

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
      const selectedDate = this.data.selectedDate ? moment(this.data.selectedDate) : moment();
      const monthStart = selectedDate.clone().startOf('month');
      const monthEnd = selectedDate.clone().endOf('month');
      
      this.weekNumber = -1; 
      this.weekRange = `${monthStart.format('MMM DD')}-${monthEnd.format('DD, YYYY')}`;

      this.viewDate = new Date(selectedDate.year(), selectedDate.month(), 1);
    }
    
    this.loadPunishmentPaidData();
    this.loadTimesheetData();

    this.totalUsedRemainPoints = this.data.totalUsedRemainPoints || 0;

    const target = this.getTargetYearMonth();
    const now = moment();
    this.isCurrentMonth = (target.year === now.year() && target.month === (now.month() + 1));

    if (this.data.summaryData) {
      this.userBalance = this.data.summaryData.userBalance;
      this.totalFine = this.userBalance ? this.userBalance.totalPunishmentMoney : 0;
      this.totalPaidPunishmentInMonth = this.data.summaryData.totalPaidPunishmentInMonth;
      this.totalRemainPointsUsedInMonth = this.data.summaryData.totalRemainPointsUsedInMonth || 0;
      this.updateOwedAmount();
      this.updateUsePointsTooltip();
    } else {
      this.loadAllPunishmentData();
    }
  }

  markAsPaid(): void {
    const dialogRef = this.dialog.open(TransactionHashDialogComponent, {
      width: '500px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(transactionHash => {
      if (!transactionHash) return;
      
      this.snackBar.open('Processing transaction...', '', { duration: 2000 });
      
      const { year, month } = this.getTargetYearMonth();
    
      this.userPunishmentPaidService.markPaidTransaction(transactionHash, year, month).subscribe(
      result => {
        if (result && result.success) {
            this.snackBar.open('Transaction processed successfully!', 'Close', { duration: 5000, panelClass: ['snackbar-success'] });

          this.loadPunishmentPaidData();
          if (this.data && typeof this.data.onPaidSuccess === 'function') {
            this.data.onPaidSuccess();
          }
        } else {
            this.snackBar.open(result.message || 'Failed to process transaction', 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
        }
      },
      error => {
          console.error('Error marking transaction as paid:', error);
          this.snackBar.open('Error processing transaction. Please try again.', 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
      }
    );
    });
  }

  paidFinedFromTimesheet(): void {
    const minAmount = this.owedAmount || 0;
    const dialogRef = this.dialog.open(TranferDialogComponent, {
      width: '500px',
      disableClose: true,
      data: {
        minAmount: minAmount,
        donationWallet: this.donationWallet,
        remainingAmount: this.owedAmount || 0,
      }
    });

    dialogRef.afterClosed().subscribe(transferResult => {
      if (!transferResult || !transferResult.txhash) return;

      const transactionHash = transferResult.txhash;
      const amountPaid = transferResult.amount
      
      this.snackBar.open('Processing transaction...', '', { duration: 2000 });
      
      const { year, month } = this.getTargetYearMonth();
    
      setTimeout(() => {
        this.userPunishmentPaidService.markPaidTransaction(transactionHash, year, month).subscribe(
          result => {
            if (result && result.success) {
              this.snackBar.open('Transaction processed successfully!', 'Close', { duration: 5000 });
              this.openTransactionSuccessDialog(result, transactionHash, amountPaid);
              
              this.loadPunishmentPaidData();
              this.loadAllPunishmentData();
              if (this.data && typeof this.data.onPaidSuccess === 'function') {
                this.data.onPaidSuccess();
              }
            } else {
              this.snackBar.open(result.message || 'Failed to process transaction', 'Close', { duration: 5000 });
            }
          },
          error => {
              console.error('Error marking transaction as paid:', error);
              this.snackBar.open('Error processing transaction. Please try again.', 'Close', { duration: 5000 });
          }
        );
      }, 2000);
    });
  }

  private openTransactionSuccessDialog(result: MarkPaidTransactionResultDto, fallbackHash: string, fallbackAmount: number): void {
    const dialogData = {
      transactionHash: result.transactionHash || fallbackHash,
      amountPaid: fallbackAmount,
    };

    this.dialog.open(TransactionSuccessDialogComponent, {
      width: '420px',
      data: dialogData
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
          if (data.result.donationWallet) {
          this.donationWallet = data.result.donationWallet;
          }
        }
      },
      () => {
        this.snackBar.open('Error loading configuration. Please try again later.', 'Close', { duration: 5000 });
      }
    );
  }

  public updateUsePointsTooltip(): void {
    if (!this.userBalance) {
      this.usePointsTooltip = '';
      return;
    }

    if (this.userBalance.totalPunishmentMoney <= 0) {
      this.usePointsTooltip = 'No punishment amount to pay.';
    } else if (this.userBalance.remainPoints < this.userBalance.totalPunishmentMoney) {
      this.usePointsTooltip = 'Your current points are not enough to cover all punishment.';
    } else {
      this.usePointsTooltip = 'You can use points to pay all remaining punishments';
    }
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
    this.isLoadingPaidData = true;
    
    const { year, month } = this.getTargetYearMonth();

    try {
      this.userPunishmentPaidService.getForCurrentUser(year, month).subscribe(
        (result) => {
          this.punishmentPaidItems = result || [];
          
          this.isLoadingPaidData = false;
        },
        (error) => {
          console.error(`Error loading punishment paid data for ${month}/${year}:`, error);
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
    return this.totalPaidPunishmentInMonth;
  }

  private getTargetYearMonth(): { year: number, month: number } {
    if (this.data.selectedDate) {
      const selectedDate = moment(this.data.selectedDate);
      return { year: selectedDate.year(), month: selectedDate.month() + 1 };
    } 
    else if (this.punishmentItems.length > 0) {
      const firstDate = moment(this.punishmentItems[0].date);
      return { year: firstDate.year(), month: firstDate.month() + 1 };
    }
    else {
      const fallbackDate = moment();
      return { year: fallbackDate.year(), month: fallbackDate.month() + 1 };
    }
  }

  loadAllPunishmentData(): void {
    const { year, month } = this.getTargetYearMonth();
    
    this.userPunishmentPaidService.previewApplyAndGetSummary(year, month).subscribe(
      (result) => {
        if (result && result.success) {
          this.userBalance = result.userBalance;
          this.totalFine = this.userBalance ? this.userBalance.totalPunishmentMoney : 0;
          this.totalPaidPunishmentInMonth = result.totalPaidPunishmentInMonth;
          this.totalRemainPointsUsedInMonth = result.totalRemainPointsUsedInMonth || 0;
          this.updateOwedAmount();
          this.totalUsedRemainPoints = result.totalRemainPointsUsedInMonth || 0;
          this.updateUsePointsTooltip();
        }
      },
      (error) => {
        console.error('Error loading punishment summary:', error);
        this.userBalance = null;
        this.totalPaidPunishmentInMonth = 0;
      }
    );
  }

  confirmApplyRemainPoints(): void {
  abp.message.confirm(
    'Do you want to use your points to pay all punishments?',
    (result: boolean) => {
      if (result) {
        this.applyRemainPoints();
      }
    } 
  );
}

  applyRemainPoints(): void {
    const target = this.getTargetYearMonth();

    this.userPunishmentPaidService.applyRemainPoints(target.year, target.month).subscribe(
      (result) => {
        if (result && result.success) {
          this.snackBar.open('Applied remain points successfully.', 'Close', { duration: 5000, panelClass: ['snackbar-success'] });

          if (this.remainPointsUsed) {
            this.remainPointsUsed.emit();
          }
        } else {
          this.snackBar.open(result && result.message ? result.message : 'Failed to apply remain points.', 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
        }
      },
      (error) => {
        console.error('Error applying remain points:', error);
        this.snackBar.open('Error applying remain points. Please try again.', 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
      }
    );
  }

  public updateOwedAmount(): void {
    const totalPunishment = this.userBalance ? this.userBalance.totalPunishmentMoney : 0;
    const remainPoints = this.userBalance ? this.userBalance.remainPoints : 0;
    
    if (totalPunishment > remainPoints) {
      this.owedAmount = totalPunishment - remainPoints;
    } else {
      this.owedAmount = totalPunishment;
    }
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
