import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { CalendarModule, DateAdapter } from 'angular-calendar';
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
import { ComplainDialogComponent } from './mytimesheet-tardiness/complain-dialog/complain-dialog.component';
import { TimesheetConfirmationDialogComponent } from './mytimesheet-tardiness/timesheet-confirmation-dialog/timesheet-confirmation-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { TransactionHashDialogComponent } from './mytimesheet-tardiness/transaction-hash-dialog/transaction-hash-dialog.component';
import { TransactionSuccessDialogComponent } from './mytimesheet-tardiness/transaction-success-dialog/transaction-success-dialog.component';
import { TranferDialogComponent } from './mytimesheet-tardiness/tranfer-from-timesheet-dialog/tranfer-from-timesheet-dialog.component';
import { MatFormFieldModule } from '@angular/material/form-field';

@NgModule({
    declarations: [
      CreateEditTimesheetByWeekComponent,
      CreateEditTimesheetItemComponent,
      MyTimeSheetsComponent,
      MytimesheetTardinessComponent,
      TimesheetWarningDialogComponent,
      MytimesheetNormalWorkingComponent,
      ComplainDialogComponent,
      TimesheetConfirmationDialogComponent,
      TransactionHashDialogComponent,
      TranferDialogComponent,
      TransactionSuccessDialogComponent
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
    MatDialogModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatInputModule,
    MatSnackBarModule,
    MatFormFieldModule
    ],
    entryComponents: [
      CreateEditTimesheetItemComponent,
      CreateEditTimesheetByWeekComponent,
      TimesheetWarningDialogComponent,
      ComplainDialogComponent,
      TimesheetConfirmationDialogComponent,
      TransactionHashDialogComponent,
      TranferDialogComponent,
      TransactionSuccessDialogComponent
    ]
  })
  export class MyTimeSheetsModule { }