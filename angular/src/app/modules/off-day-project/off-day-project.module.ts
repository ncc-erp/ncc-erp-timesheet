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
import { OffDayProjectDetailComponent } from './off-day-project-detail/off-day-project-detail.component';
import { OffDayProjectDetailModule } from './off-day-project-detail/off-day-project-detail.module';
import { OffDayProjectRoutingModule } from './off-day-project-routing.module';
import { OffDayProjectComponent } from './off-day-project.component';

@NgModule({
    declarations: [OffDayProjectComponent],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        NgxPaginationModule,
        CalendarModule.forRoot({
            provide: DateAdapter,
            useFactory: adapterFactory
        }),
        NgxMatSelectSearchModule,
        DragDropModule,
        OffDayProjectDetailModule,
        OffDayProjectRoutingModule,
    ],
    entryComponents: [OffDayProjectDetailComponent],
    providers: [
        { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
    ]
})
export class OffDayProjectModule { }
