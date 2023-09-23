import { UserService } from '../../../../service/api/user.service';
import { ReviewDetailService } from '@app/service/api/review-detail.service';
import { AppComponentBase } from 'shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { Component, OnInit, Injector, Inject, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { NgxStarsComponent } from 'ngx-stars';
import { PERMISSIONS_CONSTANT } from '../../../../constant/permission.constant';

@Component({
  selector: 'app-review-internship',
  templateUrl: './review-internship.component.html',
  styleUrls: ['./review-internship.component.css']
})
export class ReviewInternshipComponent extends AppComponentBase implements OnInit {
  public disableSelect: boolean = true;
  public review = {} as ReviewDetailDto;
  public listReviewer: InfoReviewerDto[];
  public saveReview = {} as ReviewInternshipDto;
  public internshipSearch: FormControl = new FormControl("")
  public listLevel = Object.keys(this.APP_CONSTANT.LEVEL);
  listLevelFiltered = [];
  public listSubLevel: ListSubLevelDto[];
  public listSubLevels: ListSubLevelDto[] = [];
  public listType = Object.keys(this.APP_CONSTANT.TYPE);
  public saving = false;
  public active = true;
  private reviewId: number;
  private oldLevel: number;
  private oldSubLevel: number;
  ratingDisplay
  public ratingTooltip: string = `
  -  1 or 2 star: giữ nguyên level
  -  3, 4 or 5 star: tăng 1 level
  -  TTS pass phỏng vấn english với khách hàng được đặc cách lên chính thức: chọn 5 star và chọn new level tương ứng`
  public is5Star: boolean = false
  constructor(
    injector: Injector,
    private reviewDetailService: ReviewDetailService,
    private listReviewerService: UserService,
    public dialogRef: MatDialogRef<ReviewInternshipComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
  }
  @ViewChild(NgxStarsComponent)
  starsComponent: NgxStarsComponent;
  ngOnInit() {
    this.reviewId = this.data.id;
    this.getAllPM();
    this.getAllSublevel();
    this.getReviewDetail();


    if (this.data.rateStar == 5) {
      this.is5Star = true;
    }

  }




  onRatingSet(rating: number): void {
    this.review.rateStar = rating
    // if (rating == 5) {
    //   if (this.review.isFirst4M == true) {
    //     this.is5Star = true
    //     this.review.newLevel = this.review.currentLevel
    //     return
    //   }
    //   this.review.newLevel = this.review.currentLevel + 1
    //   this.is5Star = true
    // }
    // else {
    //   this.is5Star = false
    //   if (rating < 3 || (this.review.currentLevel == 3  && this.review.isFirst4M == true)) {
    //     this.review.newLevel = this.review.currentLevel
    //   }
    //   else if ( this.review.currentLevel < 6) {
    //     this.review.newLevel = this.review.currentLevel + 1
    //   }
    // }
    // // this.checkNewLevel(this.review.newLevel)
    // if (this.review.newLevel >= 4) {
    //   this.getNewSubLevel(this.review.newLevel);

    // }

  }
  getReviewDetail(): void {
    this.reviewDetailService.getReviewToUpdate(this.reviewId).subscribe(res => {
      this.review = res.result[0];
      if(this.review.isUpOfficial == false){
        this.listLevelFiltered = this.userLevels.slice(0, 4);
      }
      // this.review.isFirst4M = true
    });
  }



  getAllSublevel(): void {
    this.reviewDetailService.getSubLevelToUpdate().subscribe(res => {
      this.listSubLevels = res.result;
    })
  }

  getAllPM(): void {
    this.listReviewerService.getAllPM().subscribe(res => {
      this.listReviewer = res.result;
    });
  }

  checkNewLevel(review: any) {


    if (review.newLevel >= 4 && review.type == 1) {
      review.type = 2;
      review.isFullSalary = false;
    }
    if (review.newLevel < 4) {
      review.type = 1;
      review.isFullSalary = false;
    }
    if (review.newLevel >= 4) {
      if (review.newLevel == this.oldLevel) {
        this.getOldSubLevel(this.oldSubLevel);
      } else {
        this.getNewSubLevel(review.newLevel);
      }
    }
  }

  getNewSubLevel(level: number) {
    this.listSubLevel = this.listSubLevels.filter(sub => sub.id == level)
    this.review.subLevel = this.listSubLevel[0].subLevels[0].id;
  }

  getOldSubLevel(subLevel: number) {
    this.listSubLevel = this.listSubLevels.filter(sub => sub.id == this.review.newLevel)
    for (let i = 0; i < this.listSubLevel[0].subLevels.length; i++) {
      if (this.listSubLevel[0].subLevels[i].id == subLevel) {
        this.review.subLevel = this.listSubLevel[0].subLevels[i].id;
      }
    }
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  checkUpOffical(review: any){
    this.review.isUpOfficial = !this.review.isUpOfficial;
    if(review.isUpOfficial){
      this.review.newLevel = this.Level.FresherMinus;
      this.getOldSubLevel(this.review.newLevel);
    }else{
      this.listLevelFiltered = this.userLevels.slice(0, 4); 
      this.review.newLevel = this.review.newLevel >= this.Level.FresherMinus? this.Level.Intern_3:this.review.newLevel;
    }
  }

  saveReviewDetail() {
    this.saveReview = {
      newLevel: this.review.newLevel,
      note: this.review.note,
      type: this.review.type,
      subLevel: this.review.subLevel,
      isFullSalary: this.review.isFullSalary ? this.review.isFullSalary : false,
      isUpOfficial: this.review.isUpOfficial ? this.review.isUpOfficial : false,
      id: this.reviewId,
      rateStar: this.review.rateStar
    } as ReviewInternshipDto;

    if(this.review.isUpOfficial == true){
      if (this.review.newLevel >= 0 && this.review.rateStar) {
        this.reviewDetailService.saveReviewInternship(this.saveReview).subscribe(res => {
          this.dialogRef.close(this.review)
        });
      } else {
        this.notify.error(this.l("Bạn phải Rating mới tính là đã review!"));
      }
    }

    if(this.review.isUpOfficial == false){
      if (this.review.note && this.review.newLevel>=0 && this.review.rateStar) {
        this.reviewDetailService.saveReviewInternship(this.saveReview).subscribe(res => {
          this.dialogRef.close(this.review)
        });
      } else {
        this.notify.error(this.l("Bạn phải nhập Note và Rating mới tính là đã review!"));
      }
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
  isUpOfficial: boolean;
  isFullSalary: boolean;
  subLevel: number;
  subLevels: SubLevelDto;
  type: number;
  note: string;
  id?: number;
  rateStar: number;
  isFirst4M: boolean;
}

export class ReviewInternshipDto {
  newLevel: number;
  note: string;
  type: number;
  isFullSalary: boolean;
  subLevel?: number;
  id?: number;
}

export class ListSubLevelDto {
  id: number;
  name: string;
  subLevels: SubLevelDto[];
}

export class SubLevelDto {
  id: number;
  name: string;
  salary: number;
}

export class InfoReviewerDto {
  pmId: number;
  pmFullName: string;
  pmEmailAddress: string;
}


