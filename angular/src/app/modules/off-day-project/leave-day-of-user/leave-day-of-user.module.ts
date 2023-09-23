import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { OffDayProjectDetailComponent } from '../off-day-project-detail/off-day-project-detail.component';
import { OffDayProjectDetailModule } from '../off-day-project-detail/off-day-project-detail.module';
import { LeaveDayOfUserRoutingModule } from './leave-day-of-user-routing.module';
import { LeaveDayOfUserComponent } from './leave-day-of-user.component';

@NgModule({
  declarations: [LeaveDayOfUserComponent],
  imports: [
    CommonModule,
    SharedModule,
    LeaveDayOfUserRoutingModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    }),
    OffDayProjectDetailModule
  ],

  entryComponents: [
    OffDayProjectDetailComponent
  ],
  exports: []
})
export class LeaveDayOfUserModule { }
