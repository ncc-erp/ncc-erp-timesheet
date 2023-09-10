import { AppComponentBase } from '@shared/app-component-base';
import {  MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { TaskDto } from '../task.component';

@Component({
  selector: 'app-create-edit-task',
  templateUrl: './create-edit-task.component.html',
  styleUrls: ['./create-edit-task.component.css']
})
export class CreateEditTaskComponent extends AppComponentBase implements OnInit {
  task = {} as TaskDto;
  saving = false;
  title: string;
  active = true;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
  }

  ngOnInit() {
    this.task = this.data;
    this.title = this.task.id != null ? 'Edit Task: ' : 'New Task';
    if (!this.task.id) {
      this.task.type = this.APP_CONSTANT.EnumTaskType.Commontask;
    }
  }
}

