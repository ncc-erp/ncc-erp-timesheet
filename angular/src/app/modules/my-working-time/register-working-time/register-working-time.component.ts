import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { RegisterInforDto } from '../my-working-time.component';

@Component({
  selector: 'app-register-working-time',
  templateUrl: './register-working-time.component.html',
  styleUrls: ['./register-working-time.component.css']
})
export class RegisterWorkingTimeComponent implements OnInit {
  title: string;
  startMor: string;
  endMor: string;
  public submitted: boolean = false;
  constructor(public dialogRef: MatDialogRef<RegisterWorkingTimeComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RegisterInforDto) { }

  ngOnInit() {
    this.title = this.data.morningStartTime != null ? 'Edit registered new working time' : 'Register new working time';
  }
  onNoClick(): void {
    this.dialogRef.close();
  }
  SaveTime() {
    this.submitted = true;
    if(!this.data.applyDate || !this.data.morningStartTime || !this.data.afternoonStartTime || !this.data.afternoonEndTime) {
      abp.notify.error('This field is required!')
      return;
    }
    this.data.morningWorkingTime = this.minsToStr( this.strToMins(this.data.morningEndTime) - this.strToMins(this.data.morningStartTime)).toString();
    this.data.afternoonWorkingTime = this.minsToStr( this.strToMins(this.data.afternoonEndTime) - this.strToMins(this.data.afternoonStartTime)).toString();
    if ((Number(this.data.morningWorkingTime) + Number(this.data.afternoonWorkingTime)) < 8) {
      abp.notify.error('Total working time min 8 hours')
      return;
    }
    this.dialogRef.close(this.data);
  }
  strToMins(t) {
    var s = t.split(":");
    return Number(s[0]) * 60 + Number(s[1]);
  }
  
  minsToStr(t) {
    let timeToFormat =Math.trunc(t / 60).toString().length == 1 ? '0' + Math.trunc(t / 60)+':'+('00' + t % 60).slice(-2) : Math.trunc(t / 60)+':'+('00' + t % 60).slice(-2);
    return this.timetoNumber(timeToFormat).toFixed(2);
  }
  timetoNumber(time){
    var hoursMinutes = time.split(/[.:]/);
    var hours = parseInt(hoursMinutes[0], 10);
    var minutes = hoursMinutes[1] ? parseInt(hoursMinutes[1], 10) : 0;
    return hours + minutes / 60;
}
}
