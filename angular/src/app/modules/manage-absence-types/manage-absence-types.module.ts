import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ManageAbsenceTypesRoutingModule } from './manage-absence-types-routing.module';
import { ManageAbsenceTypesComponent } from './manage-absence-types.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { NgxPaginationModule } from 'ngx-pagination';
import { ManageAbsenceTypesCreateComponent } from './manage-absence-types-edit/manage-absence-types-create.component';


@NgModule({
  declarations: [ManageAbsenceTypesComponent, ManageAbsenceTypesCreateComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    ManageAbsenceTypesRoutingModule
  ],
  entryComponents : [
    ManageAbsenceTypesCreateComponent
  ]
})
export class ManageAbsenceTypesModule { }
