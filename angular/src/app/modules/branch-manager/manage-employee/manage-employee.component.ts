import { Component, Injector, Input, OnInit, Output } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { BranchService } from '@app/service/api/branch.service';
import { ManageUserForBranchService } from '@app/service/api/manage-user-for-branch.service';
import { PositionDto } from '@app/service/api/model/position-dto';
import { PositionService } from '@app/service/api/position.service';
import { FilterDto, PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { ManageUserDto } from '../Dto/branch-manage-dto';
import { DetailParticipatingProjectsComponent } from './detail-participating-projects/detail-participating-projects.component';
import { MatDialog } from '@angular/material';
import { DateInfo } from '../date-filter/date-filter.component';
import {ESortProjectUserNumber, ESortType} from '@app/modules/branch-manager/manage-employee/enum/sort-project-user-number.enum';

@Component({
  selector: 'app-manage-employee',
  templateUrl: './manage-employee.component.html',
  styleUrls: ['./manage-employee.component.css']
})
export class ManageEmployeeComponent extends PagedListingComponentBase<any> implements OnInit {
  ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs = PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];
  public branchSearch: FormControl = new FormControl("")
  branchId;

  startDate: string;
  endDate: string;
  sortType = ESortType.NUMBER;
  sortProject: number = ESortProjectUserNumber.DOWN_PROJECT;
  sortNumberOfProject: number  = ESortProjectUserNumber.DOWN_NUMBER;
  sortProjectUserNumber = ESortProjectUserNumber;
  sortLevel: number = ESortProjectUserNumber.DOWN_LEVEL;
  currentComparision = ESortProjectUserNumber.DOWN_NUMBER;

  @Input() listPosition: PositionDto[];
  @Input() listPositionFilter: PositionDto[];
  public positionSearch: FormControl = new FormControl("")
  public positionId = -1;
  public filterItems: FilterDto[] = [];
  public users: ManageUserDto[];
  keyword;
  constructor(
    injector: Injector,
    private positionService: PositionService,
    private branchService: BranchService,
    private manageUserForBranchService:ManageUserForBranchService ,
    private _dialog: MatDialog,
  ) {
    super(injector);
    this.branchId = 0;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    })
    this.positionSearch.valueChanges.subscribe(() => {
      this.filterPosition();
    })
  }

  userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Internship' },
    { value: 2, label: 'Collaborator' }
  ];
 
  ngOnInit() {
  }
  filterPosition(): void{
    if(this.positionSearch.value){
      this.listPosition = this.listPositionFilter.filter(data => data.name.toLowerCase().includes(this.positionSearch.value.toLowerCase().trim()));
    }else{
      this.listPosition = this.listPositionFilter.slice();
    }
  }

  filterBranch(): void{
    if(this.branchSearch.value){
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    }else{
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void{
    if (this.keyword) {
      request.searchText = this.keyword.trim();
    }
    this.removeFilterItem();
    if (this.branchId != 0) {
      this.addFilterItem('branchId', this.toNumber(this.branchId));
    }
    if (this.positionId != null && this.positionId != -1) {
      this.addFilterItem('positionId', this.positionId);
    }
    request.filterItems = this.filterItems;
    this.manageUserForBranchService.getAllUserPagging(request, this.startDate, this.endDate, this.sortType, this.currentComparision)
    .pipe(
      finalize(() => {
        finishedCallback()
      })
    ).subscribe((rs: any) => {
      this.totalItems = rs.result.totalCount;
      if (rs.result == null || rs.result.items.length == 0) {
        this.users = []
      }else{
        this.users = rs.result.items;
        this.showPaging(rs.result, pageNumber);
        this.users.forEach(item => {
          item.hideProjectName = false;
        })
      }
    })
  }

  removeFilterItem(): void {
    this.filterItems = [];
  }

  toNumber(str: string): any {
    return +str;
  }

  addFilterItem(str, num): void {
    this.filterItems.push({ comparison: 0, propertyName: str, value: num });
  }

  searchOrFilter(): void{
    this.refresh();
  }

  clearSearchAndFilter(){
    this.keyword = '';
    this.positionId = -1;
    this.branchId = 0;
    this.searchOrFilter();
  }

  isShowSelectBranch(){
    return this.isGranted(
      PERMISSIONS_CONSTANT.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs
    )
  }

  getUserTypeCssClass(userType: number){
    switch (userType) {
      case 0: return 'bg-red';
      case 1: return 'bg-green';
      default: return 'bg-blue';
    }
  }

  togglePrivateNote(user){
    user.hideProjectName = !user.hideProjectName;
  }
  protected delete(entity: ManageUserDto): void {
    throw new Error('Method not implemented.');
  }

  defaullUserImage(sex: number){
    switch (sex) {
      case 0: return "assets/images/men.png";
      case 1: return "assets/images/women.png";
      default: return "assets/images/undefine.png";
    }
  }

  showProjectDetailDialog(user): void{
    let dialogRef = this._dialog.open(DetailParticipatingProjectsComponent, {
      minWidth: '450px',
      width: '800px',
      data: {
        user: user,
        startDate: this.startDate,
        endDate: this.endDate
      }
    })
    dialogRef.afterClosed().subscribe(() => {

    })
  }

  onDateSelected(dateInfo: DateInfo) {
    this.startDate=dateInfo.startDate;
    this.endDate=dateInfo.endDate;
    this.refresh();
  }
  toggleLevelSortOrder() {
    this.sortLevel = this.sortLevel === ESortProjectUserNumber.UP_LEVEL 
        ? ESortProjectUserNumber.DOWN_LEVEL 
        : ESortProjectUserNumber.UP_LEVEL;

    this.currentComparision = this.sortLevel;
    this.sortType = ESortType.LEVEL;
    this.refresh();
}
  toggleSortOrder(click: boolean) {
      if (click === true) {
          if (this.sortNumberOfProject === ESortProjectUserNumber.UP_NUMBER) {
              this.sortNumberOfProject = ESortProjectUserNumber.DOWN_NUMBER;
          } else {
              this.sortNumberOfProject = ESortProjectUserNumber.UP_NUMBER;
          }
          this.currentComparision = this.sortNumberOfProject;
          this.sortType = ESortType.NUMBER;
      } else  {
          if (this.sortProject === ESortProjectUserNumber.UP_PROJECT) {
              this.sortProject = ESortProjectUserNumber.DOWN_PROJECT;
          } else {
              this.sortProject = ESortProjectUserNumber.UP_PROJECT;
          }
          this.currentComparision = this.sortProject;
          this.sortType = ESortType.PROJECT;
      }
      this.refresh();
  }
}
