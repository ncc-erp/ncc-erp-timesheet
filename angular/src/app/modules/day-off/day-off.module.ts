import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { DayOffComponent } from './day-off.component';
import { DayOffRoutingModule } from './day-off-routing.module';
import { CreateEditDayOffComponent } from './create-edit-day-off/create-edit-day-off.component';

@NgModule({
  declarations: [DayOffComponent, CreateEditDayOffComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
    DayOffRoutingModule
  ],
  entryComponents: [
    CreateEditDayOffComponent
  ],
  exports: [
    
  ]
})
export class DayOffModule { }
