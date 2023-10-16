import { Component, Inject, Injector, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { AppComponentBase } from '@shared/app-component-base';
import { ManageUserProjectForBranchService } from '@app/service/api/manage-user-project-for-branch.service';
import { convertMinuteToHour } from '@shared/common-time';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material';
import * as moment from 'moment';
import { PopupCustomeTimeComponent } from './popup-custome-time/popup-custome-time.component';
import { PopupUpdateProjectComponent } from './popup-update-project/popup-update-project.component';
import { ProjectListManagement } from '../../Dto/branch-manage-dto';

@Component({
  selector: 'app-detail-participating-projects',
  templateUrl: './detail-participating-projects.component.html',
  styleUrls: ['./detail-participating-projects.component.css']
})
export class DetailParticipatingProjectsComponent extends AppComponentBase implements OnInit {
  public viewChange = new FormControl(this.APP_CONSTANT.TypeViewHomePage.Week);
  public projectList: ProjectListManagement[] = [];
  private activeView: number = 0;
  distanceFromAndToDate = '';
  typeDate: any;
  public totalWorkingHours: string;
  public valueOfUserType = [
    { value: 0, label: 'Member' },
    { value: 1, label: 'Expose' },
    { value: 2, label: 'Shadow' }
  ];
  public showInactiveProject: boolean = false;
  userId: number;
  fromDate: any;
  toDate: any;

  constructor(
    injector: Injector,
    private _dialogRef: MatDialogRef<DetailParticipatingProjectsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private _dialog: MatDialog,
    private manageUserProjectForBranchService: ManageUserProjectForBranchService,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.userId = this.data.id;
    this.changeView(true);
  }

  getData(userId, fromDate, toDate){
    this.manageUserProjectForBranchService
    .getAllValueOfUserInProjectByUserId(this.userId, fromDate, toDate)
    .subscribe(res => {
      this.projectList = res.result.getAllValueOfUserInProjectByUserIdDtos;
      this.totalWorkingHours = convertMinuteToHour(res.result.totalWorkingHours);
    })
  }

  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }

  changeView(reset?: boolean, fDate?: any, tDate?: any){
    if (reset) {
      this.activeView = 0;
    }
    let fromDate, toDate;
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Week) {
      fromDate = moment().startOf('isoWeek').add(this.activeView, 'w');
      toDate = moment(fromDate).endOf('isoWeek');
      this.typeDate = 'Week';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Month) {
      fromDate = moment().startOf('M').add(this.activeView, 'M');
      toDate = moment(fromDate).endOf('M');
      this.typeDate = 'Month';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Quater) {
      fromDate = moment().startOf('Q').add(this.activeView, 'Q');
      toDate = moment(fromDate).endOf('Q');
      this.typeDate = 'Quater';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Year) {
      fromDate = moment().startOf('y').add(this.activeView, 'y');
      toDate = moment(fromDate).endOf('y');
      this.typeDate = 'Years';
    }
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.AllTime) {
      fromDate = '';
      toDate = '';
      this.distanceFromAndToDate = 'All Time';
    }
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      fromDate = '';
      toDate = '';
      if (!reset && fDate && tDate) {
        if (fDate && tDate) {
          fromDate = fDate.format('DD MMM YYYY');
          toDate = tDate.format('DD MMM YYYY');
        }
        this.setFromAndToDate(fromDate, toDate);
        this.getData(this.userId, fromDate, toDate);
        this.distanceFromAndToDate = fromDate + '  -  ' + toDate
      }else {
        this.distanceFromAndToDate = 'Custom Time';
      }
    }

    if (fromDate != '' && toDate != '') {
      let fDate = '', tDate = '';
      let list = [];
      list[0] = { value: fromDate.isSame(tDate, 'year'), type: 'YYYY'};
      list[1] = { value: fromDate.isSame(tDate, 'month'), type: 'MMM'};
      list[2] = { value: fromDate.isSame(tDate, 'day'), type: 'D'};
      list.map(value => {
        if (value.value) {
          tDate = toDate.format(value.type) + ' ' + tDate;
        }else{
          fDate = fromDate.format(value.type) + ' ' + fDate;
          tDate = toDate.format(value.type) + ' ' + tDate;
        }
      })
      this.distanceFromAndToDate = this.typeDate + ': ' + fDate + ' - ' + tDate;
    }
    if (this.viewChange.value != this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      fromDate = fromDate == '' ? '' : fromDate.format('YYYY-MM-DD');
      toDate = toDate == '' ? '' : toDate.format('YYYY-MM-DD');
      this.getData(this.userId, fromDate, toDate);
      this.setFromAndToDate(fromDate, toDate);
    }
  }

  nextOrPre(title: any){
    if (this.viewChange.value == this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      return
    }
    if (title == 'pre') {
      this.activeView--;
    }
    if (title == 'next') {
      this.activeView++;
    }
    this.changeView();
  }

  showPopup(): void{
    let popup = this._dialog.open(PopupCustomeTimeComponent);
    popup.afterClosed().subscribe(result => {
      if (result != undefined) {
        if (result.result) {
          this.changeView(false, result.data.fromDateCustomTime, result.data.toDateCustomTime);
        }
      }
    });
  }

  updateShadowPercentage(project){
    let item = Object.assign(JSON.parse(JSON.stringify(project)))
    let data = {
      project: item,
      userId: this.userId
    }
    const dialogRef = this._dialog.open(PopupUpdateProjectComponent, {
      disableClose : true,
      width : "450px",
      data : data
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Updated Project Success');
        this.changeView(true)
      }
    });
  }
}
