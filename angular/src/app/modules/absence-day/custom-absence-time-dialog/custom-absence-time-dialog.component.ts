import { Component, Inject, Injector, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material';
import { WorkingTimeDto } from '@app/service/api/model/working-time-dto';
import { AppComponentBase } from '@shared/app-component-base';
import { time } from 'console';
import * as moment from 'moment';
@Component({
  selector: 'app-custom-absence-time-dialog',
  templateUrl: './custom-absence-time-dialog.component.html',
  styleUrls: ['./custom-absence-time-dialog.component.css']
})
export class CustomAbsenceTimeDialogComponent extends AppComponentBase implements OnInit {
  timeControl = new FormControl("", [Validators.required]);
  customTime = new CustomTimeDto;
  workingTime = new WorkingTimeDto();
  saving: boolean = false;
  note = '';
  requestTime: string;
  breakTime: number;
  constructor(injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    super(injector)
  }
  ngOnInit() {
    this.workingTime = this.data;
    this.customTime.hour = null;
    this.customTime.absenceTime = 1;
    this.workingTime.morningStartAt = moment(this.workingTime.morningStartAt, 'HH:mm').format('HH:mm');
    this.workingTime.morningEndAt = moment(this.workingTime.morningEndAt, 'HH:mm').format('HH:mm');
    this.workingTime.morningWorking = moment(this.workingTime.morningWorking, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonStartAt = moment(this.workingTime.afternoonStartAt, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonEndAt = moment(this.workingTime.afternoonEndAt, 'HH:mm').format('HH:mm');
    this.workingTime.afternoonWorking = moment(this.workingTime.afternoonWorking, 'HH:mm').format('HH:mm');

    this.breakTime = moment(this.workingTime.afternoonStartAt, 'HH:mm').diff(moment(this.workingTime.morningEndAt, 'HH:mm'), 'hours');

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

  radioChange(event) {
    this.getNote(event);
  }

  getNote(value) {
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
}

export class CustomTimeDto {
  hour: number;
  absenceTime: number;
}

export class Time {
  hour: number;
  minute: number;
}
