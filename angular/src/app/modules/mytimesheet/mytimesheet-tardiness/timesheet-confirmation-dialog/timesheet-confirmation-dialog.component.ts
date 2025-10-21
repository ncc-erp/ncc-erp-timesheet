import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog, MatSnackBar } from '@angular/material';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { UserPunishmentPaidService, UserPunishmentPaidDto } from '@app/service/api/user-punishment-paid.service';
import { TransactionHashDialogComponent } from '../transaction-hash-dialog/transaction-hash-dialog.component';

@Component({
  selector: 'app-timesheet-confirmation-dialog',
  templateUrl: './timesheet-confirmation-dialog.component.html',
  styleUrls: ['./timesheet-confirmation-dialog.component.css']
})
export class TimesheetConfirmationDialogComponent implements OnInit {
  totalErrors: number = 0;
  totalFine: number = 0;
  isPaid: boolean = false;
  weekNumber: number;
  weekRange: string;
  punishmentItems: any[] = [];
  punishmentPaidItems: UserPunishmentPaidDto[] = [];
  selectedFund: string = 'Build School Fund';
  contributeToFund: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<TimesheetConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userPunishmentPaidService: UserPunishmentPaidService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit() {
    console.log('Dialog data:', this.data);
    
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
    } else {
      const currentDate = moment();
      const monthStart = currentDate.clone().startOf('month');
      const monthEnd = currentDate.clone().endOf('month');
      
      this.weekNumber = -1; 
      this.weekRange = `${monthStart.format('MMM DD')}-${monthEnd.format('DD, YYYY')}`;
    }
    
    this.loadPunishmentPaidData();
  }

  onClose(): void {
    this.dialogRef.close();
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
          month = firstDate.month() + 1; // moment months are 0-11, API expects 1-12
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
        }
      );
    } catch (e) {
      console.error('Exception in loadPunishmentPaidData:', e);
      this.punishmentPaidItems = [];
    }
  }
  
  calculateTotalPaidAmount(): number {
    if (!this.punishmentPaidItems || this.punishmentPaidItems.length === 0) {
      return 0;
    }
    
    return this.punishmentPaidItems.reduce((total, item) => total + (item.amount || 0), 0);
  }
}
