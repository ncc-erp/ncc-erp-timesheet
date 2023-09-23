import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { DateAdapter } from '@angular/material/core';
import { CalendarModule } from 'angular-calendar';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateEditTimesheetItemComponent } from './create-edit-timesheet-item/create-edit-timesheet-item.component';
import { MyTimeSheetsComponent } from './mytimesheets.component';
import {  MyTimeSheetsRoutingModule  } from './mytimesheets-routing.module';
import{CreateEditTimesheetByWeekComponent} from'./create-edit-timesheetByWeek/create-edit-timesheetByWeek.component';
import { MytimesheetTardinessComponent } from './mytimesheet-tardiness/mytimesheet-tardiness.component';
import { TimesheetWarningDialogComponent } from './timesheet-warning-dialog/timesheet-warning-dialog.component';
import { MytimesheetNormalWorkingComponent } from './mytimesheet-normal-working/mytimesheet-normal-working.component';
@NgModule({
    declarations: [
      CreateEditTimesheetByWeekComponent,
      CreateEditTimesheetItemComponent,
      MyTimeSheetsComponent,
      MytimesheetTardinessComponent,
      TimesheetWarningDialogComponent,
      MytimesheetNormalWorkingComponent
    ],
    imports: [
      CommonModule,
      SharedModule,
      FormsModule,
      MyTimeSheetsRoutingModule,
      NgxMatSelectSearchModule,
      ReactiveFormsModule,
      CalendarModule.forRoot({
        provide: DateAdapter,
        useFactory: adapterFactory
    }),
    ],
    entryComponents: [
      CreateEditTimesheetItemComponent,
      CreateEditTimesheetByWeekComponent,
      TimesheetWarningDialogComponent
    ]
  })
  export class MyTimeSheetsModule { }