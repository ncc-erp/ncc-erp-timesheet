import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectManagerComponent } from './project-manager.component';
import { SharedModule } from '@shared/shared.module';
import { ProjectManagerRoutingModule } from './project-manager-routing.module';
import { FormsModule } from '@angular/forms';
import { CreateEditCustomerComponent } from '../customer/create-edit-customer/create-edit-customer.component';
import { CustomerModule } from '../customer/customer.module';
import { CreateProjectComponent } from './create-project/create-project.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { PopupComponent } from './project-detail/popup/popup.component';
import { MAT_DATE_LOCALE } from '@angular/material';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';

@NgModule({
  declarations: [ProjectManagerComponent, CreateProjectComponent, ProjectDetailComponent, PopupComponent],
  imports: [
    CommonModule,
    SharedModule,
    ProjectManagerRoutingModule,
    FormsModule,
    CustomerModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents:[CreateProjectComponent,CreateEditCustomerComponent,ProjectDetailComponent,PopupComponent],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class ProjectManagerModule { }
