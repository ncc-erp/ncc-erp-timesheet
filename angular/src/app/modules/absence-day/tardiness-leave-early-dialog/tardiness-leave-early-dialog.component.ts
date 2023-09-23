import { Component, Inject, Injector, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AbsenceDayService } from '@app/service/api/absence-day.service';
import { DayOffService } from '@app/service/api/day-off.service';
import { AbsenceDayDto, AbsenceDayRequest } from '@app/service/api/model/absence-day-dto';
import { WorkingTimeDto } from '@app/service/api/model/working-time-dto';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';
import { AbsenceDayComponent } from '../absence-day.component';
@Component({
  selector: 'app-tardiness-leave-early-dialog',
  templateUrl: './tardiness-leave-early-dialog.component.html',
  styleUrls: ['./tardiness-leave-early-dialog.component.css']
})
export class TardinessLeaveEarlyDialogComponent extends AppComponentBase implements OnInit {
  absenceDayReq: AbsenceDayRequest;
  timeControl = new FormControl("", [Validators.required]);
  customTime = new CustomTimeDto;
  workingTime = new WorkingTimeDto();
  saving: boolean = false;
  note = '';
  requestTime: string;
  breakTime: number;
  dateType: number
  selectedDate: string;
  constructor(injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private absenceDayService: AbsenceDayService,
    private diaLogRef: MatDialogRef<AbsenceDayComponent>,
    private dayOffService: DayOffService,

  ) {
    super(injector)
  }
  ngOnInit() {
    this.selectedDate = this.data.selectedDate;
    this.dateType = this.data.dateType;
    this.workingTime = this.data.data;
    this.customTime.hour = null;
    this.customTime.absenceTime =
      this.data.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay ? this.APP_CONSTANT.OnDayType.BeginOfDay :
        this.data.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay ? this.APP_CONSTANT.OnDayType.EndOfDay : this.APP_CONSTANT.OnDayType.BeginOfDay;
    this.workingTime.morningStartAt = moment(this.workingTime.morningStartAt, 'HH:mm').format('HH:mm');
    this.workingTime.morningEndAt = moment(this.workingTime.morningEndAt, 'HH:mm').format('HH:mm');
    this.workingTime.morningWorking = moment(this.workingTime.morningWorking, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonStartAt = moment(this.workingTime.afternoonStartAt, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonEndAt = moment(this.workingTime.afternoonEndAt, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonWorking = moment(this.workingTime.afternoonWorking, 'HH:mm').format('HH:mm');

    this.breakTime = moment(this.workingTime.afternoonStartAt, 'HH:mm').diff(moment(this.workingTime.morningEndAt, 'HH:mm'), 'hours');

    this.absenceDayReq = new AbsenceDayRequest();
    this.absenceDayReq.reason = '';
    this.absenceDayReq.absences = [] as AbsenceDayDto[];
  }

  onchange(event: any) {
    this.customTime.hour = event;
    this.getNote(this.customTime.absenceTime);
    if (event == 0. || event == 0.0 || event == null || event == "" || event > 7.5) {
      this.note = '';
      this.saving = false;
    } else {
      this.saving = true;
    }
  }

  stringToFloat(input: string): number {
    return parseFloat(input);
  }

  formatTime(input: number) {
    const hour = Math.floor(input);
    const minute = Math.round((input - hour) * 60);
    const rhour = ('0' + hour).slice(-2);
    const rminute = ('0' + minute).slice(-2);
    return rhour + ':' + rminute;
  }

  converCustomTime(input: string) {
    if (input.split('').length != 3) {
      if (input.substring(1, 2) == '.') {
        input = input.substring(1, 0)
      }
    }
    return parseFloat(input);
  }

  radioChange(value) {
    this.customTime.absenceTime = value;
    this.getNote(value);
  }

  getNote(value) {
    return '';
    if (this.customTime.hour) {
      const time = this.converCustomTime(this.customTime.hour.toString());
      switch (value) {
        case 1:
          this.requestTime = moment(this.workingTime.morningStartAt, 'HH:mm').add(time, 'hours').format('HH:mm');
          if (moment(this.requestTime, 'HH:mm').diff(moment(this.workingTime.morningEndAt, 'HH:mm')) >= 0) {
            this.requestTime = moment(this.requestTime, 'HH:mm').add(this.breakTime, 'hours').format('HH:mm');
          }
          this.note = 'Bạn phải có mặt ở công ty trước ' + this.requestTime;
          break;
        case 2:
          this.note = 'Bạn được phép vắng mặt ' + this.formatTime(time);
          break;
        case 3:
          this.requestTime = moment(this.workingTime.afternoonEndAt, 'HH:mm').subtract(time, 'hours').format('HH:mm');
          if (moment(this.requestTime, 'HH:mm').diff(moment(this.workingTime.afternoonStartAt, 'HH:mm')) <= 0) {
            this.requestTime = moment(this.requestTime, 'HH:mm').subtract(this.breakTime, 'hours').format('HH:mm');
          }
          this.note = 'Bạn được phép rời công ty sau ' + this.requestTime;
          break;
      }
    }
  }

  public mask = {
    guide: false,
    showMask: false,
    mask: [/[0-7]/, '.', /\d/,]
  };

  submitReq() {
    if (this.customTime.hour == 0. || this.customTime.hour == 0.0 || this.customTime.hour == null || this.customTime.hour > 7.5) {
      abp.message.error('Số giờ is required!');
      return;
    }
    if (!this.absenceDayReq.reason.trim()) {
      abp.message.error('Reason is required!');
      return;
    }
    this.absenceDayReq.absences = [] as AbsenceDayDto[];
    this.absenceDayReq.absences.push({
      dateAt: moment(this.selectedDate).format("YYYY-MM-DD"),
      dateType: this.APP_CONSTANT.AbsenceType.Custom,
      hour: this.customTime.hour,
      absenceTime: this.customTime.absenceTime
    } as AbsenceDayDto)

    this.absenceDayReq.type = this.APP_CONSTANT.DayAbsenceType.Off;
    this.absenceDayReq.dayOffTypeId = 1; //Loại nghỉ

    this.saving = true
    this.absenceDayService.submitAbsenceDays(this.absenceDayReq).subscribe(resp => {
      if(resp.success) {
        if(resp.result.absences.find(item => item.status == 3)){
          this.notify.warn(this.l('Submit Đi muộn/ Về sớm bị reject!'));
        }
        else {
          this.notify.success(this.l('Submit Đi muộn/ Về sớm successfully!'));
        }
      }
      this.close(true);
      this.saving = false
    }, (err) => {
      this.saving = false
    });
  }

  close(res): void {
    this.diaLogRef.close(res);
  }

}


export class CustomTimeDto {
  hour: number;
  absenceTime: number;
}

export class Time {
  hour: number;
  minute: number;
}
