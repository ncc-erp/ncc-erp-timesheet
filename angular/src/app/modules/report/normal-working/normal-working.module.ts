import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule, MatIconModule, MatMenuModule, MatSelectModule } from '@node_modules/@angular/material';
import { SharedModule } from '@shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@node_modules/@angular/forms';
import { NgxPaginationModule } from '@node_modules/ngx-pagination';
import { NormalWorkingRoutingModule } from './normal-working-routing.module';
import { NormalWorkingComponent } from './normal-working.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';

@NgModule({
  declarations: [NormalWorkingComponent],
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatMenuModule,
    MatIconModule,
    SharedModule,
    FormsModule,
    NgxPaginationModule,
    SharedModule,
    ReactiveFormsModule,
    NormalWorkingRoutingModule,
    NgxMatSelectSearchModule
  ],
  entryComponents: [],
  exports: []
})
export class NormalWorkingModule {
}
