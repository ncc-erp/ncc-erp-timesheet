import { AppComponentBase } from '@shared/app-component-base';
import { Component, OnInit, Optional, Inject, Injector, Input } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { MyTimeSheetByWeekDto, ProjectIncludingTaskDto, MyTimeSheetDto } from '@app/service/api/model/common-DTO';

@Component({
  selector: 'app-create-edit-timesheetByWeek',
  templateUrl: './create-edit-timesheetByWeek.component.html',
  styleUrls: ['./create-edit-timesheetByWeek.component.css']
})
export class CreateEditTimesheetByWeekComponent extends AppComponentBase implements OnInit {
  myTimesheet = {} as MyTimeSheetByWeekDto;
  selectedProject = {} as ProjectIncludingTaskDto;
  projectIncludingTasks = [] as ProjectIncludingTaskDto[];

  constructor(
    private injector: Injector,
    private dialogRef: MatDialogRef<CreateEditTimesheetByWeekComponent>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
  }
 
  ngOnInit() {
    this.projectIncludingTasks = this.data.projectIncludingTasks;
    this.myTimesheet.typeOfWork = this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours;
  }
  onProjectchange() {
    if (this.selectedProject.tasks != null && this.selectedProject.tasks.length > 0) {
      this.myTimesheet.projectTaskId = this.selectedProject.tasks[0].projectTaskId;
    }
  }
  checkValid() : boolean{
    if( this.myTimesheet.projectTaskId == undefined || this.myTimesheet.projectTaskId <= 0 ){
      this.notify.error('You have to choose project and task');
      return false;
    }
    return true;
    }
  

  save() {
    if (this.checkValid() ==false  ){
      return ;
    }
    this.myTimesheet.projectName = this.selectedProject.projectName;
    this.myTimesheet.projectCode = this.selectedProject.projectCode;
    this.myTimesheet.taskName = this.selectedProject.tasks.find(s => s.projectTaskId == this.myTimesheet.projectTaskId).taskName;
    this.myTimesheet.customerName=this.selectedProject.customerName;
    this.dialogRef.close(this.myTimesheet);
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

}



