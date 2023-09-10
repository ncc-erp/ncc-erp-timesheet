import { MAT_DATE_LOCALE } from '@angular/material';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { KomuTrackerComponent } from './komu-tracker.component';
import { KomuTrackerRoutingModule } from './komu-tracker-routing.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
@NgModule({
  declarations: [KomuTrackerComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    KomuTrackerRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
  ], entryComponents:[
  ],
  providers:[
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class KomuTrackerModule { }
