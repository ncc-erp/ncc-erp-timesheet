import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { MatDialog } from '@angular/material';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { TaskService } from '@app/service/api/task.service';
import { CreateEditTaskComponent } from './task-edit-customer/create-edit-task.component';
import * as _ from 'lodash';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';


@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.css'],
  animations: [appModuleAnimation()]
})
export class TaskComponent extends AppComponentBase implements OnInit {
  ADD_TASK = PERMISSIONS_CONSTANT.AddTask;
  EDIT_TASK = PERMISSIONS_CONSTANT.EditTask;
  DELETE_TASK = PERMISSIONS_CONSTANT.DeleteTask;
  CHANGE_STATUS_TASK = PERMISSIONS_CONSTANT.ChangeStatusTask;

  isTableLoading = false;
  tasks = [];
  mapTasks = [];
  searchText = '';
  filterItems = [];
  constructor(
    private taskService: TaskService,
    private _dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getAllData();
  }

  getAllData() {
    this.isTableLoading = true;
    this.taskService.getAll().subscribe(res => {
      this.tasks = res.result;
      this.mapTasks = this.buildData(this.tasks);
      this.isTableLoading = false;
    });
  }

  buildData(data: Array<any>) {
    return _(data)
      .groupBy(x => x.type)
      .map((value, key) => ({ name: key == '0' ? 'Common Task' : 'Other Task', items: value }))
      .value();
  }

  createTask(): void {
    let task = {} as TaskDto;
    this.showDialog(task);
  }

  editTask(task): void {
    this.showDialog(task);
  }


  showDialog(task: TaskDto): void {
    let item = { id: task.id, name: task.name, type: task.type } as TaskDto;
    const dialogRef = this._dialog.open(CreateEditTaskComponent, {
      disableClose : true,
      data: item
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.taskService.save(result).subscribe(res => {
          abp.notify.success('Created Task : ' + result.name);
          this.getAllData();
        })
      }
    });
  }

  archiveTask(task): void {
    abp.message.confirm(
      "Archive task: '" + task.name + "'?",
      (result: boolean) => {
        if (result) {
          this.taskService.archiveTask(task.id).subscribe(() => {
            abp.notify.success('Archive Task : ' + task.name);
            this.getAllData();
          });
        }
      })
  }

  unArchiveTask(task): void {
    abp.message.confirm(
      "Unarchive task: '" + task.name + "'?",
      (result: boolean) => {
        if (result) {
          this.taskService.unArchiveTask(task.id).subscribe(() => {
            abp.notify.success('Unarchive Task : ' + task.name);
            this.getAllData();
          });
        }
      })
  }

  deleteTask(task): void {
    abp.message.confirm(
      "Delete project: '" + task.name + "'?",
      (result: boolean) => {
        if (result) {
          this.taskService.delete(task.id).subscribe(res => {
            this.notify.success(this.l('Delete Task Successfully'));
            this.getAllData();
          });
        }
      })
  }

  searchTask() {
    if (!this.searchText) {
      this.mapTasks = this.buildData(this.tasks);
    }
    else {
      this.filterItems = this.tasks.filter(s => s.name.search(new RegExp(this.searchText, "ig")) > -1);
      this.mapTasks = this.buildData(this.filterItems);
    }
  }
}

export class TaskDto {
  id: number;
  name: string;
  type: TaskType;
  isDeleted: boolean;
}

export enum TaskType {
  CommonTask = 0,
  OtherTask = 1
}
