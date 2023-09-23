import { MAT_DATE_LOCALE } from '@angular/material';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { InternsInfoComponent } from './interns-info.component';
import { InternsInfoRoutingModule } from './interns-info-routing.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxStarsModule } from 'ngx-stars';

@NgModule({
  declarations: [InternsInfoComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    InternsInfoRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
    NgxStarsModule
  ], entryComponents:[
  ],
  providers:[
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class InternsInfoModule { }
