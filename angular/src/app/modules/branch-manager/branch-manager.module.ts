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
import { ProjectManagementComponent } from './project-management/project-management.component';
import { DateFilterComponent } from './date-filter/date-filter.component';
import {
    ProjectManagementMemberDetailComponent
} from '@app/modules/branch-manager/modal/project-management-modal/project-management-member-detail.component';
import {MatDialogModule} from '@node_modules/@angular/material';
import {ProjectTargetUserPipe} from '@shared/pipes/projectTargetUser.pipe';
import { DragDropModule } from '@angular/cdk/drag-drop';


@NgModule({
  declarations: [
      DateFilterComponent,
      ProjectTargetUserPipe,
      BranchManagerComponent,
      ManageEmployeeComponent,
      PopupCustomeTimeComponent,
      ProjectManagementComponent,
      PopupUpdateProjectComponent,
      DetailParticipatingProjectsComponent,
      ProjectManagementMemberDetailComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    BranchManagerRoutingModule,
    NgxMatSelectSearchModule,
    NgxPaginationModule,
    FormsModule,
    MatDialogModule,
    DragDropModule
  ],
  entryComponents: [
    DetailParticipatingProjectsComponent,
    PopupCustomeTimeComponent,
    PopupUpdateProjectComponent,
    DetailParticipatingProjectsComponent,
    ProjectManagementMemberDetailComponent,
  ]
})
export class BranchManagerModule { }
