import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-reject-timesheet-dialog',
  templateUrl: './reject-timesheet-dialog.component.html',
  styleUrls: ['./reject-timesheet-dialog.component.css']
})
export class RejectTimesheetDialogComponent implements OnInit {
  reason: string = '';

  constructor(
    public dialogRef: MatDialogRef<RejectTimesheetDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit() {
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    this.dialogRef.close(this.reason);
  }
}
