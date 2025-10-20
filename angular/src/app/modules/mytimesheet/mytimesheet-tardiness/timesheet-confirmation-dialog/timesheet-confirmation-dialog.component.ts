import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import * as moment from 'moment';

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
  selectedFund: string = 'Build School Fund';
  contributeToFund: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<TimesheetConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit() {
    // Filter out items with punishment
    this.punishmentItems = this.data.timekeepingData.filter(item => item.totalDayPunishment > 0);
    
    // Calculate total errors and fine
    this.totalErrors = this.punishmentItems.length;
    this.totalFine = this.data.totalMonthlyPunishment;
    
    // Get current week number and date range
    const currentDate = moment();
    this.weekNumber = currentDate.isoWeek();
    
    // Calculate week start and end dates
    const weekStart = currentDate.clone().startOf('isoWeek');
    const weekEnd = currentDate.clone().endOf('isoWeek');
    this.weekRange = `${weekStart.format('MMM DD')}-${weekEnd.format('DD, YYYY')}`;
  }

  onClose(): void {
    this.dialogRef.close();
  }

  markAsPaid(): void {
    this.isPaid = true;
    // Here you would typically call an API to mark the timesheet as paid
    // For now, we'll just close the dialog
    this.dialogRef.close({ paid: true });
  }

  donate(): void {
    if (this.contributeToFund) {
      // Open donation page or perform donation action
      window.open('https://doing.mission.ai', '_blank');
    }
  }
}
