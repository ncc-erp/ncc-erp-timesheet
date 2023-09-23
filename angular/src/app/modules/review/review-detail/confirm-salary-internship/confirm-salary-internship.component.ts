import { Component, Inject, Injector, OnInit, ViewChild } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { ReviewDetailService } from "@app/service/api/review-detail.service";
import { UserService } from "@app/service/api/user.service";
import { AppComponentBase } from "@shared/app-component-base";
import { NgxStarsComponent } from "ngx-stars";


@Component({
  selector: 'app-confirm-salary-internship',
  templateUrl: './confirm-salary-internship.component.html',
  styleUrls: ['./confirm-salary-internship.component.css'],
})
export class ConfirmSalaryInternshipComponent extends AppComponentBase implements OnInit {

  public review = {} as ReviewDetailDto;
  public listReviewer: InfoReviewerDto[];
  public saveReview = {} as ReviewInternshipDto;
  public listLevel = Object.keys(this.APP_CONSTANT.LEVEL);
  public subLevel = {} as SubLevelDto;
  public listSubLevel: ListSubLevelDto[];
  public listSubLevels: ListSubLevelDto[] = [];
  public listType = Object.keys(this.APP_CONSTANT.TYPE);
  public active = true;
  ratingDisplay
  public ratingTooltip: string = `
  -  1 or 2 star: giữ nguyên level
  -  3, 4 or 5 star: tăng 1 level
  -  TTS pass phỏng vấn english với khách hàng được đặc cách lên chính thức: chọn 5 star và chọn new level tương ứng`
  public is5Star: boolean = false;
  private reviewId: number;
  private oldLevel: number;
  private oldSubLevel: number;
  public disableSelect: boolean = true;
  public saving = false;
  public listSubsalary: SubLevelDto[] = []
  constructor(
    injector: Injector,
    private reviewDetailService: ReviewDetailService,
    private listReviewerService: UserService,
    public dialogRef: MatDialogRef<ConfirmSalaryInternshipComponent>,
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
    this.getConfirmDueSalary();
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

  getConfirmDueSalary(): void {
    this.reviewDetailService.getUserConfirmSalalry(this.reviewId).subscribe(res => {
      this.review = res.result[0];
      if (this.review.newLevel >= 4) {
        this.oldLevel = this.review.newLevel;
        this.oldSubLevel = this.review.subLevel;
        this.getOldSubLevel(this.review.subLevel);
      }
      this.review.salary = this.review.salary;
      // this.review.isFirst4M = true
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

  checkNewSubLevel(review: any){
    if (review.newLevel >= 4){
      this.getNewSubLevel(review.subLevel);
    }
  }

  getAllSublevel(): void {
    this.reviewDetailService.getSubLevelToUpdate().subscribe(res => {
      this.listSubLevels = res.result;
      this.listSubsalary = this.listSubLevels.reduce((pre, curr) => {
        if(curr.subLevels && curr.subLevels.length) {
          pre.push(...curr.subLevels)
          return pre;  
        } else {
          return pre
        }
      }, [])
    })
  }

  getNewSubLevel(level: number) {
    this.listSubLevel = this.listSubLevels.filter(sub => sub.id == level)
    this.review.subLevel = this.listSubLevel[0].subLevels[0].id;
    this.review.salary = this.review.isFullSalary? 
                        this.listSubLevel[0].subLevels[0].salary:
                        this.listSubLevel[0].subLevels[0].salary * 85 / 100;
  }

  getOldSubLevel(subLevel: number) {
    this.listSubLevel = this.listSubLevels.filter(sub => sub.id == this.review.newLevel)
    for (let i = 0; i < this.listSubLevel[0].subLevels.length; i++) {
      if (this.listSubLevel[0].subLevels[i].id == subLevel) {
        this.review.subLevel = this.listSubLevel[0].subLevels[i].id;
        this.review.salary = this.review.isFullSalary? 
                            this.listSubLevel[0].subLevels[i].salary:
                            this.listSubLevel[0].subLevels[i].salary * 85 / 100;
      }
    }
  }

  changeFullSalary(){
    this.review.isFullSalary = !this.review.isFullSalary
    this.listSubLevel = this.listSubLevels.filter(sub => sub.id == this.review.newLevel)
    if(this.review.isFullSalary == false){
      for (let i = 0; i < this.listSubLevel[0].subLevels.length; i++) {
        if (this.listSubLevel[0].subLevels[i].id == this.review.subLevel) {
          this.review.salary = this.listSubLevel[0].subLevels[i].salary * 85 / 100;
        }
      }
    }else{
      for (let i = 0; i < this.listSubLevel[0].subLevels.length; i++) {
        if (this.listSubLevel[0].subLevels[i].id == this.review.subLevel) {
          this.review.salary = this.listSubLevel[0].subLevels[i].salary;
        }
      }
    }
  }

  changeSubLevel(subLevel: number) {
    const salary = this.listSubsalary.find(subLevelSalary => subLevelSalary.id == subLevel)
    this.review.salary = this.review.isFullSalary ? salary.salary:salary.salary * 85 / 100;
  }

  getAllPM(): void {
    this.listReviewerService.getAllPM().subscribe(res => {
      this.listReviewer = res.result;
    });
  }

  close(res): void {
    this.dialogRef.close(res);
  }


  saveConfirmDetail() {
    this.saveReview = {
      newLevel: this.review.newLevel,
      note: this.review.note,
      type: this.review.type,
      subLevel: this.review.subLevel,
      isFullSalary: this.review.isFullSalary ? this.review.isFullSalary : false,
      id: this.reviewId,
      rateStar: this.review.rateStar,
      salary: this.review.salary
    } as ReviewInternshipDto;

    if (this.review.note && this.review.newLevel >= 0) {
      this.reviewDetailService.saveConfirmDueSalary(this.saveReview).subscribe(res => {
        this.dialogRef.close(this.review)
      });
    } else {
      this.notify.error(this.l("Bạn phải nhập Note mới tính là đã chốt lương!"));
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
  isFullSalary: boolean;
  subLevel: number;
  subLevels: SubLevelDto;
  type: number;
  note: string;
  id?: number;
  rateStar: number;
  isFirst4M: boolean;
  salary: number;
}

export class ReviewInternshipDto {
  newLevel: number;
  salary: number;
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
