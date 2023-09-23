import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { RequestOfUserDto } from '@app/service/api/model/absence-day-dto';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';
import { AbsenceDayComponent } from '../absence-day.component';

@Component({
  selector: 'app-detail-request',
  templateUrl: './detail-request.component.html',
  styleUrls: ['./detail-request.component.css']
})
export class DetailRequestComponent extends AppComponentBase implements OnInit {
  APPROVAL_ABSENCE_DAY_PROJECT = PERMISSIONS_CONSTANT.ApprovalAbsenceDayByProject;

  title = "";
  listRequestOfUser: RequestOfUserDto[] = [];
  date = null;
  public isLoading: boolean = false;

  constructor(
    injector: Injector,
    private absenceRequestService: AbsenceRequestService,
    private _dialogRef: MatDialogRef<AbsenceDayComponent>,

    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    super(injector);

  }

  ngOnInit() {
    this.getAllRequestOfUserByDate();
  }

  getAllRequestOfUserByDate() {
    this.date = moment(this.data).format('YYYY-MM-DD');
    this.absenceRequestService.getAllRequestOfUserByDate(this.date).subscribe(res => {
      this.listRequestOfUser = res.result;
    });

  }
  getListTypeClasses(member: RequestOfUserDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay
        || member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return ['text-primary', 'day-chip-tardiness-leave-early'];
      }
      return ['text-primary', 'day-chip-full-day'];
    } else if (member.leavedayType == 1) {
      return ['text-danger', 'onsite'];
    }
    return ['text-primary', 'day-chip-morning'];
  }

  getListClasses(member: RequestOfUserDto) {
    if (member.dateType == 1) {
      return ['text-primary', 'day-chip-full-day'];
    }
    if (member.dateType == 2) {
      return ['text-primary', 'day-chip-morning'];
    }
    if (member.dateType == 3) {
      return ['text-primary', 'day-chip-afternoon'];
    }
    return ['text-primary', 'day-chip-custom'];
  }

  getLeaveTypeText(member: RequestOfUserDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay) {
        return "Đi muộn";
      }
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return "Về sớm";
      }
      return "Off";
    } else if (member.leavedayType == 1) {
      return "Onsite";
    } else if (member.leavedayType == 2) {
      return "Remote";
    }
  }

  getLeaveText(member: RequestOfUserDto) {
    if (member.dateType == 1) {
      return "Full Day";
    }
    if (member.dateType == 2) {
      return "Morning";
    }
    if (member.dateType == 3) {
      return "Afternoon";
    }
    return member.hour + "h";
  }



  cancelAbsenceRequest(requestId) {
    this.isLoading = true;
    this.absenceRequestService.cancelAbsenceRequest(requestId).subscribe(res => {
      if (res) {
        this.isLoading = false;
        this.notify.success(this.l("Cancel Successfully!"));
        this.getAllRequestOfUserByDate();
        this.close(1);
      }
    });
  }

  close(res): void {
    this._dialogRef.close(res);
  }

}
