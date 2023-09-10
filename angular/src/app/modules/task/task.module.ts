import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskRoutingModule } from './task-routing.module';
import { TaskComponent } from './task.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { CreateEditTaskComponent } from './task-edit-customer/create-edit-task.component';

@NgModule({
  declarations: [TaskComponent, CreateEditTaskComponent],
  imports: [
    CommonModule,
    FormsModule,
    TaskRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
  ],
  entryComponents: [
    CreateEditTaskComponent
  ]
})
export class TaskModule { }
