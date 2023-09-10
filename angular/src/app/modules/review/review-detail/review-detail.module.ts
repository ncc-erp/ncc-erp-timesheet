import { CreateReviewDetailComponent } from './create-review-detail/create-review-detail.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from 'ngx-pagination';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewDetailRoutingModule } from './review-detail-routing.module';
import { ReviewDetailComponent } from './review-detail.component';
import { SharedModule } from '@shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ReviewInternshipComponent } from './review-internship/review-internship.component';
import { NgxStarsModule } from 'ngx-stars';
import { ConfirmSalaryInternshipComponent } from './confirm-salary-internship/confirm-salary-internship.component';
import { NgxCurrencyModule } from 'ngx-currency';
import { NewReviewInternshipComponent } from './new-review-internship/new-review-internship.component';
import { ViewGuidelineDialogComponent } from './new-review-internship/view-guideline-dialog/view-guideline-dialog.component';
import { UpdateReviewerComponent } from './update-reviewer/update-reviewer.component';


@NgModule({
  declarations: [ReviewDetailComponent, CreateReviewDetailComponent, ReviewInternshipComponent, ConfirmSalaryInternshipComponent,NewReviewInternshipComponent, ViewGuidelineDialogComponent, UpdateReviewerComponent ],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    ReviewDetailRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
    NgxStarsModule,
    NgxCurrencyModule,
  ],
  entryComponents: [
    CreateReviewDetailComponent,
    ReviewInternshipComponent,
    ConfirmSalaryInternshipComponent,
    NewReviewInternshipComponent,
    ViewGuidelineDialogComponent,
    UpdateReviewerComponent
  ]
})
export class ReviewDetailModule { }
