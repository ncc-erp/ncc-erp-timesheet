import { Component, Inject, Injector, OnInit } from '@angular/core';
import { InfoReviewerDto, InternshipDto, ReviewDetailDto } from '../review-detail.component';
import { FormControl } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { ActivatedRoute } from '@angular/router';
import { ReviewDetailService } from '@app/service/api/review-detail.service';
import { UserService } from '@app/service/api/user.service';
import { CreateReviewDetailComponent, CreateReviewDetailDto } from '../create-review-detail/create-review-detail.component';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-update-reviewer',
  templateUrl: './update-reviewer.component.html',
  styleUrls: ['./update-reviewer.component.css']
})
export class UpdateReviewerComponent extends AppComponentBase implements OnInit {
  
  public review = {} as ReviewDetailDto;
  public listInternship : InternshipDto[];
  public listReviewer: InfoReviewerDto[];
  public disableSelect : boolean;
  private reviewId : number;
  public reviewerSearch: FormControl = new FormControl("");
  public listLevel = Object.keys(this.APP_CONSTANT.LEVEL);
  private listReviewerFilter: InfoReviewerDto[];
  private reviewCreateId: number;
  public active = true;
  public title : string;

  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private reviewDetailService : ReviewDetailService,
    private listReviewerService : UserService,
    public dialogRef:MatDialogRef<UpdateReviewerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {

    super(injector);
    this.reviewerSearch.valueChanges.subscribe(() => {
      this.filterReviewer();

    });
  }
  
  ngOnInit() {
    this.reviewCreateId = Number(this.route.snapshot.queryParamMap.get("id"))
      this.disableSelect = true;
      this.title = "Change reviewer for: "
      this.reviewId = this.data
      this.reviewDetailService.getReviewToUpdate(this.reviewId).subscribe(res => {
        this.review = res.result[0];
      });
      this.listReviewerService.getAllPM().subscribe(res => {
        this.listReviewer = res.result;
        this.listReviewerFilter = this.listReviewer;
      });
  }

  public filterReviewer(): void {
    if (this.reviewerSearch.value) {
      const temp: string = this.reviewerSearch.value.toLowerCase().trim();
      this.listReviewer = this.listReviewerFilter.filter(data => data.pmFullName.toLowerCase().includes(temp));
    } else {
      this.listReviewer = this.listReviewerFilter.slice();
    }
  }

  saveReviewDetail() {
      let editedList = {
        reviewId: this.review.reviewId,
        internshipId: this.review.internshipId,
        currentLevel: this.review.currentLevel,
        newLevel: this.review.newLevel,
        note: this.review.note,
        status: this.review.status,
        reviewerId: this.review.reviewerId,
        id : this.reviewId
      } as CreateReviewDetailDto;
      this.reviewDetailService.changeReviewerOfDetail(editedList).subscribe(res => {
        this.dialogRef.close(this.review)
      });
  }
  
  close(res): void {
    this.dialogRef.close(res);
  }
}
