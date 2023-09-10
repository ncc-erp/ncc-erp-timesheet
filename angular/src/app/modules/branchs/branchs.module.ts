import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BranchsRoutingModule } from './branchs-routing.module';
import { BranchsComponent } from './branchs.component';
import { SharedModule } from '@shared/shared.module';
import { CreateEditBranchComponent } from './create-edit-branch/create-edit-branch.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';

@NgModule({
  declarations: [BranchsComponent, CreateEditBranchComponent],
  imports: [
    CommonModule,
    BranchsRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents: [
    CreateEditBranchComponent
  ],
  exports: [
    CreateEditBranchComponent
  ]
})
export class BranchsModule { }
