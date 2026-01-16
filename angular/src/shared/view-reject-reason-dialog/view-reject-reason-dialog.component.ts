import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { GetTimeSheetDto } from '@app/service/api/model/common-DTO';
import * as moment from 'moment';

@Component({
  selector: 'app-view-reject-reason-dialog',
  templateUrl: './view-reject-reason-dialog.component.html',
  styleUrls: ['./view-reject-reason-dialog.component.css']
})
export class ViewRejectReasonDialogComponent implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: GetTimeSheetDto,
    public dialogRef: MatDialogRef<ViewRejectReasonDialogComponent>
  ) { }

  ngOnInit() {
  }

  get formattedTime(): string {
    return this.data.lastModificationTime ? moment(this.data.lastModificationTime).format("YYYY-MM-DD HH:mm") : 'N/A';
  }

  get rejectedBy(): string {
    return this.data['lastModifierUserName'] || this.data['lastModifierUser'] || 'Unknown';
  }

  close(): void {
    this.dialogRef.close();
  }
}
