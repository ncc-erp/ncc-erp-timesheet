import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ListSubLevelDto, ReviewInternshipComponent } from './review-internship/review-internship.component';
import { finalize } from 'rxjs/operators';
import { FilterDto } from './../../../../shared/paged-listing-component-base';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { APP_CONSTANT } from '../../../constant/api.constants';
import { Component, OnInit, Injector } from '@angular/core';
import { ReviewService } from '@app/service/api/review.service';
import { AppConsts } from '@shared/AppConsts';
import { MatDialog } from '@angular/material';
import { ReviewDetailService } from '@app/service/api/review-detail.service';
import { UserService } from '@app/service/api/user.service';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { CreateReviewDetailComponent } from './create-review-detail/create-review-detail.component';
import { ViewHistoryService } from '@app/service/api/view-history.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { BranchService } from '@app/service/api/branch.service';
import { ConfirmSalaryInternshipComponent } from './confirm-salary-internship/confirm-salary-internship.component';
import { NewReviewInternshipComponent } from './new-review-internship/new-review-internship.component';
import { UpdateReviewerComponent } from './update-reviewer/update-reviewer.component';
import { NewHrVerifyInternshipComponent } from './new-hr-verify-internship/new-hr-verify-internship.component';
import { CreatePmNoteComponent } from './create-pm-note/create-pm-note.component';
import { CreateInterviewNoteComponent } from './create-interview-note/create-interview-note.component';
import { List, forEach } from 'lodash';
import { P } from '@angular/cdk/keycodes';
import * as _ from 'lodash';

@Component({
  selector: 'app-review-detail',
  templateUrl: './review-detail.component.html',
  styleUrls: ['./review-detail.component.css']
})
export class ReviewDetailComponent extends PagedListingComponentBase<ReviewDetailDto> implements OnInit {
  ReviewIntern_ReviewDetail_SendAllEmailsIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_SendAllEmailsIntern
  ReviewIntern_ReviewDetail_SendAllEmailsOffical = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_SendAllEmailsOffical
  ReviewIntern_ReviewDetail_SendAllToHRM = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_SendAllToHRM
  ReviewIntern_ReviewDetail_AddNew = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_AddNew
  ReviewIntern_ReviewDetail_Delete = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_Delete

  ReviewIntern_ReviewDetail_ReviewForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ReviewForOneIntern
  ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern
  ReviewIntern_ReviewDetail_SendEmailForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_SendEmailForOneIntern
  ReviewIntern_ReviewDetail_Update = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_Update
  ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern
  ReviewIntern_ReviewDetail_VerifyPmReviewedForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_VerifyPmReviewedForOneIntern
  ReviewIntern_ReviewDetail_CreatePMNote = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_CreatePMNote
  ReviewIntern_ReviewDetail_CreateInterviewNote = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_CreateInterviewNote

  ReviewIntern_ReviewDetail_AcceptPmReviewForAllIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_AcceptPmReviewForAllIntern
  ReviewIntern_ReviewDetail_UpdateDetailSubLevel = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_UpdateDetailSubLevel
  ReviewIntern_ReviewDetail_ApproveForOneIntern =PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ApproveForOneIntern
  ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern
  ReviewIntern_ReviewDetail_RejectForOneIntern= PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_RejectForOneIntern
  ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern
  ReviewIntern_ReviewDetail_UpdateStarToProject = PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_UpdateStarToProject
  ReviewIntern_ApproveAll = PERMISSIONS_CONSTANT.ReviewIntern_ApproveAll
  public branchList = Object.keys(this.APP_CONSTANT.BRANCH);
  public listStatus = Object.keys(this.APP_CONSTANT.ReviewStatus);
  public listReviewer : InfoReviewerDto[];
  public listLevel = Object.keys(this.APP_CONSTANT.LEVEL);
  public listHistoryLevel = Object.keys(this.APP_CONSTANT.HISTORYLEVEL);
  public listChangeLevel: any[] = this.APP_CONSTANT.CHANGE_LEVEL;
  public selectedBranch: number = InitParam.ALL;
  public selectedStatus: number = InitParam.ALLSTATUS;
  public selectedIntership: number = InitParam.ALL;
  public selectedReviewer: number;
  public selectedCurrentLevel: number = InitParam.ALL;
  public selectedNewLevel: number = InitParam.ALL;
  public selectedChangeLevel: number = InitParam.ALL;
  public isTableLoading = false;
  public isCheckToOffical = false;
  public updateAt: string;
  public listReviewIntern: ReviewDetailDto[] = [];
  //public listReviewIntern: ReviewDetailDto[];
  public listSubLevels : ListSubLevelDto[];
  public reviewId: number;
  public historyItem : string;
  public listInternship : InternshipDto[];
  public listReviewerFilter : InfoReviewerDto[];
  public serachItem : searchItemDto[] = [];
  public reviewYear: number;
  public reviewMonth: number;
  private listInternshipFilter : InternshipDto[];
  public listHistory : HistoryDto[] = [];
  public internshipSearch: FormControl = new FormControl("")
  public reviewerSearch: FormControl = new FormControl("")
  public headPmVerifyStatus: number = EnumReviewStatus.Reviewed
  public headPmRejectStatus: number = EnumReviewStatus.Rejected
  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter : BranchDto[];
  branchId;
  listInternshipMaxLevelMonths : InternshipMaxLevelMonths [] = [];
  sortStatus = true;
  listSelectedItem: SelectReviewInternDetailDto[] = [];
  selectedCheckboxCount: number = 0;
  allComplete: boolean = false;
  public saving: boolean = false;
  listHeadPmVerify: HeadPmVerifyDto[] = [];

  constructor(
    private route: ActivatedRoute,
    public _dialog: MatDialog,
    injector: Injector,
    private reviewDetailService : ReviewDetailService,
    private listReviewerService : UserService,
    private reviewService : ReviewService,
    private historyService : ViewHistoryService,
    private router:Router,
    private branchService: BranchService,

  ) { super(injector)
    this.branchId = 0;
    this.reviewerSearch.valueChanges.subscribe(() => {
      this.filterReviewer();
    });
    this.internshipSearch.valueChanges.subscribe(() => {
      this.filterInternship();
    });
    this.selectedReviewer = InitParam.ALL
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
  }

  ngOnInit() { 
    this.reviewId = Number(this.route.snapshot.queryParamMap.get("id"))
    this.reviewYear = Number(this.route.snapshot.queryParamMap.get("year"))
    this.reviewMonth = Number(this.route.snapshot.queryParamMap.get("month"))
    this.getAllPM()
    this.getAllSublevel();
    this.refresh();
    this.getListBranch();
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  filterReviewer(): void {
    if (this.reviewerSearch.value) {
      const temp: string = this.reviewerSearch.value.toLowerCase().trim();
      this.listReviewer = this.listReviewerFilter.filter(data => data.pmFullName.toLowerCase().includes(temp));
    } else {
      this.listReviewer = this.listReviewerFilter.slice();
    }
  }
  
  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    request.searchText = this.searchText;
    request.filterItems = [];
    // if (this.branchId != 0){
    //   request.filterItems.push({propertyName: 'branchId', comparison: 0, value: this.branchId} as FilterDto)
    // }
    if (this.selectedStatus != InitParam.ALLSTATUS){
      request.filterItems.push({propertyName: 'status', comparison: 0, value: this.selectedStatus} as FilterDto)
    }
    if (this.selectedReviewer > InitParam.ALL){
      request.filterItems.push({propertyName: 'reviewerId', comparison: 0, value: this.selectedReviewer} as FilterDto)
    }
    if (this.selectedReviewer == InitParam.NOREVIEWER){
      request.filterItems.push({propertyName: 'reviewerId', comparison: 0, value: null} as FilterDto)
    }
    if (this.selectedCurrentLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'currentLevel', comparison: 0, value: this.selectedCurrentLevel} as FilterDto)
    }
    if (this.selectedNewLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'newLevel', comparison: 0, value: this.selectedNewLevel} as FilterDto)
    }
    if (this.selectedIntership != InitParam.ALL){
      request.filterItems.push({propertyName: 'internshipId', comparison: 0, value: this.selectedIntership  } as FilterDto)
    }
    if(this.selectedChangeLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'levelChange', comparison: 0, value: this.selectedChangeLevel  } as FilterDto)
    }
    this.reviewDetailService
      .countMonthLevelMax()
      .subscribe(rs => {
        this.listInternshipMaxLevelMonths = rs.result as InternshipMaxLevelMonths []; 
        this.reviewDetailService
          .getAllPaging(request, this.reviewId, this.branchId)
          .pipe(finalize(() => {
            finishedCallback();
          }))
          // .subscribe((result: any) => {
          //   this.listReviewIntern = result.result.items;
          //   this.showPaging(result.result, pageNumber);
          .subscribe((rs: any) => {
            this.totalItems = rs.result.totalCount
            if (rs.result == null || rs.result.items.length == 0) {
              this.listReviewIntern = []
            }
            else {
              this.listReviewIntern = rs.result.items;
              this.showPaging(rs.result, pageNumber);
              this.listReviewIntern.forEach(item => {
                const internshipSuitable = this.listInternshipMaxLevelMonths.find(x => x.internshipId === item.internshipId);
                if (internshipSuitable) {
                    item.countMonthLevelMax = internshipSuitable.countMonthLevelMax;
                    item.maxLevel = internshipSuitable.maxLevel;  
                }
                item.history = false;
                item.hideNote = false;
                item.hidePrivateNote = false;
                item.more = false;
              })
            }
          });
      });
    }

    isShowBtnChotLuong(item:ReviewDetailDto){
      return this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern)
      && item.status >= 1 //Pending
      && item.newLevel > this.Level.Intern_3
    }

    isShowBtnEdit(item: ReviewDetailDto){
      return this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_Update) &&
                      this.checkStatus(item.status, 'edit')
  }
    isShowBtnChangeReviewer(item: ReviewDetailDto){
    return this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ChangeReviewer) &&
                    this.checkStatus(item.status, 'edit')
  }

    isShowBtnReview(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ReviewForOneIntern
      ) && this.checkStatus(item.status, 'review')
    }

  isShowBtnReviewByCapability(item: ReviewDetailDto) {
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern
      ) && this.checkStatus(item.status, 'review')
    }

    isShowBtnPmReview(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern
      ) && this.checkStatus(item.status, 'pmReview')
    }

    isShowBtnHrVerify(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_VerifyPmReviewedForOneIntern
      ) && this.checkStatus(item.status, 'hrVerify')
    }

    isShowBtnHeadPm(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_VerifyPmReviewedForOneIntern
      ) && this.checkStatus(item.status, 'headPm')
    }

    isShowBtnApprove(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ApproveForOneIntern
      ) && this.checkStatus(item.status, 'approve')
    }

    isShowBtnReject(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_RejectForOneIntern
      )
           && this.checkStatus(item.status, 'reject')
    }

    isShowBtnRejectSentEmail(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern
      )
           && this.checkStatus(item.status, 'rejectSentMail')
    }

    isShowSendEmail(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_SendEmailForOneIntern
      ) && this.checkStatus(item.status, 'sendEmail')
    }

    isShowSendToHrm(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern
      ) && this.checkStatus(item.status, 'update to HRM')
    }

    isShowDelete(item: ReviewDetailDto){
      return this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_Delete) &&
      this.checkStatus(item.status, 'delete') 
    }

    isShowCountMonthMaxLevel(item: ReviewDetailDto){
      return typeof item.newLevel === 'number' && item.newLevel <= 3;
    }

    isShowBtnPMNote(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_CreatePMNote
      ) && this.checkStatus(item.status, 'pmNote') 
    }

    isShowBtnInterviewNote(item: ReviewDetailDto){
      return this.isGranted(
        PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_CreateInterviewNote
      ) && this.checkStatus(item.status, 'interviewNote') 
    }

    protected delete(entity: any): void {

    }

    isShowPmNote(item: reviewInternCommentDto) {
      if (item && item.reviewInternNoteType !== undefined ) {
        return item.reviewInternNoteType === 0;
      }
      return false;
    }

    isShowInterviewNote(item: reviewInternCommentDto){
      if (item && item.reviewInternNoteType !== undefined ) {
        return item.reviewInternNoteType === 1;
      }
      return false;
    }


  getAllPM() {
    this.listReviewerService.getAllPM().subscribe(res => {
      this.listReviewer = res.result;
      this.listReviewerFilter = this.listReviewer;
      this.listReviewer.unshift({
        pmId: InitParam.ALL,
        pmFullName: "All",
        pmEmailAddress: ''
      },
      {
        pmId: InitParam.NOREVIEWER,
        pmFullName: "No Reviewer",
        pmEmailAddress: ''
      })
    });
  }

  getHistory(item){
    this.historyItem = '';
    this.historyService.getHistory(item.internshipId, this.reviewId).subscribe(res => {
      let count = 0;
      let listHistory = res.result;
      listHistory.forEach(history => {
        count++;
        let item = `<span> ${history.reviewerName} : ${this.getLevelForHistory(history.fromLevel)} ->
        ${this.getLevelForHistory(history.toLevel) || ''} ${history.historyYear} - ${history.historyMonth} </span>
        ${history.average ?`<strong>(${history.average}<i  class="fas fa-star ${this.getStarColorforHistory(history.average, true)}"></i>)</strong>` : ''}`
        + "<br>"
        this.historyItem+=item;
      });
      if(count>3){
        item.conditionHistory = true
      }else{
        item.conditionHistory = false;
      }
      item.listHistory = this.historyItem
    });
  }
  // ${history.rateStar ? `<strong>(${history.rateStar}<i  class="fas fa-star ${this.getStarColor(history.rateStar, true)}"></i>)</strong>` : ''}

  public getStarColor(star, isClass){
    switch(star){
      case 1:  case 2: return isClass ? 'col-grey' : '#9E9E9E'
      case 3: return isClass ? 'col-gold' : 'gold'
      case 4: return isClass ? 'col-orange' : '#FF9800'
      case 5: return isClass ? 'col-red' : '#F44336'
      default: return ''
    }

  }
  public getStarColorforHistory(average, isClass){
    if (average < 2.5) {
      return 'col-grey'
    }
    if (average < 3.5) {
      return 'col-yellow'
    }
    if (average < 4.5) {
      return 'col-orange'
    }
    else {
      return 'col-red'
    }

  }

  changeStatusHistory(item){
    item.history = true;
    this.getHistory(item)
  }

  changeMoreHistory(item){
    item.more = !item.more;
  }

  toggleHideNote(item){
    item.hideNote = !item.hideNote;
  }

  togglePrivateNote(item){
    item.hidePrivateNote = !item.hidePrivateNote;
  }

  backToReviewList(){
    this.router.navigate(['/app/main/review'])
  }

  refreshCurrentPage(){
    this.refresh()
  }

  filterInternship(): void {
    if (this.internshipSearch.value) {
      const temp: string = this.internshipSearch.value.toLowerCase().trim();
      this.listInternship = this.listInternshipFilter.filter(data => data.intershipName.toLowerCase().includes(temp));
    } else {
      this.listInternship = this.listInternshipFilter.slice();
    }
  }

  getNameById(id: any) {
    let checkId = this.listReviewer.find(res => res.pmId == id)
    if(checkId){
      return this.listReviewer.filter(res => res.pmId == id)[0].pmFullName
    }
  }

  getBranch(branchEnum) {
    for (const branch in APP_CONSTANT.BRANCH) {
      if (APP_CONSTANT.BRANCH[branch] == branchEnum) {
        return branch
      }
    }
  }

  getLevelById(levelEnum) {
    for (const level in APP_CONSTANT.LEVEL) {
      if (APP_CONSTANT.LEVEL[level] == levelEnum) {
        return level
      }
    }
  }

  getType(typeEnum){
    for (const type in APP_CONSTANT.TYPE) {
      if (APP_CONSTANT.TYPE[type] == typeEnum) {
        return type
      }
    }
  }

  getLevelForHistory(levelEnum) {
    for (const level in APP_CONSTANT.HISTORYLEVEL) {
      if (APP_CONSTANT.HISTORYLEVEL[level] == levelEnum) {
        return level
      }
    }
  }

  getStatusById(statusEnum) {
    for (const status in APP_CONSTANT.ReviewStatus) {
      if (APP_CONSTANT.ReviewStatus[status] == statusEnum) {
        return status
      }
    }
  }

  getAllSublevel(): void{
    this.reviewDetailService.getSubLevelToUpdate().subscribe(res => {
      this.listSubLevels = res.result;
    })
  }


  getSubLevelById(subLevel, newLevel){
    if(newLevel != null && this.listSubLevels && this.listSubLevels.length > 0){
      if(this.listSubLevels.length > 0){
        for(let i = 0 ; i < this.listSubLevels.length; i++){
          if(newLevel == this.listSubLevels[i].id)
          {
            let res = this.listSubLevels[i].subLevels;
            if (res && res.length) {
              for(let j = 0 ; j < res.length; j++){
                if(res[j].id == subLevel){
                  return res[j].name;
                }
              }
            }
          }
        }
      }
    }
    return ;
  }
  getAvatar(member) {
    if (member.internFullPath) {
      return member.internFullPath;
    }
    return "assets/images/undefine.png";
  }

  getAvatarReviewer(member) {
    if (member.reviewerAvatarFullPath) {
      return member.reviewerAvatarFullPath;
    }
    return "assets/images/undefine.png";
  }

  isShowSubLevel(item: ReviewDetailDto){
    return item.newLevel > this.Level.Intern_3 && this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ViewDetailSubLevel);
  }

  isShowLevel(item: ReviewDetailDto){
    return item.newLevel <= this.Level.Intern_3 || this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ViewDetailLevel);
  }

  isShowFullLuong(item: ReviewDetailDto){
    return item.isFullSalary && item.newLevel > this.Level.Intern_3 && this.isGranted(PERMISSIONS_CONSTANT.ReviewIntern_ReviewDetail_ViewFullSalary);
  }


  sendAllEmailsIntern(): void{
    this.isCheckToOffical = false;
    abp.message.confirm(
      "Send all Emails Intern for " + this.reviewMonth +"/"+ this.reviewYear +" ?",
      (result: boolean) => {
        if (result) {
          this.reviewService.sendAllEmailsIntern(this.reviewId, this.isCheckToOffical).subscribe(res => {
            const message = this.getMessageSendAll(res.result);
            if(res.result.successList.length){
              abp.message.success(message,`Kết quả`, true);
            }else {
              abp.message.error(message,`Kết quả`, true);
            }
            this.refresh()
          },()=>this.refresh());
        }
      }
    )
  }

  sendAllEmailsOffical(): void{
    this.isCheckToOffical = true;
    abp.message.confirm(
      "Send all Emails Intern to Offical for " + this.reviewMonth +"/"+ this.reviewYear +" ?",
      (result: boolean) => {
        if (result) {
          this.reviewService.sendAllEmailsIntern(this.reviewId, this.isCheckToOffical).subscribe(res => {
            const message = this.getMessageSendAll(res.result);
            if(res.result.successList.length){
              abp.message.success(message,`Kết quả`, true);
            }else {
              abp.message.error(message,`Kết quả`, true);
            }
            this.refresh()
          },()=>this.refresh());
        }
      }
    )
  }

  approveAllUser(): void{
    let request: PagedRequestDto = new PagedRequestDto();
    request.searchText = this.searchText;
    request.filterItems = [];
    if (this.selectedStatus != InitParam.ALLSTATUS){
      request.filterItems.push({propertyName: 'status', comparison: 0, value: this.selectedStatus} as FilterDto)
    }
    if (this.selectedReviewer > InitParam.ALL){
      request.filterItems.push({propertyName: 'reviewerId', comparison: 0, value: this.selectedReviewer} as FilterDto)
    }
    if (this.selectedReviewer == InitParam.NOREVIEWER){
      request.filterItems.push({propertyName: 'reviewerId', comparison: 0, value: null} as FilterDto)
    }
    if (this.selectedCurrentLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'currentLevel', comparison: 0, value: this.selectedCurrentLevel} as FilterDto)
    }
    if (this.selectedNewLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'newLevel', comparison: 0, value: this.selectedNewLevel} as FilterDto)
    }
    if (this.selectedIntership != InitParam.ALL){
      request.filterItems.push({propertyName: 'internshipId', comparison: 0, value: this.selectedIntership  } as FilterDto)
    }
    if(this.selectedChangeLevel != InitParam.ALL){
      request.filterItems.push({propertyName: 'levelChange', comparison: 0, value: this.selectedChangeLevel  } as FilterDto)
    }
    abp.message.confirm(
      "Approve all Reviewed Users for " + this.reviewMonth +"/"+ this.reviewYear +" ?",
      (result: boolean) => {
        if (result) {
          this.reviewService.sendAllApproves(request, this.reviewId, this.branchId).subscribe(res => {
            this.notify.success(this.l('Approve All Successfully'));
            this.refresh()
          },()=>this.refresh());
        }
      }
    )
  }

  sendAllEmailsHRM(): void{
    abp.message.confirm(
      "Send all Emails HRM for " + this.reviewMonth +"/"+ this.reviewYear +" ?",
      (result: boolean) => {
        if (result) {
          this.reviewService.sendAllEmailHRM(this.reviewId).subscribe(res => {
            this.notify.success(this.l('Send Email All HRM Successfully'));
            this.refresh()
          },()=>this.refresh());
        }
      }
    )
  }

  UpdateLevelHRMv2ForAll(): void{
    abp.message.confirm(
      "Send all Emails HRM for " + this.reviewMonth +"/"+ this.reviewYear +" ?",
      (result: boolean) => {
        if (result) {
          this.reviewService.UpdateLevelHRMv2ForAll(this.reviewId).subscribe(res => {
            this.notify.success(this.l('Send Email All HRM Successfully'));
            this.refresh()
          });
        }
      }
    )
  }

  UpdateLevelHRMv2ForOne(detailId:number): void{
    this.reviewService.UpdateLevelHRMv2ForOne(detailId).subscribe(res => {
      this.notify.success(this.l('Send Email All HRM Successfully'));
      this.refresh()
    });
  }

  //deleteReviewDetail(id : number) {
    deleteReviewDetailInternCapability(id : number) {
    abp.message.confirm(
      "Delete this record ?",
      (result: boolean) => {
        if (result) {
          //this.reviewDetailService.deleteReviewDetail(id).subscribe(res => {
            this.reviewDetailService.deleteReviewDetailInternCapability(id).subscribe(res => {
            this.notify.success(this.l('Delete Review Successfully'));
            this.refresh()
          },()=>this.refresh());
        }
      }
    )
  }

  reviewInternship(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(ReviewInternshipComponent, {
      disableClose : true,
      width : "600px",
      data : item
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Reviewed Complete');
        this.refresh()
      }
    });
  }
  newReviewInternship(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(NewReviewInternshipComponent, {
      disableClose : true,
      width : "900px",
      data : item
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Reviewed Complete');
        this.refresh()
      }
    });
  }
  confirmSalaryInternship(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(ConfirmSalaryInternshipComponent, {
      disableClose : true,
      width : "600px",
      data : item
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('confirm Complete');
        this.refresh()
      }
    });
  }

  updateToProject(){
    abp.message.confirm("Update to Project?",
    (result)=>{
      if(result){
        this.reviewService.UpdateStarProject(this.reviewId).subscribe(res =>{
          this.notify.success(this.l('Update Successfully'));
          this.refresh();
        })
      }
    })

  }

  updateReview(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(CreateReviewDetailComponent, {
      disableClose : true,
      width : "600px",
      data : item.id
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Updated Review');
        this.refresh()
      }
    });
  }

  createReviewDetail(): void{
    const dialogRef = this._dialog.open(CreateReviewDetailComponent, {
      disableClose : true,
      width : "600px",
    });
    dialogRef.afterClosed().subscribe(result => {
      this.isTableLoading = true;
      if(result) {
        abp.notify.success('Created Review');
        this.refresh();
        this.isTableLoading = false;
      }else{
        this.isTableLoading = false;
      }
    },()=>this.isTableLoading = false);
  }

  sendEmail(item : ReviewDetailDto):void{
    this.reviewDetailService.sendEmail(item.id).subscribe(res => {
      this.notify.success(this.l('Send Email Successfully'));
      this.refresh()
    });
  }

  approveReview(item : ReviewDetailDto):void{
    this.reviewDetailService.approve(item.id).subscribe(res => {
      this.notify.success(this.l('Approve Review Successfully'));
      this.refresh()
    });
  }

  rejectReview(item : ReviewDetailDto):void{
    this.reviewDetailService.reject(item.id).subscribe(res => {
      this.notify.success(this.l('Reject Review Successfully'));
      this.refresh()
    });
  }

  printReview(item : ReviewDetailDto):void{
    // this.reviewDetailService.sendEmail(item.id).subscribe(res => {
      window.print();
      // this.notify.success(this.l('Print Review Successfully'));
      // this.refresh()
    // });
  }

  sendEmailHRM(item : ReviewDetailDto):void{
    this.reviewDetailService.sendEmailHRM(item.id).subscribe(res => {
      this.notify.success(this.l('Send Email For HRM Successfully'));
      this.refresh()
    });
  }



  cancel(item) {
    item.isEdit = false;
  }
  checkStatus(status:number, action:string):boolean{
    switch(status){
      case 0: return action=="edit"|| action=="delete" || action == "pmReview" ? true :false
      case 1: return  action == "approve" || action == "reject" || action =="pmNote" || action == "interviewNote" ? true : false
      case 2: return action=="sendEmail" || action=="reject" || action == "print" ? true : false
      case -1: return action=="edit"|| action=="review" || action == "approve" ? true : false
      case 3: return action=="update to HRM"|| action=="rejectSentMail" || action=="print" ? true : false
      case 4: return action=="headPm" || action=="pmReview" ? true : false
      case 5: return action=="edit"|| action=="pmReview" ? true : false
      default: return false

    }
  }
  rejectSentMail(item:ReviewDetailDto){
    this.reviewDetailService.rejectSentMail(item.id).subscribe(rs=>{
      this.notify.success(this.l('Rejected'));
      this.refresh()
    })
  }

  getMessageSendAll(result: {successList: any[], failedList: any[]}){
    let successMessage = `
    <p style='color:red'>Error <b>${result.failedList.length}</b>${result.failedList.length > 1 ? " users" : " user"}</p>
    <div class='text-left' style='max-height: 300px; overflow: auto'>
    ${result.failedList.map(s => {return "<span>" + s.internEmail + ": " + s.message + "</span>"}).join("<br/>")}
    </div>
    <p style='color:#28a745'>Success <b>${result.successList.length} </b>${result.successList.length > 1 ? " users" : " user"}!</p>
    <div class='text-left' style='max-height: 300px; overflow: auto'>
    ${result.successList.map(s => "<span>" + s.internEmail + "</span>").join("<br/>")}
    </div>
    `;
    return successMessage;
  }
  updateReviewer(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(UpdateReviewerComponent, {
      disableClose : true,
      width : "600px",
      data : item.id
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Updated Reviewer');
        this.refresh()
      }
    });
  }

  refreshDataFilter(){
    this.getDataPage(1);
  }

  formatNoteContent(content){
    return content.replaceAll('\n', '<br>');
  }
  
  getLineReviewContentCount(note: string): number {
    const lineCount = note.split('\n').length;
    return lineCount;
}
  getLineReviewContentsCount(items: any[]): number {
    let lineCount = 0;
    items.forEach(item => {
        lineCount += this.getLineReviewContentCount(item.privateNote);
    });
    return lineCount;
  }

  createHrNote(item: ReviewDetailDto): void{
    const dialogRef = this._dialog.open(NewHrVerifyInternshipComponent, {
      disableClose : true,
      width : "750px",
      data: item,
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('HR Verify Complete');
        this.refresh()
      }
    });
  }

  headPmApproveOrReject (item: ReviewDetailDto, status: number): void{
    const headPmApprove = {
      reviewDetailId: item.id,
      status: status,
    }
    const messageSuccessfully: string = (headPmApprove.status == 1) ? "PM Approve Successfully" : "PM Reject Successfully" ;
    this.reviewDetailService.headPmApproveIntern(headPmApprove).subscribe(res => {
      this.notify.success(this.l(messageSuccessfully));
      this.refresh()
    });
  }

  getStatusCssClass(status: EnumReviewStatus){
    switch(status){
      case EnumReviewStatus.Draft:
        return 'label-default';
      case EnumReviewStatus.Reviewed:
        return 'label-success';
      case EnumReviewStatus.Approved:
        return 'label-primary';
      case EnumReviewStatus.SentEmail:
        return 'label-info';
      case EnumReviewStatus.PMReviewed:
        return 'label-warning';
      default:
        return 'label-danger'
    }
  }

  toggleSort(columnName) {
    if (this.sortStatus) {
      this.listReviewIntern = <ReviewDetailDto[]>_.orderBy(this.listReviewIntern, [columnName], ['asc']);
    } else {
      this.listReviewIntern = <ReviewDetailDto[]>_.orderBy(this.listReviewIntern, [columnName], ['desc']);
    }
    this.sortStatus = !this.sortStatus;
  }

  setAll(completed: boolean) {
    this.allComplete = completed;
    if (this.listReviewIntern == null) {
      return;
    }

    this.listSelectedItem = []; 

    this.listReviewIntern.forEach((reviewDetail) => {
      if (reviewDetail.status === 4) {
        reviewDetail.selected = completed;

        if (completed) {
          this.listSelectedItem.push({
            selected: completed,
            id: reviewDetail.id,
            status: reviewDetail.status,
          });
        }
      }
    });
    this.checkSelectedCheckbox();
  }

  handleSelectlistReviewInternItem(index, $event) {
    if (index != undefined && this.listReviewIntern[index]) {
      this.listReviewIntern[index].selected = $event.checked;
  
      if (!this.listSelectedItem) {
        this.listSelectedItem = [];
      }
  
      const itemIndex = this.listSelectedItem.findIndex(item => item.id === this.listReviewIntern[index].id);

      if ($event.checked) {
        if (itemIndex !== -1) {
            this.listSelectedItem[itemIndex].selected = $event.checked;
        } else {
            this.listSelectedItem.push({
                selected: $event.checked,
                id: this.listReviewIntern[index].id,
                status: this.listReviewIntern[index].status
            });
        }
      } else if(itemIndex !== -1){
          this.listSelectedItem.splice(itemIndex, 1);
      }
        this.updateAllComplete();
        this.checkSelectedCheckbox();
      }
  }

  checkSelectedCheckbox() {
    this.selectedCheckboxCount = this.listSelectedItem.filter(
      (x) => x.selected
    ).length;
  }

  updateAllComplete() {
    this.allComplete =
      this.listReviewIntern != null &&
      this.listReviewIntern.every((t) => this.getIsSelected(t));
  }

  
  getIsSelected(item) {
    return this.listSelectedItem.find((c) => c.id == item.id)
      ? this.listSelectedItem.find((c) => c.id == item.id).selected
      : item.selected;
  }

  someComplete(): boolean {
    if (this.listReviewIntern == null || this.listReviewIntern.length == 0) {
      return false;
    }
  
    return this.listReviewIntern.some((t) => this.getIsSelected(t)) && !this.allComplete;
  }

  hasSelectedItems(){
    return this.listReviewIntern.some(item => item.selected === true);
  }

  headPmAprroveOrRejectAll(status: number){
    if(this.selectedCheckboxCount === 0){
      abp.notify.error("Please select the listing record before performing this action");
    }
    else {
      this.saving = true;
      this.listHeadPmVerify = this.listSelectedItem.map(item => ({
        ReviewDetailId: item.id,
        Status: status
      }));
      
      const messageSuccessfully: string = (this.listHeadPmVerify[0].Status == 1) ? "PM Approve Successfully" : "PM Reject Successfully" ;
      this.reviewDetailService.headPmVerifyOrRejectAll(this.listHeadPmVerify).subscribe(
        (rs) => {
          abp.notify.success(messageSuccessfully);
          this.listSelectedItem = [];
          this.saving = false;
          this.refresh();
        }
      )
    }
  }

  createPMNote(item: ReviewDetailDto): void{
    const dialogRef = this._dialog.open(CreatePmNoteComponent, {
      disableClose : true,
      width : "750px",
      data: item,
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('PM/ Loren Note success');
        this.refresh()
      }
    });
  }
  createInterviewNote(item : ReviewDetailDto): void{
    const dialogRef = this._dialog.open(CreateInterviewNoteComponent, {
      disableClose : true,
      width : "750px",
      data: item,
    });
    dialogRef.afterClosed().subscribe(result => {
      if(result) {
        abp.notify.success('Interview Note success');
        this.refresh()
      }
    });
  }
}

export enum EnumReviewStatus {
  Draft = 0,
  Reviewed = 1,
  Approved = 2,
  SentEmail = 3,
  Rejected =-1,
  PMReviewed = 4,
  HRApproved = 5,
  ReOpen  = 6,
}

export enum InitParam{
  ALL = -1,
  ALLSTATUS = -3,
  NOREVIEWER = -2,
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
  type: number;
  salary: number;
  updatedAt: string;
  updatedName : string;
  internAvatar: string;
  internFullPath: string;
  reviewerAvatar : string;
  reviewerAvatarFullPath : string;
  conditionHistory : boolean;
  isFullSalary : boolean
  more : boolean;
  internEmail: string;
  updatedId : number;
  reviewerEmail : string;
  history : boolean;
  listHistory : string;
  hideNote : boolean;
  hidePrivateNote: boolean;
  reviewInternCommentDto: reviewInternCommentDto[];
  id?: number;
  countMonthLevelMax: number;
  maxLevel:number;
  selected?: boolean;
}

export class reviewInternCommentDto{

  reviewDetailId?: number;
  poteByUserId?: number;
  noteByUserName?: string;
  created?: Date;
  privateNote?: string;
  id?: number;
  reviewInternNoteType: number;
}

export class UpdateReviewDetailDto {
  reviewId: number;
  internName : string;
  internshipId: number;
  currentLevel: number;
  newLevel: number;
  note: string;
  status: number;
  reviewerId: number;
}

export class searchItemDto{
    propertyName: string;
    value: {};
    comparison: number;
}

export class InternshipDto {
  intershipId: number;
  intershipName: string;
}

export class HistoryDto{
  historyMonth: number;
  historyYear: number;
  fromLevel: number;
  toLevel: number;
  reviewerId: number;
  intershipId: number
}

export class InfoReviewerDto {
  pmId: number;
  pmFullName: string;
  pmEmailAddress: string;
}
function UpdateReviewDetailComponent(UpdateReviewDetailComponent: any, arg1: { disableClose: true; width: string; data: ReviewDetailDto; }) {
  throw new Error('Function not implemented.');
}
export class InternshipMaxLevelMonths {
  internshipId: number;  
  maxLevel: number;
  countMonthLevelMax: number;
}

export class SelectReviewInternDetailDto {
  id: number;
  selected?: boolean;
  status: number;
}

export class HeadPmVerifyDto {
  ReviewDetailId: number;
  Status: number;
}
