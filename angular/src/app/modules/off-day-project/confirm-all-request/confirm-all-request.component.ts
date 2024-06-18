import { Component, Inject, Injector, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';

@Component({
  selector: 'app-confirm-all-request',
  templateUrl: './confirm-all-request.component.html',
  styleUrls: ['./confirm-all-request.component.css']
})
export class ConfirmAllRequestComponent extends AppComponentBase implements OnInit {
  APPROVAL_ABSENCE_DAY_PROJECT = PERMISSIONS_CONSTANT.ApprovalAbsenceDayByProject;
  RELEASE_USER_FROM_PROJECT = PERMISSIONS_CONSTANT.ReleaseUser;

  title = "";
  events: Map<string, any>;
  date = null;
  number : any;
  isLoading: boolean;

  constructor(
    injector: Injector,
    private absenceRequestService: AbsenceRequestService,
    public dialogRef: MatDialogRef<ConfirmAllRequestComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    super(injector);
    if (data) {
      this.events = data.events;
      this.date = data.date;
      this.number = data.number;
    }
  }

  confirmClick() {
    const arrays = Array.from(this.events.values());
    if (this.number == 1) {
      this.isLoading = true;
      this.absenceRequestService.approveAbsenceRequest([].concat(...arrays)).subscribe((res) => {
        if (res) {
          this.notify.success(this.l("Approve Successfully!"));
        }
        this.isLoading = false;
      }, (error) => {
        this.isLoading = false;
      });
    } else {
      this.isLoading = true;
      this.absenceRequestService.rejectAbsenceRequest([].concat(...arrays)).subscribe((res) => {
        if (res) {
          this.notify.success(this.l("Reject Successfully!"));
        }
        this.isLoading = false;
      }, (error) => {
        this.isLoading = false;
      });
    }
  }

  ngOnInit() {
    this.date = moment(this.date).format('DD-MM-YYYY');
  }

  close(): void {
    this.dialogRef.close();
  }
}
