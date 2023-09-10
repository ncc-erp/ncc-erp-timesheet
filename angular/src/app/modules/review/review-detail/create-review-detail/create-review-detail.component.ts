import { UserService } from '../../../../service/api/user.service';
import { ReviewDetailService } from '@app/service/api/review-detail.service';
import { AppComponentBase } from 'shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-review-update',
  templateUrl: './create-review-detail.component.html',
  styleUrls: ['./create-review-detail.component.css']
})
export class CreateReviewDetailComponent extends AppComponentBase implements OnInit {
  public title : string;
  public disableSelect : boolean;
  public review = {} as ReviewDetailDto;
  public newReview = {} as CreateReviewDetailDto;
  public branchList = Object.keys(this.APP_CONSTANT.BRANCH);
  public listStatus = Object.keys(this.APP_CONSTANT.ReviewStatus);
  public listReviewer : InfoReviewerDto[];
  public listInternship : InternshipDto[];
  public internshipSearch: FormControl = new FormControl("")
  public listLevel = Object.keys(this.APP_CONSTANT.LEVEL);
  public saving = false;
  public active = true;
  private listInternshipFilter : InternshipDto[];
  private reviewId : number;
  private reviewCreateId : number;
  public reviewerSearch: FormControl = new FormControl("");
  private listReviewerFilter : InfoReviewerDto[];
  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private reviewDetailService : ReviewDetailService,
    private listReviewerService : UserService,
    public dialogRef:MatDialogRef<CreateReviewDetailComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
    this.internshipSearch.valueChanges.subscribe(() => {
      this.filterInternship();
    });
    this.reviewerSearch.valueChanges.subscribe(() => {
      this.filterReviewer();
    });
   }

  ngOnInit() {
    this.reviewCreateId = Number(this.route.snapshot.queryParamMap.get("id"))
    if(this.data){
      this.disableSelect = true;
      this.title = "Edit Review : "
      this.reviewId = this.data
      this.reviewDetailService.getReviewToUpdate(this.reviewId).subscribe(res => {
        this.review = res.result[0];
      });
      this.listReviewerService.getAllInternship().subscribe(res => {
        this.listInternship = res.result;
        this.listInternshipFilter = this.listInternship;
      });
    }else{
      this.disableSelect = false;
      this.title = "Create Review"
      this.reviewDetailService.getInternshipToReview(this.reviewCreateId).subscribe(res => {
        this.listInternship = res.result;
        this.listInternshipFilter = this.listInternship;
      });
    }
    this.listReviewerService.getAllPM().subscribe(res => {
      this.listReviewer = res.result;
      this.listReviewerFilter = this.listReviewer;
    });
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  changeCurrentLevel(id : number): void{
    this.review.currentLevel = this.listInternship.filter(data => data.internshipId==id)[0].currentLevel;
  }

  filterInternship(): void {
    if (this.internshipSearch.value) {
      const temp: string = this.internshipSearch.value.toLowerCase().trim();
      this.listInternship = this.listInternshipFilter.filter(data => data.internName.toLowerCase().includes(temp));
    } else {
      this.listInternship = this.listInternshipFilter.slice();
    }
  }

  saveReviewDetail() {
    if(this.data){
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
      this.reviewDetailService.saveReviewDetail(editedList).subscribe(res => {
        this.dialogRef.close(this.review)
      });
    }
    if(!this.data){
      let createList = {
        reviewId: this.reviewCreateId,
        internshipId: this.review.internshipId,
        currentLevel: this.review.currentLevel,
        newLevel: this.review.newLevel,
        note: this.review.note,
        status: this.review.status,
        reviewerId: this.review.reviewerId,
      } as CreateReviewDetailDto;
      //this.reviewDetailService.createReviewDetail(createList).subscribe(res => {
        this.reviewDetailService.createInternCapability(createList).subscribe(res => {
        this.dialogRef.close(this.review)
      });
    }
  }

  public filterReviewer(): void {
    if (this.reviewerSearch.value) {
      const temp: string = this.reviewerSearch.value.toLowerCase().trim();
      this.listReviewer = this.listReviewerFilter.filter(data => data.pmFullName.toLowerCase().includes(temp));
    } else {
      this.listReviewer = this.listReviewerFilter.slice();
    }
  }
}

export class ReviewDetailDto {
  reviewId: number;
  internName: string;
  internshipId: number;
  reviewerName: string;
  reviewerId: number;
  currentLevel: number;
  newLevel: number;
  status: number;
  branch: number;
  note: string;
  updateAt: string;
  avatarPath: string;
  avatarFullPath: string;
  emailIntership: string;
  id?: number
}
export class CreateReviewDetailDto{
  reviewId: number;
  internshipId: number;
  currentLevel: number;
  newLevel: number;
  note: string;
  status: number;
  reviewerId: number;
  id?: number
}

export class InfoReviewerDto {
  pmId: number;
  pmFullName: string;
  pmEmailAddress: string;
}

export class InternshipDto {
  internshipId: number;
  internName: string;
  currentLevel : number;
}

