import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { AbsenceRequestDto } from '@app/service/api/model/absence.dto';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import * as moment from 'moment';

@Component({
  selector: 'app-popup',
  templateUrl: './popup.component.html',
  styleUrls: ['./popup.component.css']
})
export class PopupComponent extends AppComponentBase implements OnInit {
  title = "";
  events: AbsenceRequestDto[] = [];
  date = null;

  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];

  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    super(injector);
    if (data) {
      this.events = data.events;
      this.date = data.date;
    }
  }

  ngOnInit() {
    this.date = moment(this.date).format('DD-MM-YYYY');
  }


  getListTypeClasses(member: AbsenceRequestDto){
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay
        || member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return ['text-primary', 'day-chip-tardiness-leave-early'];
      }
      return ['text-primary', 'day-chip-full-day'];
    }else if(member.leavedayType == 1){
      return ['text-primary', 'onsite'];
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

  getLeaveTypeText(member: AbsenceRequestDto){
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay) {
        return "Đi muộn";
      }
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return "Về sớm";
      }
      return "Off";
    }  else if(member.leavedayType == 1){
      return "Onsite";
    }
    return "Remote";
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

}
