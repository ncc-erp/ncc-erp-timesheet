import { SpecialProjectTaskSettingService } from './../../service/api/special-project-task-config.service';
import { CreateEditTimesheetItemComponent } from './create-edit-timesheet-item/create-edit-timesheet-item.component';
import { MatDialog, MatTabChangeEvent, MAT_DIALOG_DATA } from '@angular/material';
import { GetTimeSheetDto, DayOfWeek, WeekByTask, MyTimeSheetDto, ProjectIncludingTaskDto } from '../../service/api/model/common-DTO';
import { Component, OnInit, Injector, Optional, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import * as _ from 'lodash';
import * as moment from 'moment';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { CreateEditTimesheetByWeekComponent } from './create-edit-timesheetByWeek/create-edit-timesheetByWeek.component';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import { convertHourtoMinute, convertMinuteToHour, convertFloatHourToMinute } from '@shared/common-time';
import { AbsenceDayService } from '@app/service/api/absence-day.service';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};
@Component({
  selector: 'app-mytimesheets',
  templateUrl: './mytimesheets.component.html',
  styleUrls: ['./mytimesheets.component.css'],
  providers: [

    { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE] },

    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})
export class MyTimeSheetsComponent extends AppComponentBase implements OnInit {
  ADD_MY_TIMESHEET = PERMISSIONS_CONSTANT.AddMyTimesheet;
  EDIT_MY_TIMESHEET = PERMISSIONS_CONSTANT.EditMyTimesheet;
  DELETE_MY_TIMESHEET = PERMISSIONS_CONSTANT.DeleteMyTimesheet;
  SUBMIT_MY_TIMESHEET = PERMISSIONS_CONSTANT.SubmitMyTimesheet;

  activeDay: any;
  days;
  day;
  offDay = [];
  month;
  year;
  dayOff
  viewDate: Date
  activeWeek: number = 0;
  startDate: string;
  endDate: string;
  typeOfView = this.APP_CONSTANT.MyTimesheetView.Day;
  timesheets = [] as GetTimeSheetDto[];
  displayDay: any;
  mapDayOfWeek: DayOfWeek[] = [];
  mapWeekByTask = [] as WeekByTask[];
  projectIncludingTasks = [] as ProjectIncludingTaskDto[];
  weekByTask = {} as WeekByTask;
  isTableLoading = false;
  isCanNextBack = true;
  totalWorkingTime: number = 0;
  totalWorkingTimeByDay: number;
  workingtime: number;
  isMapDayOfWeekLoaded: boolean;
  selectedDays: Map<string, number>;
  autoSubmitTimesheet: boolean;
  autoSubmitAt: string;
  specialProjectTask = {} as SpecialProjectTaskSettingDTO;
  isRefresh: boolean = false;
  constructor(
    @Optional() @Inject(MAT_DIALOG_DATA) private data: any,
    injector: Injector,
    private route: ActivatedRoute,
    private router: Router,
    private timesheetService: MyTimesheetService,
    private _dialog: MatDialog,
    private projectService: ProjectManagerService,
    private absenceDayService: AbsenceDayService,
    private specialProjectTaskSerivice: SpecialProjectTaskSettingService
  ) {
    super(injector);
    this.viewDate = new Date();
    this.selectedDays = new Map<string, number>();
  }

  receiveRefresh($event) {
    this.isRefresh = $event;
  }

  ngOnInit() {
    this.displayDay = moment().format('YYYY-MM-DD');
    this.startDate = moment().startOf('isoWeek').format('YYYY-MM-DD');
    this.endDate = moment().startOf('isoWeek').add(6, 'd').format('YYYY-MM-DD');
    this.getAllTimeSheet();
    this.getProjectsIncludingTasks();
    this.updateDay();
    this.getSpecialProjectTaskSetting();
  }

  refreshData() {
    this.getAllTimeSheet();
    this.getProjectsIncludingTasks();
  }

  getSpecialProjectTaskSetting() {
    this.specialProjectTaskSerivice.get().subscribe((res: any) => {
      this.specialProjectTask = res.result;
    })
}

  getAllTimeSheet() {
    this.isTableLoading = true;
    this.isCanNextBack = false;
    this.timesheetService.getAllTimeSheet(this.startDate, this.endDate).subscribe(result => {
      this.timesheets = result.result;
      // this.canSubmit = this.timesheets.some(t => t.status === this.APP_CONSTANT.TimesheetStatus.Draft);
      this.buildDataForDay(this.timesheets);
      this.isTableLoading = false;
      let that = this;
      setTimeout(() => {
        that.isCanNextBack = true;
      }, 1000)
    })
  }

  sumWorkingTimeByDay(index: number) {

    if (index == 0) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.monWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 1) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.tueWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 2) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.wedWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 3) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.thuWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 4) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.friWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 5) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.satWorkingTime)).reduce((a, b) => a + b, 0);
    } else if (index == 6) {
      return this.mapWeekByTask.map(s => convertHourtoMinute(s.sunWorkingTime)).reduce((a, b) => a + b, 0);
    }
  }


  buildDataForDay(lstTimeSheet: GetTimeSheetDto[]) {
    const dates = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
    this.mapDayOfWeek = [];
    this.totalWorkingTimeByDay = 0;
    for (let i = 0; i < 7; i++) {
      let d = new DayOfWeek();
      d.name = dates[i];
      d.dateAt = moment().add(this.activeWeek, 'w').startOf('isoWeek').add(i, 'd').format('YYYY-MM-DD');
      d.timesheets = lstTimeSheet.filter(s => s.dateAt.substring(0, 10) == d.dateAt);
      d.totalTime = d.timesheets.map(s => s.workingTime).reduce((a, b) => a + b, 0);
      this.mapDayOfWeek.push(d);
      this.totalWorkingTimeByDay += d.totalTime;

      if (moment(this.displayDay).format('YYYY-MM-DD') == d.dateAt)
        this.activeDay = i;
    }

    this.buildDataForWeek(lstTimeSheet);
    this.isMapDayOfWeekLoaded = true;
  }
  dayClicked(event: any): void {

  }
  buildDataForWeek(lstTimeSheet: GetTimeSheetDto[]) {
    let that = this;
    this.totalWorkingTime = 0;
    this.mapWeekByTask = _(lstTimeSheet).groupBy(x => x.projectTaskId)
      .map(function (value, key) {
        let isEditable = true;
        let obj = lstTimeSheet.find(s => s.projectTaskId == key);

        let monTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[0].dateAt);
        isEditable = isEditable && monTimeSheets.length > 1 ? false : isEditable;


        let tueTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[1].dateAt);
        isEditable = isEditable && tueTimeSheets.length > 1 ? false : isEditable;


        let wedTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[2].dateAt);
        isEditable = isEditable && wedTimeSheets.length > 1 ? false : isEditable;


        let thuTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[3].dateAt);
        isEditable = isEditable && thuTimeSheets.length > 1 ? false : isEditable;


        let friTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[4].dateAt);
        isEditable = isEditable && friTimeSheets.length > 1 ? false : isEditable;


        let satTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[5].dateAt);
        isEditable = isEditable && satTimeSheets.length > 1 ? false : isEditable;


        let sunTimeSheets = lstTimeSheet.filter(s => s.projectTaskId == key
          && s.dateAt.substring(0, 10) == that.mapDayOfWeek[6].dateAt);
        isEditable = isEditable && sunTimeSheets.length > 1 ? false : isEditable;

        let monWorkingTime = monTimeSheets ? monTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let tueWorkingTime = tueTimeSheets ? tueTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let wedWorkingTime = wedTimeSheets ? wedTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let thuWorkingTime = thuTimeSheets ? thuTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let friWorkingTime = friTimeSheets ? friTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let satWorkingTime = satTimeSheets ? satTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;
        let sunWorkingTime = sunTimeSheets ? sunTimeSheets.map(s => s.workingTime).reduce((a, b) => a + b, 0) : 0;

        let wbt = {
          isAddNew: false,
          isEditable: isEditable,
          projectTaskId: obj.projectTaskId,
          projectName: obj.projectName,
          taskName: obj.taskName,
          projectCode: obj.projectCode,
          customerName: obj.customerName,
          typeOfWork: obj.typeOfWork,
          isCharged: obj.isCharged,
          totalTime: 0,


          monWorkingTime: that.convertMinuteToHour(monWorkingTime),
          tueWorkingTime: that.convertMinuteToHour(tueWorkingTime),
          wedWorkingTime: that.convertMinuteToHour(wedWorkingTime),
          thuWorkingTime: that.convertMinuteToHour(thuWorkingTime),
          friWorkingTime: that.convertMinuteToHour(friWorkingTime),
          satWorkingTime: that.convertMinuteToHour(satWorkingTime),
          sunWorkingTime: that.convertMinuteToHour(sunWorkingTime),

          idMonday: monTimeSheets && monTimeSheets.length > 0 ? monTimeSheets[0].id : 0,
          idTueday: tueTimeSheets && tueTimeSheets.length > 0 ? tueTimeSheets[0].id : 0,
          idWeday: wedTimeSheets && wedTimeSheets.length > 0 ? wedTimeSheets[0].id : 0,
          idThuday: thuTimeSheets && thuTimeSheets.length > 0 ? thuTimeSheets[0].id : 0,
          idFriday: friTimeSheets && friTimeSheets.length > 0 ? friTimeSheets[0].id : 0,
          idSatday: satTimeSheets && satTimeSheets.length > 0 ? satTimeSheets[0].id : 0,
          idSunday: sunTimeSheets && sunTimeSheets.length > 0 ? sunTimeSheets[0].id : 0,

        } as WeekByTask;
        wbt.totalTime = monWorkingTime + tueWorkingTime + wedWorkingTime + thuWorkingTime + friWorkingTime + satWorkingTime + sunWorkingTime;

        that.totalWorkingTime += wbt.totalTime;

        return wbt;
      }).value();
  }

  getProjectsIncludingTasks() {
    this.projectService.GetProjectsIncludingTasks().subscribe(result => {
      this.projectIncludingTasks = result.result;
    })
  }

  showCreateOrEditTimesheetItemDialog(item?) {
    const dialogRef = this._dialog.open(CreateEditTimesheetItemComponent, {
      disableClose: true,
      data: {
        dateAt: this.displayDay,
        projectIncludingTasks: this.projectIncludingTasks,
        myTimeSheetId: item ? item.id : null,
        specialProjectTask: this.specialProjectTask
      }
    })
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.setIsRefresh();
        this.getAllTimeSheet();
      }
    }
    );
  }
  setIsRefresh(){
    this.isRefresh = true;
  }

  showCreateOrEditTimesheetByWeek(item?) {
    const dialogRef = this._dialog.open(CreateEditTimesheetByWeekComponent, {
      disableClose: true,
      data: {
        projectIncludingTasks: this.projectIncludingTasks,
      }
    })
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        let a = new WeekByTask();
        a.totalTime = 0;
        a.taskName = result.taskName;
        a.projectName = result.projectName;
        a.projectCode = result.projectCode;
        a.projectTaskId = result.projectTaskId;
        a.customerName = result.customerName;
        a.isCharged = result.isCharged;
        a.typeOfWork = result.typeOfWork;
        a.note = result.note;
        a.isEditable = true;
        a.isEditing = true;
        a.isAddNew = true;
        this.mapWeekByTask.push(a);
      }
    }
    );
  }

  viewBy(value) {
    this.typeOfView = value;
  }
  checkValidTimesheet(myTimeSheets: MyTimeSheetDto[]) {
    for (let i = 0; i < myTimeSheets.length; i++) {
      if (myTimeSheets[i].workingTime > this.APP_CONSTANT.MAX_WORKING_TIME || myTimeSheets[i].workingTime <= 0) {
        this.notify.error('working hours must be greater than 0 and less than  ' + this.APP_CONSTANT.MAX_WORKING_TIME + ' minutes');
        return false
      }
    }
    return true;
  }

  isSaving = false;
  SaveList(item: WeekByTask) {
    this.isSaving = true;
    item.isEditing = false;
    let myTimeSheets = [] as MyTimeSheetDto[];
    if (item.idMonday > 0 || convertFloatHourToMinute(item.monWorkingTime) > 0) {
      let mon = {
        id: item.idMonday,
        dateAt: this.mapDayOfWeek[0].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.monWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note

      } as MyTimeSheetDto;
      myTimeSheets.push(mon);
    }

    if (item.idTueday > 0 || convertFloatHourToMinute(item.tueWorkingTime) > 0) {
      let tue = {
        id: item.idTueday,
        dateAt: this.mapDayOfWeek[1].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.tueWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(tue);
    }

    if (item.idWeday > 0 || convertFloatHourToMinute(item.wedWorkingTime) > 0) {
      let wed = {
        id: item.idWeday,
        dateAt: this.mapDayOfWeek[2].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.wedWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(wed);
    }

    if (item.idThuday > 0 || convertFloatHourToMinute(item.thuWorkingTime) > 0) {
      let thu = {
        id: item.idThuday,
        dateAt: this.mapDayOfWeek[3].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.thuWorkingTime.toString()),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(thu);
    }

    if (item.idFriday > 0 || convertFloatHourToMinute(item.friWorkingTime) > 0) {
      let fri = {
        id: item.idFriday,
        dateAt: this.mapDayOfWeek[4].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.friWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(fri);
    }

    if (item.idSatday > 0 || convertFloatHourToMinute(item.satWorkingTime) > 0) {
      let sat = {
        id: item.idSatday,
        dateAt: this.mapDayOfWeek[5].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.satWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(sat);
    }

    if (item.idSunday > 0 || convertFloatHourToMinute(item.sunWorkingTime) > 0) {
      let sun = {
        id: item.idSunday,
        dateAt: this.mapDayOfWeek[6].dateAt,
        projectTaskId: item.projectTaskId,
        isCharged: item.isCharged,
        workingTime: convertFloatHourToMinute(item.sunWorkingTime),
        typeOfWork: item.typeOfWork,
        note: item.note
      } as MyTimeSheetDto;
      myTimeSheets.push(sun);
    }

    let that = this;
    if (this.checkValidTimesheet(myTimeSheets) === false) {
      this.isSaving = false;
      item.isEditing = true;
      return;
    }
    this.timesheetService.SaveList(myTimeSheets).subscribe((data) => {
      this.notify.success(this.l('Save timesheets successfully'));
      this.getAllTimeSheet();

      let mon = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[0].dateAt);
      item.idMonday = mon ? mon.id : 0;

      let tue = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[1].dateAt);
      item.idTueday = tue ? tue.id : 0;

      let wed = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[2].dateAt);
      item.idWeday = wed ? wed.id : 0;

      let thu = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[3].dateAt);
      item.idThuday = thu ? thu.id : 0;

      let fri = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[4].dateAt);
      item.idFriday = fri ? fri.id : 0;

      let sat = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[5].dateAt);
      item.idSatday = sat ? sat.id : 0;

      let sun = data.result.find(s => s.dateAt.substring(0, 10) == that.mapDayOfWeek[6].dateAt);
      item.idSunday = sun ? sun.id : 0;

      data.result.forEach(element => {
        let obj = that.timesheets.find(s => s.id == element.id);
        if (obj) {
          obj.workingTime = element.workingTime;
        } else {
          element.projectCode = item.projectCode;
          element.projectName = item.projectName;
          element.taskName = item.taskName;
          element.projectTaskId = item.projectTaskId;
          element.customerName = item.customerName;
          element.typeOfWork = item.typeOfWork;
          element.isCharged = item.isCharged;
          element.note = item.note;
          that.timesheets.push(element);
        }
      });

      that.buildDataForDay(that.timesheets);
      that.isSaving = false;
    }, (err) => { item.isEditing = true; that.isSaving = false; });

  }
  edit(item: WeekByTask) {
    item.isEditing = true;
  }

  updateDay(): void {
    this.day = this.viewDate.getDate();
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }
  createTimesheetItem(): void {
    this.showCreateOrEditTimesheetItemDialog();
  }

  createTimesheet(): void {
    this.showCreateOrEditTimesheetByWeek();
  }

  editTimesheetItem(item): void {
    this.showCreateOrEditTimesheetItemDialog(item);
  }

  tabChanged(tabChangeEvent: MatTabChangeEvent) {
    if (tabChangeEvent.index >= this.mapDayOfWeek.length) {
      this.activeDay = this.mapDayOfWeek.length - 1;
    } else {
      this.activeDay = tabChangeEvent.index;
    }

    this.displayDay = this.mapDayOfWeek[this.activeDay].dateAt;

  }

  next() {
    if (!this.isCanNextBack) {
      return;
    }
    if (this.typeOfView == this.APP_CONSTANT.MyTimesheetView.Day) {
      if (this.activeDay == 6) {
        this.activeWeek++;
        this.startDate = moment().startOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
        this.endDate = moment().endOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
        this.displayDay = this.startDate;
        this.getAllTimeSheet();
      } else {
        this.activeDay++;
      }
    } else {
      this.activeWeek++;
      this.startDate = moment().startOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
      this.endDate = moment().endOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
      this.displayDay = this.startDate;
      this.getAllTimeSheet();
    }
  }

  back() {
    if (!this.isCanNextBack) {
      return;
    }
    if (this.typeOfView == this.APP_CONSTANT.MyTimesheetView.Day) {
      if (this.activeDay == 0) {

        this.activeWeek--;
        this.startDate = moment().startOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
        this.endDate = moment().endOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
        this.displayDay = this.endDate;
        this.getAllTimeSheet();
      } else {
        this.activeDay--;
      }
    } else {
      this.activeWeek--;
      this.startDate = moment().startOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
      this.endDate = moment().endOf('isoWeek').add(this.activeWeek, 'w').format('YYYY-MM-DD');
      this.displayDay = this.endDate;
      this.getAllTimeSheet();
    }
  }

  today() {
    this.displayDay = new Date();
    this.customDate();
  }

  customDate() {
    let newStartDate = moment(this.displayDay).startOf('isoWeek').format('YYYY-MM-DD');
    let newEndDate = moment(this.displayDay).endOf('isoWeek').format('YYYY-MM-DD');
    if (this.startDate != newStartDate) {
      this.activeWeek = moment(newStartDate).diff(moment().startOf('isoWeek'), 'w');
      this.startDate = newStartDate;
      this.endDate = newEndDate;
      this.getAllTimeSheet();
    } else {
      this.activeDay = this.mapDayOfWeek.findIndex(s => s.dateAt == moment(this.displayDay).format('YYYY-MM-DD'))
    }
  }

  submitTimesheet() {
    let startDate = this.mapDayOfWeek[0].dateAt;
    let endDate = this.mapDayOfWeek[this.mapDayOfWeek.length - 1].dateAt;
    this.isTableLoading = true;
    this.isCanNextBack = false;
    this.timesheetService.SubmitToPending(startDate, endDate).subscribe(res => {
      this.notify.info(this.l(res.result));
      this.getAllTimeSheet();
    })
    this.isTableLoading = false;
    this.isCanNextBack = true;
  }

  delete(item: GetTimeSheetDto): void {
    abp.message.confirm(
      "Delete Timesheet  ?",
      (result: boolean) => {
        if (result) {
          this.timesheetService.delete(item.id).subscribe(() => {
            abp.notify.info('Deleted');
            this.setIsRefresh();
            this.getAllTimeSheet();
          });
        }
      }
    );
  }

  public mask1 = {
    guide: false,
    showMask: false,
    mask: [/\d/, /\d/, ':', /\d/, /\d/]
  };
  public mask2 = {
    guide: false,
    showMask: false,
    mask: [/\d/,/\d/, '.', /\d/]
  };
}
export class SpecialProjectTaskSettingDTO {
  projectTaskId: number;
}




