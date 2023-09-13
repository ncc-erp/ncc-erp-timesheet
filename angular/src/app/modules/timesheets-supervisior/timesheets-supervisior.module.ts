import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimesheetsSupervisiorComponent } from './timesheets-supervisior.component';
import { TimesheetsSupervisiorRoutingModule } from './timesheets-supervisior-routing.module';
import { SharedModule } from '@shared/shared.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';


@NgModule({
  declarations: [TimesheetsSupervisiorComponent],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    TimesheetsSupervisiorRoutingModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents: []
})
export class TimesheetsSupervisiorModule { }
