import { MAT_DATE_LOCALE } from '@angular/material';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { TardinessLeaveEarlyComponent } from './tardiness-leave-early.component';
import { TardinessLeaveEarlyRoutingModule } from './tardiness-leave-early-routing.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { SelectedDateComponent } from './selected-date/selected-date.component';
@NgModule({
  declarations: [TardinessLeaveEarlyComponent, SelectedDateComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    TardinessLeaveEarlyRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
  ], entryComponents:[
    SelectedDateComponent
  ],
  providers:[
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class TardinessLeaveEarlyModule { }
