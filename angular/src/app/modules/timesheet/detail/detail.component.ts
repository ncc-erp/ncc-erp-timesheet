import { convertMinuteToHour, countDayInWeek } from './../../../../shared/common-time';
import { AppComponentBase } from '@shared/app-component-base';
import { Component, OnInit, Inject, Optional, Injector } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material';
import { TimesheetService } from '@app/service/api/timesheet.service';
import { APP_CONFIG } from '@app/constant/api-config.constant';
import * as _ from 'lodash';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Component({
  selector: 'app-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class DetailComponent extends AppComponentBase implements OnInit {
  enumDayOfWeek = _.cloneDeep(APP_CONFIG.EnumDayOfWeek);//clone
  timesheet: any;
  timesheetId: any;
  enumDate: any;
  statusTimesheet = APP_CONSTANT;
  constructor(
    injector: Injector,
    private _dialogRef: MatDialogRef<DetailComponent>,
    private timesheetService: TimesheetService,
    @Optional() @Inject(MAT_DIALOG_DATA) private data: any
  ) {
    super(injector);
  }

  ngOnInit() {
    this.timesheetId = this.data.timesheetId;
    this.getOne(this.timesheetId);
  }
  getOne(timesheetId: any): any {
    this.timesheetService.getOne(timesheetId, "Member,TimesheetItems.Task,TimesheetItems.Project").subscribe((res: any) => {
      this.enumDate = countDayInWeek(res.result.startDate);
      console.log(res);
      this.handleAfterGetTimesheet(res);
    });
  }

  private handleAfterGetTimesheet(res: any) {
    this.timesheet = res.result;
    this.timesheet.startDate = moment(new Date(this.timesheet.startDate)).format("DD"+ " " + "MMM" + " " + "YYYY").toString();
    this.timesheet.endDate = moment(new Date(this.timesheet.endDate)).format("DD"+ " " + "MMM" + " " + "YYYY").toString();
    // map // enumDayOfWeek
    // this.enumDayOfWeek = this.enumDayOfWeek.map((res: any) => {
    //   res.items = (this.timesheet.timesheetItems || []).filter(it => it.dayOfWeek == res.value || res.value == -1);
    //   res.totalTime = res.items.reduce((total, currentValue, currentIndex, arr) => {
    //     return total + currentValue.workingTime;
    //   }, 0);
    //   return res;
    // });
    
    this.timesheet.timesheetItems = this.timesheet.timesheetItems.map((value: any, key) => {
      return {
        timesheetId: value.timesheetId,
        isCharged: value.isCharged,
        projectName: value.project.name,
        taskName: value.task.name,
        typeOfWork: value.typeOfWork,
        status: value.status,
        mon: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Monday),
        tue: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Tuesday),
        wed: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Wednesday),
        thu: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Thursday),
        fri: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Friday),
        sat: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Saturday),
        sun: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Sunday),
        total: this.getWorkingTimeByDayOfWeek(value)
      }
    });
    // temp.map(value => {
    //   let totalWorkingTime = 0;
    //   totalWorkingTime = value.reduce((total, currentValue, currentIndex, arr) => {
    //     return total + currentValue.workingTime;
    //   }, 0);
    //   return {
    //     timesheetId: value.timesheetId,
    //     isCharged: value.isCharged,
    //     projectName: value.project.name,
    //     taskName: value.task.name,
    //     typeOfWork: value.typeOfWork,
    //     status: value.status,
    //     mon: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Monday),
    //     tue: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Tuesday),
    //     wed: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Wednesday),
    //     thu: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Thursday),
    //     fri: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Friday),
    //     sat: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Saturday),
    //     sun: this.getWorkingTimeByDayOfWeek(value, APP_CONSTANT.EnumDayOfWeek.Sunday),
    //     totalWorkingTime: convertMinuteToHour(totalWorkingTime)
    //   }
    // })
    // this.timesheet.push({
    //   projectName: '',
    //   taskName: '',
    //   mon: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Monday),
    //   tue: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Tuesday),
    //   wed: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Wednesday),
    //   thu: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Thursday),
    //   fri: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Friday),
    //   sat: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Saturday),
    //   sun: this.getTotalTimeByWeek(temp, APP_CONSTANT.EnumDayOfWeek.Sunday),
    //   totalWorkingTime: this.getTotalTimeByWeek(temp, -1)
    // })
    
  }

  getWorkingTimeByDayOfWeek(value: any, day: number = -1) {
    if (value.dayOfWeek == day || day == -1) {
      return {
        note: value.note,
        hours: convertMinuteToHour(value.workingTime)
      }
    } else {
      return {
        note: '',
        hours: 0
      }
    }
  }

  getTotalTimeByWeek(value, day: number = -1) {
    let total = 0;
    if (value.length != 0) {
      value.map(result => {
        result.map(res => {
          if (res.dayOfWeek == day) {
            total += res.workingTime;
          }
          if (day == -1) {
            total += res.workingTime;
          }
        })
      })
    }
    return convertMinuteToHour(total);
  }

  // getTimesheetItemByDayOfWeek(value: any, day: number = -1): any {
  //   let tmIt = value.filter(res => res.dayOfWeek == day || day == -1);
  //   if (!tmIt || tmIt.length == 0) {
  //     return {
  //       hours: 0,
  //       tooltip: ''
  //     };
  //   }

  //   const hours = tmIt.reduce((total, currentItem) => {
  //     return total += currentItem.workingTime;
  //   }, 0);
  //   if (day == -1) {
  //     return {
  //       hours: convertMinuteToHour(hours),
  //       tooltip: ''
  //     };
  //   }
  //   let noteArray = [];
  //   tmIt.map(vl => {
  //     if(vl.note != '' && vl.note != undefined){
  //       noteArray.push(vl.note + `[ workingTime: ${convertMinuteToHour(vl.workingTime)}]`);
  //     }
  //   })
  //   return {
  //     hours: convertMinuteToHour(hours),
  //     tooltip: noteArray.join()
  //   };
  // }
  
  approveTimesheet() {
    this.timesheetService.approveTimesheet(this.timesheetId).subscribe(res => {
      this.notify.success(this.l('Approve Timesheet successfully'));
      this.close(true);
    })
  }

  // rejectTimesheet() {
  //   this.timesheetService.rejectTimesheet(this.timesheetId).subscribe(res => {
  //     if (res.success == true) {
  //       this.notify.success(this.l('Reject Timesheet successfully'));
  //       this.close(true);
  //     }
  //   })
  // }

  close(result: any): void {
    this._dialogRef.close(result);
  }
}
