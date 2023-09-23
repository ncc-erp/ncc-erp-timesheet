import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {AbsenceDayRoutingModule} from './absence-day-routing.module';
import {AbsenceDayComponent} from '@app/modules/absence-day/absence-day.component';
import {SharedModule} from '@shared/shared.module';
import {FormsModule, ReactiveFormsModule} from '@node_modules/@angular/forms';
import {NgxPaginationModule} from '@node_modules/ngx-pagination';
import {NgxMatSelectSearchModule} from '@node_modules/ngx-mat-select-search';
import {CalendarModule, DateAdapter} from '@node_modules/angular-calendar';
import {adapterFactory} from '@node_modules/angular-calendar/date-adapters/date-fns';
import {NgbModalModule, NgbModule} from '@node_modules/@ng-bootstrap/ng-bootstrap';
import {FlatpickrModule} from '@node_modules/angularx-flatpickr';
import {SubmitAbsenseDayComponent} from './submit-absense-day/submit-absense-day.component';
import { ShowDetailDialogComponent } from './show-detail-dialog/show-detail-dialog.component';
import { CustomAbsenceTimeDialogComponent } from './custom-absence-time-dialog/custom-absence-time-dialog.component';
import { TardinessLeaveEarlyDialogComponent } from './tardiness-leave-early-dialog/tardiness-leave-early-dialog.component';
import { DetailRequestComponent } from './detail-request/detail-request.component';

@NgModule({
    declarations: [AbsenceDayComponent, SubmitAbsenseDayComponent, ShowDetailDialogComponent,CustomAbsenceTimeDialogComponent, TardinessLeaveEarlyDialogComponent, DetailRequestComponent],
    imports: [
        AbsenceDayRoutingModule,
        CommonModule,
        SharedModule,
        ReactiveFormsModule,
        NgxPaginationModule,
        FormsModule,
        NgxMatSelectSearchModule,
        CalendarModule.forRoot({
            provide: DateAdapter,
            useFactory: adapterFactory
        }),
        NgbModalModule,
        FlatpickrModule,
        NgbModule
    ],
    entryComponents: [
      SubmitAbsenseDayComponent,
      ShowDetailDialogComponent,
      CustomAbsenceTimeDialogComponent,
      TardinessLeaveEarlyDialogComponent,
      DetailRequestComponent
    ],
    exports: []
})
export class AbsenceDayModule {
}
