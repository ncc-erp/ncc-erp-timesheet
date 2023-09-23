import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { ViewLeaveDayOfUserRoutingModule } from './view-leave-day-of-user-routing.module';
import { ViewLeaveDayOfUserComponent } from './view-leave-day-of-user.component';

@NgModule({
  declarations: [ViewLeaveDayOfUserComponent],
  imports: [
    ViewLeaveDayOfUserRoutingModule,
    CommonModule,
    SharedModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    }),
  ]
})
export class ViewLeaveDayOfUserModule { }
