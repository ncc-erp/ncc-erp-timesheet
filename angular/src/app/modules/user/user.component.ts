import { AppConsts } from './../../../shared/AppConsts';
import { Component, OnInit, Injector, ViewChild } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto, FilterDto } from '@shared/paged-listing-component-base';
import { UserService } from '@app/service/api/user.service';
import { finalize } from 'rxjs/operators';
import { MatDialog, MatMenuTrigger } from '@angular/material';
import { CreateUserComponent } from './create-user/create-user.component';
import { RoleUserComponent } from './role-user/role-user.component';
import { UpdateUserComponent } from './update-user/update-user.component';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { ResetPasswordDialogComponent } from '@app/users/reset-password/reset-password.component';
import * as _ from 'lodash';
import { UploadAvatarComponent } from './upload-avatar/upload-avatar.component';
import * as moment from 'moment';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ImportUserWorkingTimeComponent } from './import-user-working-time/import-user-working-time.component';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { FormControl } from '@angular/forms';
import { BranchService } from '@app/service/api/branch.service';
import { PositionDto } from '@app/service/api/model/position-dto';
import { PositionService } from '@app/service/api/position.service';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css'],
  providers: [DatePipe, CurrencyPipe]
})
export class UserSecondComponent extends PagedListingComponentBase<userDTO> implements OnInit {
  ADD_USER = PERMISSIONS_CONSTANT.AddUser;
  EDIT_USER = PERMISSIONS_CONSTANT.EditUser;
  EDIT_USER_ROLE = PERMISSIONS_CONSTANT.EditUserRole;
  DELETE_USER = PERMISSIONS_CONSTANT.DeleteUser;
  CHANGE_STATUS_USER = PERMISSIONS_CONSTANT.ChangeStatusUser;
  RESET_PASSWORD = PERMISSIONS_CONSTANT.ResetPassword;
  UPLOAD_AVATAR = PERMISSIONS_CONSTANT.UploadAvatar;
  UploadWorkingTime = PERMISSIONS_CONSTANT.UploadWorkingTime;
  VIEW_LEVEL_USER = PERMISSIONS_CONSTANT.ViewLevelUser;
  enableExpandName = true;
  isLoadingFileUpload: boolean;
  isActive;
  managerId;
  expand = true;
  managers: Manager[] = [];
  userType;
  level;
  keyword;
  branchId;
  positionId = -1;
  sexes = [
    { value: 0, label: 'Male', class: 'bg-red label status-label' },
    { value: 1, label: 'Female', class: 'bg-green label status-label' }
  ];
  users: userDTO[];
  showHeader = false;
  turnOffPermission = false;
  ProjectUserType = ProjectUserType;
  usersNotPagging: UserForManager[];
  filterItems: FilterDto[] = [];
  @ViewChild(MatMenuTrigger)
  actionsMenu1: MatMenuTrigger;
  tableHeader = [
    { name: 'Check All', value: true, fieldName: 'checkAll' },
    { name: 'User', value: true, fieldName: 'user' },
    //{ name: 'User Code', value: true, fieldName: 'userCode' },
    { name: 'Position', value: true, fieldName: 'position' },
    { name: 'Branch', value: true, fieldName: 'branch' },
    { name: 'Working Time', value: true, fieldName: 'workingTime' },
    { name: 'Start Date', value: true, fieldName: 'sd' },
    { name: 'Allowed Leaveday', value: true, fieldName: 'allowedLeaveday' },
    { name: 'Type', value: true, fieldName: 'type' },
    { name: 'Level', value: true, fieldName: 'level' },
    { name: 'Projects', value: true, fieldName: 'project' },
    { name: 'Salary', value: true, fieldName: 'salary' },
    { name: 'Salary At', value: true, fieldName: 'salaryAt' },
    { name: 'Sex', value: true, fieldName: 'sex' },

    { name: 'Basic Tranner', value: true, fieldName: 'basicTranner' },

    { name: 'Roles', value: true, fieldName: 'role' },
    { name: 'Creation Time', value: true, fieldName: 'createTime' },
    { name: 'Is Active', value: true, fieldName: 'isActive' },
    //{ name: 'Position', value: true, fieldName: 'position' },
  ];
  contextMenuPosition = { x: '0px', y: '0px' };

  scrollbarPosition = 0;
  scrollingValue = 200;

  userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Internship' },
    { value: 2, label: 'Collaborator' }
  ];

  isExpandUserName = false;

  TABLE_NAME = 'TABLE_USERS';
  APP_NAME_VERSION = 'NCC_TIMESHEET_VERSION';

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter: BranchDto[];
  listPosition: PositionDto[] = [];
  positionSearch: FormControl = new FormControl("")
  listPositionFilter: PositionDto[];

  onContextMenu(event: MouseEvent, temp) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    temp.openMenu();
  }
  constructor(
    injector: Injector,
    private userService: UserService,
    private dialog: MatDialog,
    private datePipe: DatePipe,
    private currency: CurrencyPipe,
    private branchService: BranchService,
    private positionService: PositionService,
  ) {
    super(injector);
    this.isActive = '1';
    this.userType = '100';
    this.level = '100';
    this.managerId = '-1';
    this.branchId = 0;

    const currentVersion = this.getLocal(this.TABLE_NAME);
    let currentColumnList = localStorage.getItem(this.TABLE_NAME)
    if (!currentColumnList) {
      localStorage.setItem(this.TABLE_NAME, JSON.stringify(this.tableHeader))
    }
    else {
      if (this.compareWithLocalStorage()) {
        this.tableHeader = JSON.parse(currentColumnList)
      }
      else{
        localStorage.setItem(this.TABLE_NAME, JSON.stringify(this.tableHeader))
      }
    }

    this.userService.getAllNotPagging().subscribe(data => {
      this.usersNotPagging = data.result.filter(res => res.type == 0);
    });
    this.userService.getAllManager().subscribe(data => {
      this.managers = data.result;
    });
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
    this.positionSearch.valueChanges.subscribe(() => {
      this.filterPosition();
    })

  }
  compareWithLocalStorage() {
    let listToCompare = this.getLocal(this.TABLE_NAME)
    listToCompare = listToCompare.map(item => item.fieldName)
    let currentColumnList = this.tableHeader.map(item => item.fieldName)
    console.log(listToCompare)
    console.log(currentColumnList)
    console.log(JSON.stringify(listToCompare) === JSON.stringify(currentColumnList))
    if (JSON.stringify(listToCompare) === JSON.stringify(currentColumnList)) {
      return true
    }
    return false
  }
  public getLocal(name: string) {
    return JSON.parse(localStorage.getItem(name));
  }
  public setLocal(name: string, value: any): void {
    localStorage.setItem(name, JSON.stringify(value));
  }

  ngOnInit(): void {
    this.refresh();
    this.getListBranch();
    this.getListPosition();
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }
  getListPosition() {
    this.positionService.getAll().subscribe(res => {
      this.listPosition = res.result;
      this.listPositionFilter = this.listPosition;
    });
  }
  filterPosition(): void {
    if (this.positionSearch.value) {
      this.listPosition = this.listPositionFilter.filter(data => data.name.toLowerCase().includes(this.positionSearch.value.toLowerCase().trim()));
    } else {
      this.listPosition = this.listPositionFilter.slice();
    }
  }
  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  toNumber(str: string): any {
    return +str;
  }

  showOrHideHeader(): void {
    this.showHeader = !this.showHeader;
    if (this.showHeader) {
      this.turnOffPermission = false;
    }
  }
  getAll() {
    this.userService.getAll().subscribe(data => {


    })
  }
  turnOff(): void {
    if (this.turnOffPermission) {
      this.showHeader = false;
    }
    this.turnOffPermission = true;
  }

  changSelection(item): void {
    if (item.fieldName == 'checkAll') {
      this.tableHeader.forEach(e => {
        e.value = item.value;
      });
    }

    this.setLocal(this.TABLE_NAME, this.tableHeader);
    const countShowHeader = this.tableHeader.reduce((total, currentItem)=>{ return total + (currentItem.value ? 1: 0)},0)
    this.tableHeader[0].value = countShowHeader === this.tableHeader.length;
    if (countShowHeader > 8) {
      this.enableExpandName = true;
    } else {
      this.enableExpandName = false;
      this.isExpandUserName = false;
      this.users.forEach(s => {
        s.expandMgName = false;
      });
    }
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (this.keyword) {
      request.searchText = this.keyword.trim();
    }
    this.removeFilterItem();
    if (this.isActive != null && this.isActive < 100) {
      this.addFilterItem('isActive', this.toNumber(this.isActive));
    }
    if (this.userType != null && this.userType < 100) {
      this.addFilterItem('type', this.toNumber(this.userType));
    }
    if (this.level != null && this.level < 100) {
      this.addFilterItem('level', this.toNumber(this.level));
    }
    if (this.managerId != null && this.managerId != -1) {
      this.addFilterItem('managerId', this.toNumber(this.managerId));
    }
    if (this.branchId != 0) {
      this.addFilterItem('branchId', this.toNumber(this.branchId));
    }
    if (this.positionId != null && this.positionId != -1) {
      this.addFilterItem('positionId', this.positionId);
    }
    request.filterItems = this.filterItems;
    this.userService
      .getAllPagging(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.users = result.result.items;
        _.map(this.users, (u) => {
          if (u.salary) {
            u.salary = this.currency.transform(u.salary, "", "", "0.0-0", "").replace(/\u00a0/g, ".");
            u.salary = u.salary.slice(0, u.salary.length);
          }

          if (u.startDateAt) {
            u.startDateAt = this.datePipe.transform(new Date(u.startDateAt), 'dd/MM/yyyy');
          }
          if (u.salaryAt) {
            if (u.type == this.UserType.Intern && u.level >= this.Level.Intern_1) {
              u.isLevelUp = moment(u.salaryAt).get('M') === moment().get('M');
            } else {
              u.isLevelUp = false;
            }

            u.salaryAt = this.datePipe.transform(new Date(u.salaryAt), 'dd/MM/yyyy');

          }

          if (u.creationTime) {
            // u.creationTime = moment(u.creationTime).tz('Asia/Ho_Chi_Minh').format("DD/MM/YYYY HH:mm:ss");
            u.creationTime = moment(u.creationTime).format('DD/MM/YYYY HH:mm:ss');
          }
          if (!u.avatarFullPath) {
            switch (u.sex) {
              case 0: {
                u.avatarFullPath = 'assets/images/men.png';
                break;
              }
              case 1: {
                u.avatarFullPath = 'assets/images/women.png';
                break;
              }
              default: u.avatarFullPath = 'assets/images/undefine.png';
                break;
            }
          }
          if (!u.managerAvatarFullPath && u.managerId) {
            switch (u.sex) {
              case 0: {
                u.managerAvatarFullPath = 'assets/images/men.png';
                break;
              }
              case 1: {
                u.managerAvatarFullPath = 'assets/images/women.png';
                break;
              }
              default: u.managerAvatarFullPath = 'assets/images/undefine.png';
                break;
            }
          }
        });
        this.showPaging(result.result, pageNumber);
      });
  }

  removeFilterItem(): void {
    this.filterItems = [];
  }

  addFilterItem(str, num): void {
    this.filterItems.push({ comparison: 0, propertyName: str, value: num });
  }

  protected delete(user): void {
    abp.message.confirm(this.l('Delete This User?'),
      (result: boolean) => {
        if (result) {
          this.userService.delete(user.id).subscribe(res => {
            if (res) {
              this.notify.success(this.l('Delete User Successfully!'));
              this.refresh();
            }
          });
        }
      }
    );
  }

  searchOrFilter(): void {
    this.refresh();
  }

  createOrEdit(editUser?): void {

    let userId = editUser ? editUser.id : null;
    let diaLogRef = this.dialog.open(CreateUserComponent, {
      disableClose: true,
      data: { userId: userId, userss: this.usersNotPagging, sexes: this.sexes }
    });

    diaLogRef.afterClosed().subscribe(res => {
      if (res) {
        this.refresh();
      }
    });
  }

  editUser(editUser?): void {

    let userId = editUser ? editUser.id : null;
    let diaLogRef = this.dialog.open(UpdateUserComponent, {
      disableClose: true,
      data: { userId: userId, userss: this.usersNotPagging, sexes: this.sexes }
    });

    diaLogRef.afterClosed().subscribe(res => {
      if (res) {
        this.refresh();
      }
    });
  }

  editRole(editUser?): void {
    let userId = editUser ? editUser.id : null;
    let diaLogRef = this.dialog.open(RoleUserComponent, {
      disableClose: true,
      data: { userId: userId}
    });

    diaLogRef.afterClosed().subscribe(res => {
      if (res) {
        this.refresh();
      }
    });
  }

  showResetPasswordUserDialog(user?: number): void {
    this.dialog.open(ResetPasswordDialogComponent, {
      data: user
    });
  }

  deactivateUser(id: number) {
    abp.message.confirm(this.l('Deactivate this user?'),
      (result: boolean) => {
        if (result) {
          this.userService.deactivateUser(id).subscribe(res => {
            if (res) {
              this.notify.success(this.l('Deactivated User Successfully!'));
              this.refresh();
            }
          });
        }
      }
    );
  }

  upLoadAvatar(user): void {
    let diaLogRef = this.dialog.open(UploadAvatarComponent, {
      width: '600px',
      data: user.id
    });
    diaLogRef.afterClosed().subscribe(res => {
      if (res) {
        this.userService.uploadImageFile(res, user.id).subscribe(data => {
          if (data) {
            this.notify.success('Upload Avatar Successfully!');
            if (this.appSession.user.id == user.id) {
              this.appSession.user.avatarFullPath = data.body.result;
            }
            user.avatarFullPath = data.body.result;
            this.users.forEach(u => {
              if (u.managerId == user.id) {
                u.managerAvatarFullPath = user.avatarFullPath;
              }
            });
            this.refresh()
          } else { this.notify.error('Upload Avatar Failed!'); }
        });
      }
    });
  }

  nextOrPre(str): void {

    function getMaxChildWidth(elm) {
      let childrenWidth = $.map($('>*', elm), function (el: string) { return $(el).width(); });
      let max = 0;
      for (let i = 0; i < childrenWidth.length; i++) {
        max = childrenWidth[i] > max ? childrenWidth[i] : max;
      }
      return max;
    }

    function getScrollingValue(toLeft, ctx, pos, val) {

      if (toLeft) {
        return pos < 1 ? 0 : val;
      }
      return pos >= getMaxChildWidth(ctx) ? 0 : val;
    }

    if (str == 'left') {
      $('#tbl').scrollLeft(
        this.scrollbarPosition -= getScrollingValue(true, $('#tbl'), this.scrollbarPosition, this.scrollingValue)
      );
    } else {
      $('#tbl').scrollLeft(
        this.scrollbarPosition += getScrollingValue(false, $('#tbl'), this.scrollbarPosition, this.scrollingValue)
      );
    }
  }

  activateUser(id: number) {
    abp.message.confirm(this.l('Activate this user?'),
      (result: boolean) => {
        if (result) {
          this.userService.activateUser(id).subscribe(res => {
            if (res) {
              this.notify.success(this.l('Activated User Successfully!'));
              this.refresh();
            }
          });
        }
      }
    );
  }

  importExcel() {
    const dialog = this.dialog.open(ImportUserWorkingTimeComponent, {
      width: '500px'
    });
    dialog.afterClosed().subscribe(result => {
      if (result === 'refresh') {
        this.ngOnInit();
      }
    });
  }
}

export class userDTO {
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  phoneNumber: string;
  address: string;
  isActive: true;
  fullName: string;
  lastLoginTime: string;
  creationTime: string;
  roleNames: string[];
  projectUsers: ProjectUser[];
  type: number;
  salary: any;
  salaryAt: string;
  startDateAt: string;
  allowedLeaveDay: number;
  userCode: string;
  jobTitle: string;
  level: number;
  avatarFullPath: any;
  managerId: number;
  managerAvatarFullPath: string;
  managerName: string;
  branch: string;
  position: string;
  sex: number;
  expandProject = false;
  expandRole = false;
  expandUser = false;
  expandMgName = false;
  id: number;
  isLevelUp: boolean;
  workStartHour: number;
  workStartMinute: number;
}
export class ProjectUser {
  projectId: number;
  projectCode: string;
  projectName: string;
  projectUserType: number;
}

export class UserManager {
  managerId: number;
  manageravatarFullPath: string;
  managerName: string;
}

export class Manager {
  name: string;
  isActive: boolean;
  type: number;
  jobTitle: string;
  level: number;
  userCode: string;
  id: number;
}

export const ProjectUserType = [
  { value: 0, name: 'Member' },
  { value: 1, name: 'PM' },
  { value: 2, name: 'Shadow' },
  { value: 3, name: 'DeActive' }
];

export class UserForManager {
  name: string;
  isActive: boolean;
  type: number;
  jobTitle: string;
  level: number;
  userCode: string;
  id: number;
}

export class NameValue {
  name: string;
  value: boolean;
}
