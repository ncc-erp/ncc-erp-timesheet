import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { TimesheetRoutingModule } from './timesheet-routing.module';
import { TimesheetComponent } from './timesheet.component';
import { DetailComponent } from './detail/detail.component';
import { FormsModule } from '@angular/forms';
import { TimesheetWarningComponent } from './timesheet-warning/timesheet-warning.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';


@NgModule({
  declarations: [TimesheetComponent, DetailComponent, TimesheetWarningComponent],
  imports: [
    CommonModule,
    TimesheetRoutingModule,
    SharedModule,
    FormsModule,
    NgxMatSelectSearchModule
  ],
  entryComponents:[DetailComponent,TimesheetWarningComponent]
})
export class TimesheetModule { }
