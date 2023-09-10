import { ComplainReplyComponent } from './../complain-reply/complain-reply.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TardinessDetailComponent } from './tardiness-detail.component';
import { SharedModule } from '@shared/shared.module';
import { TardinessDetailRoutingModule } from './tardiness-detail-routing.module';
import { TardinessLeaveEarlyComponent } from '../tardiness-leave-early.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { TardinessLeaveEarlyModule } from '../tardiness-leave-early.module';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';

@NgModule({
  declarations: [TardinessDetailComponent, ComplainReplyComponent],
  imports: [
    CommonModule,
    SharedModule,
    TardinessDetailRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    NgxPaginationModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    }),
    NgxMatSelectSearchModule,
    TardinessLeaveEarlyModule
  ],

  entryComponents: [
    TardinessLeaveEarlyComponent,
    ComplainReplyComponent
  ],
  exports: []
})
export class TardinessDetailModule { }
