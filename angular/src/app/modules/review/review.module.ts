import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReviewRoutingModule } from './review-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ReviewComponent } from './review.component';
import { SharedModule } from '@shared/shared.module';
import { CreateReviewComponent } from './create-review/create-review.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { ReviewReportComponent } from './review-report/review-report.component';
import { AngularEditorModule } from '@kolkov/angular-editor';
import { NgxPaginationModule } from 'ngx-pagination';

@NgModule({
  declarations: [ReviewComponent, CreateReviewComponent, ReviewReportComponent],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    ReviewRoutingModule,
    NgxMatSelectSearchModule,
    AngularEditorModule,
    NgxPaginationModule
  ],
  entryComponents: [
    CreateReviewComponent,
    ReviewReportComponent
  ],
  exports: [
    
  ]
})
export class ReviewModule { }
