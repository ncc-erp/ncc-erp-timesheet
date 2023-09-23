import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ExportService } from '@app/service/export.service';
import { ReviewService } from '@app/service/api/review.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AppComponentBase } from 'shared/app-component-base';
import { Component, OnInit, Inject, Injector } from '@angular/core';
import { AbsenceRequestDto } from '@app/service/api/model/absence.dto';
import { AppConsts } from '@shared/AppConsts';
import * as FileSaver from 'file-saver';
import * as moment from 'moment';

@Component({
  selector: 'app-review-report',
  templateUrl: './review-report.component.html',
  styleUrls: ['./review-report.component.css']
})
export class ReviewReportComponent extends AppComponentBase {
  ReviewIntern_ExportReport = PERMISSIONS_CONSTANT.ReviewIntern_ExportReport
  reviewReport: any[] = []
  searchText: string = ""
  tempReport = []
  monthList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11,12];
  monthResult: any[] = [];
  listYear: number[] = []
  isLoading: boolean = false
  year: number;
  month: number;
  levelChangeStatus = {
    '': 'All' ,
    'level-not-change': 'Not change',
    'level-up': 'Level up'
  }
  level = ''
  isselectedMonth: boolean = false
  isCurrentInternOnly: boolean = true;
  isDisabled: boolean = false
  constructor(public _dialogRef: MatDialogRef<ReviewReportComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any, injector: Injector, private reviewService: ReviewService) {
    super(injector)
  }

  ngOnInit() {
    this.getYearList()
    this.getTimeDefault();
  }
  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];
  getTimeDefault() {
    const now = new Date();
    let timeDefault = moment().subtract(1, 'months');
    if(now.getDate() <= 15){
      timeDefault.subtract(1, 'months');
    }
    this.month = Number(timeDefault.format('MM'));
    this.year = Number(timeDefault.format('YYYY'));
    this.getReviewReport();
  }
  getMonthDefault() {
    let day =  new Date().getDate();
    let currentMonth = new Date().getMonth() + 1;
    if(day > 15){
      return currentMonth - 1;
    }
    else{
      return currentMonth - 2;
    }
  }
  getYearList() {
    let currentYear = new Date().getFullYear()
    for (let i = currentYear+2; i >= 2020; i--) {
      this.listYear.push(i)
    }
  }
  getReviewReport() {
    this.isLoading = true
    this.reviewService.getReport(this.year, this.month, this.level,this.isCurrentInternOnly, this.searchText).subscribe(data => {
      this.reviewReport = data.result.listInternLevel
      this.tempReport = data.result.listInternLevel
      this.monthResult = data.result.listMonth
      this.isLoading = false
    }, () => {
      this.isLoading = false
    })
  }
  searchIntern() {
    this.getReviewReport()
  }
  checkLevel(data: any[], index) {
    let level = -1
    data.forEach(item => {
      if (item.month == index + 1) {
        level = item.newLevel
      }
    })
    return level
  }
  // checkMonth(data: any[], index) {
  //   let a= false
  //   data.forEach(item => {
  //     if (item.month == index+1 && item.month== this.month) {
  //      a = item.month
  //      console.log("aaaa",a)
  //     }
  //   })
  //   return a
  // }

  checkReviewName(data: any[], index) {
    let reviewName: string = ""
    data.forEach(item => {
      if (item.month == index + 1) {
        reviewName = item.reviewerName
      }
    })
    return reviewName ? 'Reviewer: ' + reviewName : ''
  }
  checkLevelStatus(data, index) {
    let status = ""
    data.forEach(item => {
      if (item.month == index + 1) {
        if (item.isWarning == 1) {
          status = "warning-level"
        }
        if (item.isWarning == 2) {
          status = "fire"
        }
      }
    })
    return status
  }

  getClassWarningType(warningType,hasReview){
    let className = "";
    if(hasReview==false){
      className = "not-start";
    }
    else{
      if (warningType == 1) {
        className = "warning-level";
      }
      else if (warningType == 2) {
        className = "fire";
      }
      else if (warningType == 3) {
        className = "staff";
      }
      else{
        className = "";
      }
    }

    return className;
  }
  getMonth(data, index){
    data.forEach(item => {
      if (item.month == this.month) {
       return "a"
      }
    })
    return "b"
  }
  onStatusChange() {
    this.isselectedMonth = true
    this.getReviewReport()
  }
  onYearChange() {
    this.getReviewReport()
  }
  onMonthChange() {
    this.isselectedMonth = true
      this.getReviewReport()
  }
  onClearFilter(){
    this.month= new Date().getMonth() + 1;
    this.level = '';
    this.isselectedMonth= false
    this.getReviewReport()
  }
  onChangeCurrentInternOnly(event) {
    this.isCurrentInternOnly = event.checked;
    this.getReviewReport();
  }
  exportReport() {
    this.isDisabled = true;
    this.reviewService.ExportReportIntern(this.year, this.month, this.level,this.isCurrentInternOnly, this.searchText).subscribe(data => {
      const file = new Blob([this.s2ab(atob(data.result))], {
        type: "application/vnd.ms-excel;charset=utf-8"
      });
      FileSaver.saveAs(file, `Report review intern ${this.year}.xlsx`);
      this.isDisabled = false
    })

  }
  s2ab(s) {
    var buf = new ArrayBuffer(s.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
    return buf;
  }
}

