import { PTaskDto } from './../../../service/api/model/common-DTO';

// import { MonthViewDay } from '@node_modules/calendar-utils';
// import { DayOffService } from '@app/service/api/day-off.service';
import { Component, Inject, Injector, OnInit, Optional } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { MyTimesheetService } from '@app/service/api/mytimesheet.service';
import { TaskService } from '@app/service/api/task.service';
import { AppComponentBase } from '@shared/app-component-base';
import { convertDateToString, convertFloatHourToMinute, convertMinuteToFloat, convertMinuteToHour } from '@shared/common-time';
import * as moment from 'moment';
import { APP_CONFIG } from './../../../constant/api-config.constant';
import { DayOfWeek, FilterRequest, MyTimeSheetDto, ProjectIncludingTaskDto } from './../../../service/api/model/common-DTO';
import { ProjectManagerService } from './../../../service/api/project-manager.service';
import { TimesheetWarningDialogComponent } from '../timesheet-warning-dialog/timesheet-warning-dialog.component';
import { WarningMyTimesheetDto } from '@app/service/api/model/mytimesheet-dto';
import { TypeAction } from '../const';

// import { dayOffDTO } from '@app/modules/off-day/off-day.component';
// import { CalendarEvent } from 'angular-calendar/modules/common/calendar-common.module';
// import { DatePipe } from '@angular/common';
// import { AbsenceDayService } from '@app/service/api/absence-day.service';
// import { AbsenceRequest } from '@app/modules/absence-day/absence-day.component';

@Component({
  selector: 'app-create-edit-timesheet-item',
  templateUrl: './create-edit-timesheet-item.component.html',
  styleUrls: ['./create-edit-timesheet-item.component.css'],
  // providers: [DatePipe]
})
export class CreateEditTimesheetItemComponent extends AppComponentBase implements OnInit {
  title: string;
  formTimesheetItem: FormGroup;
  listTypeWork: any = [];
  listProject: any = [];
  listTask: any = [];
  filterRequest = new FilterRequest();
  userId: number;
  currentDate: any;
  dateAt = {} as DayOfWeek;
  myTimesheet = {} as MyTimeSheetDto;
  selectedProject = {} as ProjectIncludingTaskDto;
  dateMothYear: any;
  projectIncludingTasks = [] as ProjectIncludingTaskDto[];
  strWorkingTime: any;
  strTargetUserWorkingTime: any;
  isLoading = false;
  defaultTask: boolean = false;
  day: Date | string | number;
  // events: CalendarEvent[] = [];
  // dayOffs: dayOffDTO[] = [];
  // monthViewBody: MonthViewDay[];

  // year;

  // month;
  // months;
  // days;
  // branch = 0
  // listYear = [];
  // viewDate : Date
  // absenceReqs: AbsenceRequest[];

  DEFAULT_TARGET_USER_ID_FOR_PROJECT_ = "DEFAULT_TARGET_USER_ID_FOR_PROJECT_";
  warningMyTimesheet = {} as WarningMyTimesheetDto;

  constructor(
    // private dayOffService : DayOffService,
    // private datePipe : DatePipe,
    private fb: FormBuilder,
    // private absenceDayService: AbsenceDayService,

    private injector: Injector,
    private _dialogRef: MatDialogRef<CreateEditTimesheetItemComponent>,
    private _dialog: MatDialog,
    private taskService: TaskService,
    private projectManagerService: ProjectManagerService,
    private timesheetItemService: MyTimesheetService,
    @Optional() @Inject(MAT_DIALOG_DATA) private data: any
  ) {
    super(injector);
  }

  typeOfWorks = [
    { id: APP_CONSTANT.EnumTypeOfWork.Normalworkinghours, name: "Normal Working Hour" },
    { id: APP_CONSTANT.EnumTypeOfWork.Overtime, name: "Overtime" }];

  ngOnInit() {
    this.day = moment(this.data.dateAt).format("YYYY-MM-DD");
    this.projectIncludingTasks = this.data.projectIncludingTasks;
    let myTimesheetID = this.data.myTimeSheetId;
    this.title = myTimesheetID != null ? 'Edit Timesheet ' : 'New Timesheet';
    if (myTimesheetID == null) {
      this.onCreateTimesheet();
    } else {
      this.isLoading = true;
      this.timesheetItemService.getById(myTimesheetID).subscribe(res => {
        this.myTimesheet = res.result;
        this.strWorkingTime = convertMinuteToFloat(this.myTimesheet.workingTime);
        this.strTargetUserWorkingTime = convertMinuteToFloat(this.myTimesheet.targetUserWorkingTime);        
        this.onEditTimesheet();
        this.isLoading = false;
      });
    }
  }

  private onEditTimesheet() {
    this.selectedProject = this.findSelectedProject();
    let task = this.selectedProject.tasks.find(s => s.projectTaskId == this.myTimesheet.projectTaskId);
    if (task){
      this.defaultTask = task.isDefault;
    }
  }

  private processDefaultProjectTask(){
    this.myTimesheet.typeOfWork = this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours;
    for (let i = 0; i < this.projectIncludingTasks.length; i++) {
      let project = this.projectIncludingTasks[i];
      for (let j = 0; j < project.tasks.length; j++) {
        let task = project.tasks[j];
        if (task.isDefault) {
          this.selectedProject = project;
          this.myTimesheet.projectTaskId = task.projectTaskId;
          this.defaultTask = true;
          return;
        }
      }
    }
  }

  private processSpecialTask(){
    if (this.isSpecialTask()){
      this.strWorkingTime = 4;
    }
  }

  private onCreateTimesheet() {
    this.processDefaultProjectTask();
    this.processSpecialTask();
  }

  onProjectChange() {
    if (this.selectedProject.tasks != null && this.selectedProject.tasks.length > 0) {
      this.myTimesheet.projectTaskId = this.selectedProject.tasks[0].projectTaskId;
      this.defaultTask = this.selectedProject.tasks[0].isDefault;
      this.onTaskChange();
    }

    if (this.selectedProject.projectUserType == this.APP_CONSTANT.EnumUserType.Shadow) {
      let projectTargetUserId = localStorage.getItem(this.DEFAULT_TARGET_USER_ID_FOR_PROJECT_ + this.selectedProject.id);
      if (projectTargetUserId) {
        this.myTimesheet.projectTargetUserId = parseInt(projectTargetUserId);
      } else if (this.selectedProject.targetUsers && this.selectedProject.targetUsers.length > 0) {
        this.myTimesheet.projectTargetUserId = this.selectedProject.targetUsers[0].projectTargetUserId;
      }
    }
  }

  isSpecialTask(){
    return this.myTimesheet.projectTaskId == this.data.specialProjectTask.projectTaskId;
  }

  onTaskChange(): void {
    if (this.isSpecialTask()) {
      this.strWorkingTime = 4;
      this.myTimesheet.typeOfWork = APP_CONFIG.EnumTypeOfWork[0].value;
    }

    let task = this.findSelectedTaskId();
    if (task) {
      this.defaultTask = task.isDefault;
    }

  }

  private findSelectedProject(): ProjectIncludingTaskDto {
    for (let i = 0; i < this.projectIncludingTasks.length; i++) {
      let project = this.projectIncludingTasks[i];
      for (let j = 0; j < project.tasks.length; j++) {
        let task = project.tasks[j];
        if (task.projectTaskId == this.myTimesheet.projectTaskId) {
          return project;
        }
      }
    }
  }

  private findSelectedTaskId(): PTaskDto {
    for (let i = 0; i < this.projectIncludingTasks.length; i++) {
      let project = this.projectIncludingTasks[i];
      for (let j = 0; j < project.tasks.length; j++) {
        let task = project.tasks[j];
        if (task.projectTaskId == this.myTimesheet.projectTaskId) {
          return task;
        }
      }
    }
  }

  private onUpdateDefaultProjectTask(projetTaskId: number) {
    this.projectIncludingTasks.forEach((project) => {
      project.tasks.forEach((task) => {
        if (projetTaskId == task.projectTaskId) {
          task.isDefault = true;
        }
        else if (task.isDefault) {
          task.isDefault = false;
        }
      })
    })
  }

  private onClearDefaultProjectTask() {
    this.projectIncludingTasks.forEach((project) => {
      project.tasks.forEach((task) => {
        if (task.isDefault) {
          task.isDefault = false;
        }
      })
    })
  }

  checkDefaultTask() {
    this.defaultTask = !this.defaultTask;
    if (this.defaultTask) {
      this.projectManagerService.updateDefaultProjectTask(this.myTimesheet.projectTaskId).subscribe((res) => {
        this.onUpdateDefaultProjectTask(this.myTimesheet.projectTaskId);
        abp.notify.success("Update default project task successfully!");
      })
    } else {
      this.projectManagerService.clearDefaultProjectTask().subscribe((res) => {
        this.onClearDefaultProjectTask();
        abp.notify.success("Clear default project task successfully!");
      })
    }

  }

  checkValid(): boolean {
    this.myTimesheet.workingTime = convertFloatHourToMinute(this.strWorkingTime);
    this.myTimesheet.targetUserWorkingTime = convertFloatHourToMinute(this.strTargetUserWorkingTime);
    if (this.myTimesheet.projectTaskId == undefined || this.myTimesheet.projectTaskId <= 0) {
      this.notify.error('You have to choose project and task');
      this.isLoading = false
      return false;
    }

    // if (this.myTimesheet.workingTime > this.APP_CONSTANT.MAX_WORKING_TIME ||
    //   this.myTimesheet.workingTime <= 0 ||
    //   this.myTimesheet.targetUserWorkingTime > this.APP_CONSTANT.MAX_WORKING_TIME ||
    //   this.myTimesheet.targetUserWorkingTime < 0 ||
    //   (isNaN(this.myTimesheet.workingTime) && this.myTimesheet.targetUserWorkingTime === 0)) {
    //   this.notify.error('working hours must be greater than 0 and less than  ' + this.APP_CONSTANT.MAX_WORKING_TIME / 60 + ' hours');
    //   this.isLoading = false
    //   return false;
    // }

    return true;
  }

  saveAndReset() {
    if (this.checkValid() === false) {
      return;
    }

    this.isLoading = true;
    this.myTimesheet.projectId = this.selectedProject.id; 
    this.myTimesheet.dateAt = this.data.dateAt
    if(this.myTimesheet.typeOfWork == APP_CONSTANT.EnumTypeOfWork.Overtime){
      this.doSaveAndReset();
      return;
    }
    let dateAt = moment(this.data.dateAt).format("YYYY-MM-DD");
    this.timesheetItemService.warningMyTimesheet(dateAt, this.myTimesheet.workingTime,this.myTimesheet.id).subscribe((rs) => {
      if (rs.success) {
        this.warningMyTimesheet = rs.result;
        if (this.warningMyTimesheet.isWarning) {
          this.showTimesheetWarning(this.warningMyTimesheet,this.myTimesheet,TypeAction.SAVEANDRESET);
        }
        else{
          this.doSaveAndReset();
        }
      }
    },() => this.isLoading = false);
  }

  doSaveAndReset(){
    if (this.myTimesheet.status == 3) {
      this.isLoading = true;
      this.timesheetItemService.saveAndReset(this.myTimesheet).subscribe((result) => {
        this.isLoading = false;
        this.notify.success(this.l('Save and Reset timesheetItem successfully'));
        this._dialogRef.close(result);
      }, (err) => { this.isLoading = false; });
    }
  }

  Save() {
    if (this.selectedProject.projectUserType != this.APP_CONSTANT.EnumUserType.Shadow) {
      this.strTargetUserWorkingTime = 0;
      this.myTimesheet.projectTargetUserId = null;
    } else {
      localStorage.setItem(this.DEFAULT_TARGET_USER_ID_FOR_PROJECT_ + this.selectedProject.id, '' + this.myTimesheet.projectTargetUserId);
    }

    if (this.checkValid() == false) {
      return;
    }

    if (!this.selectedProject.tasks.find(s => s.projectTaskId == this.myTimesheet.projectTaskId).billable) {
      this.myTimesheet.isCharged = false;
    }
    this.isLoading = true;
    this.myTimesheet.projectId = this.selectedProject.id; 
    this.myTimesheet.dateAt = this.data.dateAt
    if(this.myTimesheet.typeOfWork == APP_CONSTANT.EnumTypeOfWork.Overtime){
      this.doSave();
      return;
    }
    let dateAt = moment(this.data.dateAt).format("YYYY-MM-DD");
    this.timesheetItemService.warningMyTimesheet(dateAt, this.myTimesheet.workingTime,this.myTimesheet.id).subscribe((rs) => {
      if (rs.success) {
        this.warningMyTimesheet = rs.result;
        if (this.warningMyTimesheet.isWarning) {
          this.showTimesheetWarning(this.warningMyTimesheet,this.myTimesheet,TypeAction.SAVE);
        }
        else{
          this.doSave();
        }
      }
    },() => this.isLoading = false);
  }

  doSave(){
    this.isLoading = true;
    if (this.myTimesheet.id == null) {
      this.timesheetItemService.create(this.myTimesheet).subscribe((result) => {
        this.notify.success(this.l('Create timesheet Item successfully'));
        this._dialogRef.close(result);
      },
        () => this.isLoading = false);
    } else {
      this.timesheetItemService.update(this.myTimesheet).subscribe((result) => {
        this.isLoading = false;
        this.notify.success(this.l('Edit timesheet Item successfully'));
        this._dialogRef.close(result);
      }, (error => this.isLoading = false)
      );
    }
  }
  isShowCharged() {
    if (this.myTimesheet.typeOfWork == APP_CONSTANT.EnumTypeOfWork.Overtime && this.selectedProject && this.selectedProject.tasks) {
      for (let i = 0; i < this.selectedProject.tasks.length; i++) {
        if (this.myTimesheet.projectTaskId == this.selectedProject.tasks[i].projectTaskId) {
          return this.selectedProject.tasks[i].billable;
        }
      }
    }
    return false;
  }
  public mask = {
    guide: false,
    showMask: false,
    mask: [/\d/, '.', /\d/]
  };

  getProject() {
    this.filterRequest.filters = 'status==0';
    this.projectManagerService.filter(this.filterRequest).subscribe(res => {
      this.listProject = res['result'].map(vl => {
        return {
          value: vl.id,
          name: vl.name
        }
      })
    })
  }

  patchValueToForm(id: any): any {
    this.timesheetItemService.getOne(id).subscribe(res => {
      this.currentDate = convertDateToString(new Date(this.data.startDate).getTime() + res.result.dayOfWeek * 1000 * 24 * 60 * 60);
      res.result.workingTime = convertMinuteToHour(res.result.workingTime);
      this.formTimesheetItem.patchValue(res.result);
    })
  }

  showTimesheetWarning(warningMyTimesheet : WarningMyTimesheetDto,myTimesheet: MyTimeSheetDto,typeAction:TypeAction) {
    const dialogRef = this._dialog.open(TimesheetWarningDialogComponent, {
      disableClose: true,
      data: {
        warningMyTimesheet: warningMyTimesheet,
        myTimesheet: myTimesheet,
        typeAction:typeAction
      }
    })
    dialogRef.afterClosed().subscribe((result) => {
      this.isLoading = false
      if (result) {
        this._dialogRef.close(result);
      }
    }
    );
  }
}