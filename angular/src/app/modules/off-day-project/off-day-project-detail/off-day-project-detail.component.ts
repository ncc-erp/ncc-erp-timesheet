import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { AbsenceRequestDto } from '@app/service/api/model/absence.dto';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import * as moment from 'moment';

@Component({
  selector: 'app-off-day-project-detail',
  templateUrl: './off-day-project-detail.component.html',
  styleUrls: ['./off-day-project-detail.component.css']
})
export class OffDayProjectDetailComponent extends AppComponentBase implements OnInit {
  APPROVAL_ABSENCE_DAY_PROJECT = PERMISSIONS_CONSTANT.ApprovalAbsenceDayByProject;

  title = "";
  events: AbsenceRequestDto[] = [];
  date = null;

  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];
 public isLoading: boolean = false;

  constructor(
    injector: Injector,
    private absenceRequest: AbsenceRequestService,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    super(injector);
    if (data) {
      this.events = data.events;
      this.date = data.date;
    }
  }

  ngOnInit() {
    //this.title = "Detail absences of project " + `${this.project.name} in ${moment(this.date).format('DD-MM-YYYY')}`;
    this.date = moment(this.date).format('DD-MM-YYYY');
  }

  getListTypeClasses(member: AbsenceRequestDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay
        || member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return ['text-primary', 'day-chip-tardiness-leave-early'];
      }
      return ['text-primary', 'day-chip-full-day'];
    }  else if (member.leavedayType == 1) {
      return ['text-danger', 'onsite'];
    }
    return ['text-primary', 'day-chip-morning'];
  }

  getListClasses(member: AbsenceRequestDto) {
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

  getLeaveTypeText(member: AbsenceRequestDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay) {
        return "Đi muộn";
      }
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return "Về sớm";
      }
      return "Off";
    } else if(member.leavedayType == 1) {
      return "Onsite";
    } else if(member.leavedayType == 2) {
      return "Remote";
    }
  }

  getLeaveText(member: AbsenceRequestDto) {
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

  getSexText(member: AbsenceRequestDto) {
    if (member.sex == 0) {
      return "Male";
    }
    return "Female";
  }

  getSexClasses(member: AbsenceRequestDto) {
    if (member.sex == 0) {
      return ['text-primary', 'day-chip-full-day'];
    }
    return ['text-primary', 'day-chip-morning'];
  }

  onApproveAbsence(item) {
    let data = [];
    data.push(item.id);
    this.isLoading = true;
    this.absenceRequest.approveAbsenceRequest(data).subscribe(res => {
      if (res) {
        for (let i = 0; i < this.events.length; i++) {
          if (this.events[i].id === item.id) {
            this.events[i].status = 2
          }
        }
        this.notify.success(this.l("Approve Successfully!"));
        this.isLoading = false;
      }
    });
  }

  onRejectAbsence(item) {
    let data = [];
    data.push(item.id);
    this.isLoading = true;
    this.absenceRequest.rejectAbsenceRequest(data).subscribe(res => {
      if (res) {
        for (let i = 0; i < this.events.length; i++) {
          if (this.events[i].id === item.id) {
            this.events[i].status = 3
          }
        }
        this.notify.success(this.l("Reject Successfully!"));
        this.isLoading = false;
      }
    });
  }

}
