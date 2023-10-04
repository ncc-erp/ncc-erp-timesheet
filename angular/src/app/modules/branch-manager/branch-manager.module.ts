import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { BranchManagerComponent } from './branch-manager.component';
import { BranchManagerRoutingModule } from './branch-manager-routing.module';
import { ManageEmployeeComponent } from './manage-employee/manage-employee.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { FormsModule } from '@angular/forms';
import { ProjectManagementComponent } from './project-management/project-management.component';


@NgModule({
  declarations: [BranchManagerComponent, ManageEmployeeComponent, ProjectManagementComponent],
  imports: [
    CommonModule,
    SharedModule,
    BranchManagerRoutingModule,
    NgxMatSelectSearchModule,
    NgxPaginationModule,
    FormsModule,
  ]
})
export class BranchManagerModule { }