import { OverTimeRoutingModule } from './over-time-routing.module';
import { OverTimeComponent } from './over-time.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import {NgxMatSelectSearchModule} from '@node_modules/ngx-mat-select-search';
@NgModule({
  declarations: [OverTimeComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    OverTimeRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule
  ],
  entryComponents: [
    
  ],
  exports: [
    
  ]
})
export class OverTimeModule { }
