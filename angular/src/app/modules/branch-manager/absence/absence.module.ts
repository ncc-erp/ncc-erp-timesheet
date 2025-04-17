import { MAT_DATE_LOCALE } from '@angular/material';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AbsenceComponent } from './absence.component';
import { AbsenceRoutingModule } from './absence-routing.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgxStarsModule } from 'ngx-stars';
import { CalendarModule } from 'angular-calendar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import {MatTableModule} from '@angular/material/table';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatMomentDateModule } from '@angular/material-moment-adapter';




@NgModule({
  declarations: [AbsenceComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    AbsenceRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
    NgxStarsModule,
    CalendarModule,
    MatDatepickerModule, 
    MatNativeDateModule,  
    MatFormFieldModule,   
    MatInputModule,
    MatTableModule,
    MatButtonToggleModule,
    MatTooltipModule,
    MatIconModule,
    MatButtonModule,
    MatPaginatorModule,
    MatMomentDateModule
  ], entryComponents: [
  ],
  exports: [
    AbsenceComponent
  ],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class AbsenceModule { }
