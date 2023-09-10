import { DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MAT_DATE_LOCALE } from '@angular/material';
import { SharedModule } from '@shared/shared.module';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { OffDayProjectForUserRoutingModule } from './off-day-project-for-user-routing.module';
import { OffDayProjectForUserComponent } from './off-day-project-for-user.component';
import { PopupComponent } from './popup/popup.component';
import { PopupModule } from './popup/popup.module';
import { ExportDataComponent } from './export-data/export-data.component';

@NgModule({
    declarations: [OffDayProjectForUserComponent, ExportDataComponent],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        NgxPaginationModule,
        NgxMatSelectSearchModule,
        DragDropModule,
        PopupModule,
        OffDayProjectForUserRoutingModule,
        CalendarModule.forRoot({
            provide: DateAdapter,
            useFactory: adapterFactory
        }),
    ],
    entryComponents: [PopupComponent,
    ExportDataComponent],
    providers: [
        { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
    ]
})
export class OffDayProjectForUserModule { }
