import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { OffDayRoutingModule } from './off-day-routing.module';
import { OffDayComponent } from './off-day.component';
import { FlatpickrModule } from 'angularx-flatpickr';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { CreateEditOffDayComponent } from './create-edit-off-day/create-edit-off-day.component';

@NgModule({
  declarations: [OffDayComponent, CreateEditOffDayComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
    OffDayRoutingModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    }),
    NgbModalModule,
    FlatpickrModule
  ],
  entryComponents: [
    CreateEditOffDayComponent
  ],
  exports: [
    
  ]
})
export class OffDayModule { }
