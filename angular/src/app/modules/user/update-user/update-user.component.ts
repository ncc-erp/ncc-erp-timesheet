import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ConfigurationService } from './../../../service/api/configuration.service';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { BranchDto, RoleDto, UserDto } from '@shared/service-proxies/service-proxies';
import { MatCheckboxChange, MAT_DIALOG_DATA, MatDialogRef, MAT_DATE_LOCALE } from '@angular/material';
import * as _ from 'lodash';
import { UserService } from '@app/service/api/user.service';
import * as moment from 'moment';
import { FormControl } from '@angular/forms';
import { UserForManager } from '../user.component';
import { AppConsts } from '@shared/AppConsts';
import { has } from 'lodash';
import { BranchService } from '@app/service/api/branch.service';
import { PositionService } from '@app/service/api/position.service';
import { PositionDto } from '@app/service/api/model/position-dto';

@Component({
  selector: 'app-update-user',
  templateUrl: './update-user.component.html',
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class UpdateUserComponent extends AppComponentBase implements OnInit {
  UPDATE_USER_WORKING_TIME = PERMISSIONS_CONSTANT.UpdateUserWorkingTime;
  timeZone: string
  isDefault: boolean;
  activeRandom = true;
  shPassword = false;
  shConfirmPassword = false;
  confirmPassword = '';
  confirmDisale = false;
  title = '';
  saving = false;
  ischeck = false;
  user = {} as createUserDTO;
  roles: RoleDto[] = [];
  checkedRolesMap: { [key: string]: boolean } = {};
  defaultRoleCheckedStatus = false;
  isLoadingAvatarUpload: boolean = false;
  listLevelFiltered = [];
  listLevel = [];
  startDate: FormControl = new FormControl();
  salaryDate: FormControl = new FormControl();
  managerSearch: FormControl = new FormControl();
  users;
  sexes;
  managers: UserForManager[];
  managersFiltered: UserForManager[];
  respone = 1;
  userGot;
  enableNormalLogin = AppConsts.enableNormalLogin;
  userId: number;
  isSaving: boolean = false;
  mapBranchWorkingTime;
  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter: BranchDto[];
  listPosition: PositionDto[] = [];
  positionSearch: FormControl = new FormControl("")
  listPositionFilter : PositionDto[];
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private diaLogRef: MatDialogRef<UpdateUserComponent>,
    private userService: UserService,
    private configService: ConfigurationService,
    private branchService: BranchService,
    private positionService: PositionService,
  ) {
    super(injector);
    this.userId = data.userId;
    this.sexes = data.sexes;
    this.listLevelFiltered = this.userLevels.slice();
    this.listLevel = this.userLevels.slice();
    this.managers = data.userss;
    this.managerSearch.setValue("");
    this.managerSearch.valueChanges.subscribe(() => {
      this.filterManagers();
    });

    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
		
    this.positionSearch.valueChanges.subscribe(() => {
      this.filterPosition();
    })
  }

  ngOnInit(): void {
    this.getBranchWorkingTime()
    this.getListBranch();
    this.getListPosition();
  }
  getListBranch() {
    this.branchService.getAllBranchFilter(false).subscribe(res => {
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
  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }
  filterPosition(): void {
    if (this.positionSearch.value) {
      this.listPosition = this.listPositionFilter.filter(data => data.name.toLowerCase().includes(this.positionSearch.value.toLowerCase().trim()));
    } else {
      this.listPosition = this.listPositionFilter.slice();
    }
  }
  getBranchWorkingTime(){
    this.branchService.GetAllNotPagging().subscribe(data=>{
      this.mapBranchWorkingTime =data.result
      this.userService.getOneUser(this.userId).subscribe(result => {
        this.user = result.result;
        this.isDefault = this.user.isWorkingTimeDefault;
        this.title = 'Edit User ' + this.user.userName;
        this.managersFiltered = this.managers.slice();
      });
    })
  }
  changeTime() {
    var workingTime = this.mapBranchWorkingTime.find(item => item.id==this.user.branchId);
    if(this.isDefault==true){
      this.user.morningStartAt = workingTime.morningStartAt
      this.user.morningEndAt = workingTime.morningEndAt
      this.user.afternoonStartAt = workingTime.afternoonStartAt
      this.user.afternoonEndAt = workingTime.afternoonEndAt
      this.user.morningWorking = workingTime.morningWorking
      this.user.afternoonWorking = workingTime.afternoonWorking
    }

  }

  changeValue(value) {
      this.isDefault = value.checked;
      this.changeTime()
  }
  onChangeBranch() {
    this.changeTime()
  }

  onEmailChange(): void {
    if (this.user.emailAddress) {
      if (this.user.emailAddress.endsWith(".ncc@gmail.com")) {
        this.user.type = this.UserType.Intern;
        this.onUserTypeChange();
      } else {
        if (this.user.emailAddress.endsWith("ncc.asia")) {
          this.user.type = this.UserType.Staff;
          this.onUserTypeChange();
        } else {
          this.user.type = null;
        }
      }
    }
  }

  onUserTypeChange(): void {
    if (this.user.type == null) {
      this.listLevelFiltered = this.userLevels.slice();
    } else if (this.user.type == this.UserType.Intern) {
      this.listLevelFiltered = this.userLevels.slice(0, 4);
    } else {
      this.listLevelFiltered = this.userLevels.slice(4, this.userLevels.length);
    }
    if (!this.listLevelFiltered.find(s => s.value == this.user.level)) {
      this.user.level = null;
      this.onLevelChange();
    }
    //
  }

  onLevelChange(): void {
    if (this.user.level == null) {
      this.user.salary = 0;
      return;
    }
    if (this.user.level == this.Level.Intern_0) {
      this.user.salary = 75000;
    } else if (this.user.level == this.Level.Intern_1) {
      this.user.salary = 675000;
    } else if (this.user.level == this.Level.Intern_2) {
      this.user.salary = 2675000;
    } else if (this.user.level == this.Level.Intern_3) {
      this.user.salary = 4675000;
    }

    if (this.user.type == this.UserType.Intern && this.user.level > this.Level.Intern_0) {
      this.user.salaryAt = moment().add(1, 'M').startOf("M").toISOString();
    }

  }

  stringToFloat(input: string): number {
    return parseFloat(input);
  }

  onChangeMSA(event) {
    if (this.user.morningEndAt && event) {
      this.user.morningWorking = this.calculationTime(event, this.user.morningEndAt);
    }
    this.user.morningWorking = Number(this.timetoNumber(this.user.morningWorking).toFixed(2).toString())
  }

  onChangeMEA(event) {
    if (this.user.morningStartAt && event) {
      this.user.morningWorking = this.calculationTime(this.user.morningStartAt, event);
    }
    this.user.morningWorking = Number(this.timetoNumber(this.user.morningWorking).toFixed(2).toString())
  }

  onChangeASA(event) {
    if (this.user.afternoonEndAt && event) {
      this.user.afternoonWorking = this.calculationTime(event, this.user.afternoonEndAt);
    }
    this.user.afternoonWorking = Number(this.timetoNumber(this.user.afternoonWorking).toFixed(2).toString())
  }


  onChangeAEA(event) {
    if (this.user.afternoonStartAt && event) {
      this.user.afternoonWorking = this.calculationTime(this.user.afternoonStartAt, event);
    }
    this.user.afternoonWorking = Number(this.timetoNumber(this.user.afternoonWorking).toFixed(2).toString())
  }
  timetoNumber(time) {
    var hoursMinutes = time.split(/[.:]/);
    var hours = parseInt(hoursMinutes[0], 10);
    var minutes = hoursMinutes[1] ? parseInt(hoursMinutes[1], 10) : 0;
    return hours + minutes / 60;
  }
 
  
  

  filterManagers(): void {
    if (this.managerSearch.value) {
      var temp: string = this.managerSearch.value.toLowerCase().trim();
      this.managersFiltered = this.managers.filter(data => data.name.toLowerCase().includes(temp));
    } else this.managersFiltered = this.managers.slice();
  }

  getCheckedRoles(): string[] {
    const roles: string[] = [];
    _.forEach(this.checkedRolesMap, function (value, key) {
      if (value) {
        roles.push(key);
      }
    });
    return roles;
  }

  randomPassword() {
    let passRandom = Math.random()
      .toString(36)
      .substr(2, 10);

    this.user.password = passRandom;
    this.confirmPassword = this.user.password;
    this.reactChangePassword(true);
  }
  backField(val) {
    if (val === '') {
      this.confirmPassword = '';
      this.reactChangePassword(false);
    }
  }
  reactChangePassword(val: boolean) {
    this.confirmDisale = val;
    this.shPassword = val;
  }

  getManagerId(): number {
    var temp = _.pick(this.users, ["userManager"]);
    if (temp.userManager == null || temp.userManager.length == 0) {
      return 0;
    } else return temp.userManager[0].managerId;
  }

  calculationTime(timeStart: string, timeEnd: string): string {
    let time = moment(timeEnd, 'HH:mm').diff(moment(timeStart, 'HH:mm'), 'minutes');
    var h = time / 60 | 0, m = time % 60 | 0;
    return moment.utc().hours(h).minutes(m).format("HH:mm");
  }

  formatTime(time: string) {
    if (time) {
      if (time.includes(':')) {
        let t = time.split(':');
        if (t[1]) {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(Number.parseInt(t[1])).format("HH:mm");
        } else {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(0).format("HH:mm");
        }
      } else {
        return moment.utc().hours(Number.parseInt(time)).minutes(0).format("HH:mm");
      }
    }
  }

  save(): void {
    this.isSaving = true;
    if (this.user.salary == null) {
      this.user.salary = 0;
    }

    this.user.userName = this.user.userName.trim();
    this.user.startDateAt = this.user.startDateAt ? moment(this.user.startDateAt).add(7, 'h').toISOString() : this.user.startDateAt;

    this.user.salaryAt = this.user.salaryAt ? moment(this.user.salaryAt).add(7, 'h').toISOString() : this.user.salaryAt;

    if (isNaN(this.user.allowedLeaveDay)) {
      abp.message.error(this.l("Allowed Leaveday must be a number!"));
      this.isSaving =false;
      return;
    }

    if (this.user.allowedLeaveDay < 0) {
      abp.message.error(this.l("Allowed Leaveday can't be negative!"));
      this.isSaving =false;
      return;
    }
    this.user.isWorkingTimeDefault = this.isDefault;
    this.user.morningStartAt = this.formatTime(this.user.morningStartAt);
    this.user.morningEndAt = this.formatTime(this.user.morningEndAt);
    this.user.afternoonStartAt = this.formatTime(this.user.afternoonStartAt);
    this.user.afternoonEndAt = this.formatTime(this.user.afternoonEndAt);

    if (this.user.allowedLeaveDay == null || !this.user.allowedLeaveDay) {
      this.user.allowedLeaveDay = 0;
    }
    this.userService.update(this.user).subscribe(data => {
      if (data.success) {
        this.notify.success(this.l("Edit User Successfully!"));
        this.close(true);
        this.isSaving = false;
      }
    },
      err => {
        this.isSaving = false;
      }
    );

  }

  close(isCloseDialog): void {
    this.diaLogRef.close(isCloseDialog);
  }

  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]

  public maskHour = {
    guide: false,
    showMask: false,
    mask: [/\d/, /\d/],
  }

  public maskMinute = {
    guide: false,
    showMask: false,
    mask: [/[0-5]/, /[0-9]/]
  };
}

export class createUserDTO {
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  phoneNumber: string;
  address: string;
  isActive: boolean;
  password: string;
  type: number;
  jobTitle: string;
  level: number;
  isWorkingTimeDefault : boolean;
  registerWorkDay: string;
  allowedLeaveDay: number;
  startDateAt: any;
  salary: number;
  salaryAt: string;
  sex: number;
  userCode: string;
  managerId: number;
  id: number;
  morningWorking: any;
  morningStartAt: string;
  morningEndAt: string;
  afternoonWorking: any;
  afternoonStartAt: string;
  afternoonEndAt: string;
  isStopWork: boolean;
  branchId: number;
  positionId: number;
  beginLevel:number;
  endDateAt:any;
}



