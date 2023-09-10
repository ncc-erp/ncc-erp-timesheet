import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditlogComponent } from './auditlog.component';
import { SharedModule } from '@shared/shared.module';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { AuditlogRoutingModule } from './auditlog-routing.module';

@NgModule({
  declarations: [AuditlogComponent],
  imports: [
    CommonModule,
    AuditlogRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents: [
  ],
  exports: [
  ]
})
export class AuditlogModule { }
