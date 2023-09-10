import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { Component, Inject, Injector, OnInit } from '@angular/core';
import { InternsInfoService } from '@app/service/api/interns-info.service';
import { GetInternsInfoDto } from '@app/service/api/model/interns-info-dto';
import { DateFilterType, DateSelectorEnum } from '@shared/AppEnums';
import { DateTimeSelector } from '@shared/date-selector-new/date-selector-new.component';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';
import { DATE_FILTER_TYPE } from '@shared/AppConsts';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { Time } from '@angular/common';
@Component({
  selector: 'app-interns-info',
  templateUrl: './interns-info.component.html',
  styleUrls: ['./interns-info.component.css']
})
export class InternsInfoComponent extends PagedListingComponentBase<any> implements OnInit {
  InternsInfo_View = PERMISSIONS_CONSTANT.InternsInfo_View;
  InternsInfo_ViewLevelIntern = PERMISSIONS_CONSTANT.InternsInfo_ViewLevelIntern
  protected list(request: GetInternsInfoDto, pageNumber: number, finishedCallback: Function): void {
    this.isLoading = true
    request.basicTrainerIds = this.selectedBasicTrainerIds ? this.selectedBasicTrainerIds : null;
    request.branchIds = this.selectedBranchIds;
    request.dateFilterType = this.dateFilterType;
    request.searchText = this.searchText;
    request.startDate = this.filter.startDate;
    request.endDate = this.filter.endDate;
    this.internsInfoService.getInternInfo(request)
    .subscribe(data => {
      if(data.result)
      {
        this.internsInfo = data.result.listInternInfo.items
        this.listMonth = data.result.listMonth
        this.showPaging(data.result.listInternInfo, pageNumber);
      }
      else
      {
        this.totalItems = 0
        this.internsInfo = []
        this.listMonth = []
      }
      this.isLoading = false
    });
  }
  protected delete(entity: any): void {
  }

  internsInfo: any[] = []
  searchText: string = ""
  listMonth: any[] = [];
  isLoading: boolean = false

  filter : GetInternsInfoDto = new GetInternsInfoDto();
  startDateDefault: Date = new Date(new Date().getFullYear(), 0, 1);
  endDateDefault: Date = new Date(new Date().getFullYear(), 11, 31);

  selectedBasicTrainerIds : number[] = []
  selectedBranchIds : number[] = []
  basicTraners: any[] = []
  branchIds: any[] = []

  localStorageBasicTraner = "interInfoSelectedBasicTrainerIds";
  localStorageBranch = "interInfoSelectedBranchIds";
  dateFilterTypeOptions = DATE_FILTER_TYPE

  dateFilterType = DateFilterType.OnBoardDate

  ALL_BASIC_TRAINERID = -1;
  ALL_BRANCH = -1;

  constructor( injector: Injector, private internsInfoService: InternsInfoService) {
    super(injector);
    this.setDefault();
  }

  ngOnInit() {
    // this.getAllBasicTraner();
    // this.getAllBranch();
    //this.refresh();
  }
  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];

  setDefault(){
    this.getLocalStorageBranch();
    this.getLocalStorageBasicTraner();
    this.setDefaultTime();
  }
  getLocalStorageBasicTraner(){
    this.internsInfoService.getAllBasicTraner()
      .subscribe(data => {
      this.basicTraners = data.result.map(item =>{
        return {id : item.id, name : item.fullName + ' - ' + item.emailAddress}
      });
      let localStorageData = this.getlocalStorageData(this.localStorageBasicTraner);

      localStorageData.split(",").forEach((value: string) => {
        if (this.basicTraners.some(item => item.id === Number.parseInt(value))) {
          this.selectedBasicTrainerIds.push(Number.parseInt(value));
        }
      });
        this.refresh();
        this.basicTraners.unshift({ id: -1, name: "No Trainer" })
    });

  }
  getLocalStorageBranch(){
    this.internsInfoService.getAllBranch()
    .subscribe(data => {
      this.branchIds = data.result.map(item =>{
        return {id : item.branchId, name : item.branchDisplayName}
      });
      let localStorageData = this.getlocalStorageData(this.localStorageBranch);

      localStorageData.split(",").forEach((value: string) => {
        if (this.branchIds.some(item => item.id === Number.parseInt(value))) {
          this.selectedBranchIds.push(Number.parseInt(value));
        }
      });
      this.refresh();
    });
  }
  getlocalStorageData(localStorageName: string){
    return localStorage.getItem(localStorageName);
  }
  setDefaultTime(){
    this.filter.startDate = moment(this.startDateDefault).format('YYYY-MM-DD');
    this.filter.endDate = moment(this.endDateDefault).format('YYYY-MM-DD');
  }
  getAllBasicTraner(){
    this.internsInfoService.getAllBasicTraner()
    .subscribe(data => {
      this.basicTraners = data.result.map(item =>{
        return {id : item.id, name : item.fullName + ' - ' + item.emailAddress}
      });
    });
  }
  getAllBranch(){
    this.internsInfoService.getAllBranch()
    .subscribe(data => {
      this.branchIds = data.result.map(item =>{
        return {id : item.branchId, name : item.branchDisplayName}
      });;
    });
  }
  searchIntern() {
    this.filter.searchText = this.searchText;
    this.getDataPage(1);
  }
  getClassWarningType(cellColor,hasReview){
    let className = "";
    if(hasReview==false){
      className = "not-start";
    }
    else{
      if (cellColor == this.APP_CONSTANT.CellColor.Begin) {
        className = "Begin";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.Staff) {
        className = "Staff";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.End){
        className = "EndHasRivew";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.BeginAndEnd){
        className = "BeginAndEnd";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.BeginHasRivew){
        className = "BeginHasRivew";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.EndHasRivew){
        className = "EndHasRivew";
      }
      else if (cellColor == this.APP_CONSTANT.CellColor.BeginAndStaff){
        className = "BeginAndStaff";
      }
      else{
        className = "Normal";
      }
    }
    return className;
  }
  getAvatarManager(member) {
    if (member) {
      return member;
    }
    return "assets/images/undefine.png";
  }
  getAvatar(member) {
    if (member) {
      return member;
    }
    return "assets/images/undefine.png";
  }

  public getStarColorByFloatRateStar(star) {
    if(star >= 1 && star < 3) {
      return   'col-grey'
    }
    else if (star >= 3 && star < 4) {
      return 'col-gold'
    }
    else if (star >= 4 && star < 5) {
      return  'col-orange'
    }
    else if (star == 5) {
      return  'col-red'
    }
    else {
      return ''
    }
  }

  onChangeBasicTrannerSelected(event) {
    this.selectedBasicTrainerIds = event;
    localStorage.setItem(this.localStorageBasicTraner, event.toString());
    this.getDataPage(1);
  }
  onChangeBranchSelected(event) {
    this.selectedBranchIds = event;
    localStorage.setItem(this.localStorageBranch, event.toString());
    this.getDataPage(1);
  }

  onOnboardChange(): void {
    this.getDataPage(1);
  }
  onDateChange(searchDate: DateTimeSelector): void {
    // if(searchDate.dateType == DateSelectorEnum.ALL)
    // {
    //   this.filter.startDate = null;
    //   this.filter.endDate = null;
    // }
    // else
    // {
      this.filter.startDate = moment(searchDate.fromDate).format('YYYY-MM-DD');
      this.filter.endDate = moment(searchDate.toDate).format('YYYY-MM-DD');
    // }
    this.getDataPage(1);
  }
  pageSizeChange(value)
  {
    this.pageSize = value;
    this.getDataPage(1);
  }
}

