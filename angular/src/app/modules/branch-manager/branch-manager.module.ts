import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { BranchManagerComponent } from './branch-manager.component';
import { BranchManagerRoutingModule } from './branch-manager-routing.module';
import { ManageEmployeeComponent } from './manage-employee/manage-employee.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { FormsModule } from '@angular/forms';
import { DetailParticipatingProjectsComponent } from './manage-employee/detail-participating-projects/detail-participating-projects.component';
import { PopupCustomeTimeComponent } from './manage-employee/detail-participating-projects/popup-custome-time/popup-custome-time.component';
import { PopupUpdateProjectComponent } from './manage-employee/detail-participating-projects/popup-update-project/popup-update-project.component';


@NgModule({
  declarations: [BranchManagerComponent, ManageEmployeeComponent, DetailParticipatingProjectsComponent, PopupCustomeTimeComponent, PopupUpdateProjectComponent],
  imports: [
    CommonModule,
    SharedModule,
    BranchManagerRoutingModule,
    NgxMatSelectSearchModule,
    NgxPaginationModule,
    FormsModule,
  ],
  entryComponents: [
    DetailParticipatingProjectsComponent,
    PopupCustomeTimeComponent,
    PopupUpdateProjectComponent,
  ]
})
export class BranchManagerModule { }