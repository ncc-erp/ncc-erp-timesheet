import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { OffDayProjectDetailComponent } from '../off-day-project-detail/off-day-project-detail.component';
import { OffDayProjectDetailModule } from '../off-day-project-detail/off-day-project-detail.module';
import { LeaveDayOfUserRoutingModule } from './leave-day-of-user-routing.module';
import { LeaveDayOfUserComponent } from './leave-day-of-user.component';
import { ConfirmAllRequestModule } from '../confirm-all-request/confirm-all-request.module';
import { ConfirmAllRequestComponent } from '../confirm-all-request/confirm-all-request.component';

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
    OffDayProjectDetailModule,
    ConfirmAllRequestModule
  ],

  entryComponents: [
    OffDayProjectDetailComponent,
    ConfirmAllRequestComponent
  ],
  exports: []
})
export class LeaveDayOfUserModule { }
