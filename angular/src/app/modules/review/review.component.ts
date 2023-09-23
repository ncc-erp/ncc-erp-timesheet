import { ReviewReportComponent } from './review-report/review-report.component';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { Router } from '@angular/router';
import { Component, Injector, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { CreateReviewComponent } from './create-review/create-review.component';
import { ReviewService } from '@app/service/api/review.service';

@Component({
  selector: 'app-review',
  templateUrl: './review.component.html',
  styleUrls: ['./review.component.css']
})
export class ReviewComponent extends AppComponentBase implements OnInit {
  ReviewIntern_AddNewReviewByCapability = PERMISSIONS_CONSTANT.ReviewIntern_AddNewReviewByCapability
  ReviewIntern_Delete = PERMISSIONS_CONSTANT.ReviewIntern_Delete
  ReviewIntern_Active = PERMISSIONS_CONSTANT.ReviewIntern_Active
  ReviewIntern_DeActive = PERMISSIONS_CONSTANT.ReviewIntern_DeActive
  ReviewIntern_ReviewDetail_ViewAll = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ViewAll
  ReviewIntern_ViewAllReport = PERMISSIONS_CONSTANT.ReviewIntern_ViewAllReport
  currentMonth: number = new Date().getMonth() + 1;
  public reviewList: ReviewListDto[] = [];
  public reportList: ReportInternDto[] = [];
  public isTableLoading = false;

  constructor(
    private router: Router,
    public _dialog: MatDialog,
    private reviewService: ReviewService,
    injector: Injector,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.getAllData()
  }

  refresh() {
    this.getAllData()
  }

  viewReviewDetail(item): void {
    if (this.permission.isGranted(this.ReviewIntern_ReviewDetail_ViewAll)) {
      this.router.navigate(['/app/main/review-detail'], {
        queryParams: {
          id: item.id,
          year: item.year,
          month: item.month
        }
      })
    } else {
      this.notify.error(this.l("You don't have permission"));
      this.getAllData();
    }
  }
  newReviewInternCapability(): void {
    const dialogRef = this._dialog.open(CreateReviewComponent, {
      disableClose: true,
    });
    let message:string = ""
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (result.month) {
          this.isTableLoading = true;
          this.reviewService.createReviewInternCapability(result).subscribe(res => {
            if (res.result.length > 0) {
              message = res.result[0]
              abp.message.error(message, "Error", true);
              this.isTableLoading = false;
              return
            }
            else {
              abp.notify.success('Created New Review');
              this.getAllData();
              this.isTableLoading = false;
            }

            }, (error) => {
              this.isTableLoading = false
          })
        } else {
          var today = new Date();
          var time = {
            isActive: result.isActive,
            year: today.getFullYear(),
            month: (today.getMonth() + 1)
          }
          this.isTableLoading = true;
          this.reviewService.createReviewInternCapability(time).subscribe(res => {
            if (res.result.length > 0) {
              let message = res.result.join("<br/>")
              abp.message.error(message, "Error", true);
              this.isTableLoading = false;
              return
            } else {
              abp.notify.success('Created New Review');
              this.getAllData();
              this.isTableLoading = false;
            }
          },  (error) => {
            abp.message.error(error,"Error",true);
            this.isTableLoading = false
        })
          this.getAllData();
        }
      }
    });
  }

  changeActive(id: number): void {
    abp.message.confirm(
      "Active this record ?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          this.reviewService.changeActive(id).subscribe(res => {
            this.notify.success(this.l('Active Review Successfully'));
            this.getAllData();
            this.isTableLoading = false;
          }, () => this.isTableLoading = false);
        }
      }
    )
  }

  changeDeActive(id: number): void {
    abp.message.confirm(
      "DeActive this record ?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          this.reviewService.changeDeActive(id).subscribe(res => {
            this.notify.success(this.l('DeActive Review Successfully'));
            this.getAllData();
            this.isTableLoading = false;
          }, () => this.isTableLoading = false);
        }
      }
    )
  }

  getAllData() {
    this.reviewService.getListReview().subscribe(res => {
      this.reviewList = res.result;
    });
  }

  // getAllReport(){
  //   this.reviewService.getAllReport().subscribe((res) => {
  //     this.reportList = res.result.map((e) => {
  //       for(let i = 0 ; i <= 12 ; i++){
  //         if(e.listLevelForMonth[i] == -1) {
  //           e.listLevelForMonth[i] = "";
  //           continue;
  //         }
  //         for(let j = i + 1; j <= 12 ; j++){
  //           if(e.listLevelForMonth[j] == -1 || (e.listLevelForMonth[j] != -1 && e.listLevelForMonth[j] != e.listLevelForMonth[i])){
  //             e.listLevelForMonth[i] = this.mapNameLevel(e.listLevelForMonth[i], e.isOut, i);
  //             i = j-1;
  //             break;
  //           }
  //           else
  //             e.listLevelForMonth[j] = '';
  //         }
  //       }
  //       return e;
  //     });
  //   });
  //   $('#showReportIntern').modal('show');
  // }

  customClass(month, item) {
    if (month != this.currentMonth - 1)
      return '';
    else {
      if (item.isWarning)
        return 'bg-red';
    }
  }

  mapNameLevel(level, isOut, index) {
    let text = "";
    switch (level) {
      case 0:
        text = "Intern_0";
        break;
      case 1:
        text = "Intern_1";
        break;
      case 2:
        text = "Intern_2";
        break;
      case 3:
        text = "Intern_3";
        break;
      case 4:
        text = "FresherMinus";
        break;
    }
    if (isOut && index == this.currentMonth)
      text = "Nghỉ";
    return text;
  }
  //deleteListReview(id: number): void {
    deleteListReviewInternCapability(id: number): void {
    abp.message.confirm(
      "Delete this record ?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          //this.reviewService.delete(id).subscribe(res => {
            this.reviewService.deleteListReviewInternCapability(id).subscribe(res => {
            this.notify.success(this.l('Delete Review Successfully'));
            this.isTableLoading = false;
            this.getAllData();
          }, () => this.isTableLoading = false && this.getAllData());
        }
      }
    )
  }
  viewReport(reviewId) {
    this._dialog.open(ReviewReportComponent, {
      data:{
        reviewId: reviewId
      },
      width: '100vw',
      minHeight:'90vh',
      maxWidth: '90vw',
      panelClass: 'full-width-dialog'
    },
    )
  }
}

export class ReviewListDto {
  month: number;
  year: number;
  isActive: boolean;
  id?: number
}
export class ReportInternDto {
  reviewer: string;
  fullname: string;
  listLevelForMonth: any[];
  isWarning: boolean;
  isOut: boolean;
}
