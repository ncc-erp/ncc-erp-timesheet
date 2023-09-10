import { ExportService } from './../../service/export.service';
import { AppComponentBase } from 'shared/app-component-base';
import { Component, OnInit, Injector, ChangeDetectorRef } from '@angular/core';
import { MatCheckboxChange, MatDialog } from '@angular/material';
import { TimesheetService } from '@app/service/api/timesheet.service';
import * as _ from 'lodash';
import { DomSanitizer } from '@angular/platform-browser';
import { TimeSheetDto } from '@app/service/api/model/timesheet-Dto';
import { AppConsts } from '@shared/AppConsts';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { NotifyService } from 'abp-ng2-module/dist/src/notify/notify.service';
import { DatePipe } from '@angular/common';
import { TimesheetWarningComponent } from './timesheet-warning/timesheet-warning.component';
import * as moment from 'moment';
import { FormControl } from '@angular/forms';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { BranchService } from '@app/service/api/branch.service';

export const MY_FORMATS = {
  parse: {
    dateInput: ['LL', 'YYYY-MM-DD'],
  },
  display: {
    dateInput: 'YYYY-MM-DD',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },

};

@Component({
  selector: 'app-timesheet',
  templateUrl: './timesheet.component.html',
  styleUrls: ['./timesheet.component.css']
})

export class TimesheetComponent extends AppComponentBase implements OnInit {
  APPROVAL_TIMESHEET = PERMISSIONS_CONSTANT.ApprovalTimesheets;
  EXPORT_EXCEL_TIMESHEET = PERMISSIONS_CONSTANT.ExportExcelTimesheets;

  viewBy: number = this.APP_CONSTANT.TimesheetViewBy.Project;
  filterStatus: number = this.APP_CONSTANT.TimesheetStatus.Pending;
  timesheetsGroup: any[] = [];
  //timesheets: any[] = [];

  //timesheetsTypeOfWorksFilter: any[] = []
  rawData: TimeSheetDto[] = []; // store the raw data (check getTimeSheets function for assign)
  filteredTimesheets: TimeSheetDto[] = [];
  defaultWorkingHourPerDay: number = 8 * 60;
  defaultWorkingHourPerSaturday: number = 4 * 60;
  listTimeSheetWarningDto: TimeSheetWarningDto[] = [];
  Timesheet_Statuses = [
    {
      value: this.APP_CONSTANT.TimesheetStatus.All,
      name: 'All',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Draft,
      name: 'Draft',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Pending,
      name: 'Pending',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Approve,
      name: 'Approved',
      count: 0
    },
    {
      value: this.APP_CONSTANT.TimesheetStatus.Reject,
      name: 'Rejected',
      count: 0
    }
  ]
  Timesheet_TypeOfWorks = [
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.All,
      name: 'All',
      count: 0
    },
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours,
      name: 'Normal working ',
      count: 0
    },
    {
      value: this.APP_CONSTANT.EnumTypeOfWork.Overtime,
      name: 'OverTime',
      count: 0
    },
  ]
  checkedCount: number = 0;
  totalCount: number = 0;

  fromDate: any;
  toDate: any;
  typeDate: any;
  isLoading: boolean;
  defaultFromDate: any;
  defaultToDate: any;

  userTypes = [
    { value: 0, label: "Staff" },
    { value: 1, label: "Intern" },
    { value: 2, label: "CTV" }
  ];


  selectedTypeOfWork: number = this.APP_CONSTANT.EnumTypeOfWork.All;


  projectFilter = []
  projectId = this.APP_CONSTANT.FILTER_DEFAULT.All;
  projectSearch: FormControl = new FormControl("")
  projects = []

  checkInFilter = this.APP_CONSTANT.FILTER_DEFAULT['All'];
  checkInFilterList = Object.keys(this.APP_CONSTANT.HaveCheckInFilter)

  public searchText: string = "";

  public listBranch: BranchDto[] = [];
  public branchId: number = 0;
  public branchSearch: FormControl = new FormControl("");
  public listBranchFilter: BranchDto[];

  constructor(
    injector: Injector,
    private timesheetService: TimesheetService,
    private domSanitizer: DomSanitizer,
    private exportService: ExportService,
    private changeDetector: ChangeDetectorRef,
    private _dialog: MatDialog,
    private projectManageService: ProjectManagerService,
    private branchService: BranchService,
  ) {
    super(injector)
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    })
  }

  ngOnInit() {
    this.getProjects();
    this.getListBranch();
    this.defaultFromDate = moment().add(-1, 'months').startOf('month');
    this.defaultToDate = moment();
    this.fromDate = this.defaultFromDate.format("YYYY-MM-DD");
    this.toDate = this.defaultToDate.format("YYYY-MM-DD");
  }

  getProjects() {
    this.projectManageService.getProjectPM().subscribe(data => {
      this.projectFilter = data.result
      this.projects = this.projectFilter
      this.projects.unshift({
        id: -1,
        name: "All"
      })
    })
  }

  ngAfterViewChecked() {
    this.changeDetector.detectChanges();
  }

  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.projects = this.projectFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.projects = this.projectFilter.slice();
    }
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
      console.log("data: ", this.listBranch);
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  getTimesheets() {
    this.isLoading = true;
    this.timesheetService.getAllTimesheets(this.fromDate, this.toDate, this.filterStatus, Number(this.projectId),Number(this.checkInFilter), this.searchText, Number(this.branchId)).subscribe(obj => {
      //this.timesheets = obj.result;
      this.rawData = obj.result;
      this.totalCount = this.rawData.filter(ts => ts.isUserInProject).length;
      this.updateCheckedCount();
      //this.convertData(this.rawData);
      this.onSelectedTypeOfWorkChange();
      this.isLoading = false;
    });
    this.getQuantiyTimesheetStatus();

  }
  getQuantiyTimesheetStatus() {
    this.timesheetService.getQuantiyTimesheetStatus(this.fromDate, this.toDate, Number(this.projectId), Number(this.checkInFilter), this.searchText, this.branchId).subscribe((obj: any) => {
      this.Timesheet_Statuses.forEach(item => {
        if(item.value === this.APP_CONSTANT.TimesheetStatus.All) {
          item.count = obj.result.reduce((previousValue, currentValue) => previousValue + currentValue.quantity, 0);
        } else {
          const resultListByStatus = obj.result.filter(s => s.status === item.value);
          if (resultListByStatus.length > 0) {
            item.count = resultListByStatus[0].quantity;
          }
        }
      })
    })
  }

  onSelectedTypeOfWorkChange() {
    this.filteredTimesheets = this.rawData.filter(s => this.selectedTypeOfWork === this.APP_CONSTANT.EnumTypeOfWork.All
      || s.typeOfWork === this.selectedTypeOfWork);

    this.convertData(this.filteredTimesheets);

  }

  onGroupByChange() {

    this.convertData(this.filteredTimesheets);

  }

  convertData(timesheets: TimeSheetDto[]) {
    this.timesheetsGroup = [];
    if (this.viewBy == this.APP_CONSTANT.TimesheetViewBy.People) {
      this.timesheetsGroup = this.buildDataByUser(timesheets);
    }
    else if (this.viewBy == this.APP_CONSTANT.TimesheetViewBy.Project) {
      this.timesheetsGroup = this.buildDataByProject(timesheets);
    }
  }

  buildDataByDate(data: Array<TimeSheetDto>): TimesheetGroupByDayDto[] {
    let typeOfWorkNormalworkinghours =  this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours;
    return _.chain(data).groupBy('dateAt').map((value, key) => ({
      dateAt: key,
      timesheets: value,
      checkStatus: this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE,
      //totalWorkingTime: value.map(s => s.workingTime).reduce((a, b) => a + b, 0),
      totalWorkingTime: value.reduce((total, item) => {
                                                        return total += item.workingTime;
                                                      },0),
      selectedCount: 0,
      totalCount: value.length,
      isThanDefaultWorkingHourPerDay : this.warningByDate(value),
      totalWorkingTimeNormalworkinghours : value.reduce((total, item) => {
                                                                            if (item.typeOfWork === typeOfWorkNormalworkinghours) {
                                                                              return total += item.workingTime + item.offHour * 60;
                                                                            }
                                                                          },0)
    } as TimesheetGroupByDayDto)).orderBy(x => x.dateAt, 'desc').value();

  }
  warningByDate(value: Array<TimeSheetDto>){
    let typeOfWorkNormalworkinghours =  this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours;
    let totalWorkingTime =  value.filter(s => s.typeOfWork === typeOfWorkNormalworkinghours).reduce((total, item) => {
      return total += item.workingTime + item.offHour * 60;
    },0)

    let isLogTsWorkNormalOnOffDay = false;
    if(value.find(s => s.typeOfWork === typeOfWorkNormalworkinghours && s.isOffDay)){
      isLogTsWorkNormalOnOffDay =  true;
    }

    if(isLogTsWorkNormalOnOffDay ||
        totalWorkingTime > this.defaultWorkingHourPerDay ||
        (moment(value[0].dateAt, 'YYYY-MM-DD').toDate().getDay() == this.APP_CONSTANT.EnumDayOfWeekByGetDay.Saturday &&
          totalWorkingTime > this.defaultWorkingHourPerSaturday
        )
      ){
      return true;
    }
    return false;
  }

  buildDataByUser(data: Array<TimeSheetDto>): TimesheetGroupByUserDto[] {
    let that = this;
    return _.chain(data).groupBy('emailAddress').map(function (value, key) {
      let dayTasks = that.buildDataByDate(value);
      const { userId, user, avatarFullPath, branch, branchName, level, type, branchColor, branchDisplayName} = value[0];
      return {
        userId: userId,
        userName: user,
        avatarFullPath,
        branch,
        branchName,
        level,
        type,
        branchColor,
        branchDisplayName,
        checkStatus: that.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE,
        //totalUserWorkingTime: dayTasks.map(s => s.totalWorkingTime).reduce((a, b) => a + b, 0),
        totalUserWorkingTime: dayTasks.reduce((total, item) => {
                                                                  return total += item.totalWorkingTime;
                                                                },0),
        selectedCount: 0,
        totalCount: dayTasks.length,
        dayTasks: dayTasks,
        isThanDefaultWorkingHourPerDay: dayTasks.find(item => item.isThanDefaultWorkingHourPerDay == true) != undefined ? true:false
      } as TimesheetGroupByUserDto
    }).value();
  }


  buildDataByProject(data: Array<TimeSheetDto>): TimesheetGroupByProjectDto[] {
    let that = this;
    return _.chain(data).groupBy('projectId').map(function (value, key) {
      let userTasks = that.buildDataByUser(value);
      return {
        projectId: +key,
        projectName: value[0].projectName,
        checkStatus: that.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE,
        totalProjectWorkingTime: userTasks.map(s => s.totalUserWorkingTime).reduce((a, b) => a + b, 0),
        selectedCount: 0,
        totalCount: userTasks.length,
        userTasks: userTasks
      } as TimesheetGroupByProjectDto;
    }).value();
  }

  refresh(): void {
    this.getTimesheets();
  }


  afterChildChangeWhatStatusParentAre(parent: any, childList: any[]) {
    let countAll = 0;
    let countNone = 0;
    childList.forEach(item => {
      if (item.checkStatus === this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL) countAll++;
      else if (item.checkStatus === this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE) countNone++;
    });
    if (countNone === childList.length) parent.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE;
    else if (countAll === childList.length) parent.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL;
    else parent.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_SOME;

    parent.selectedCount = countAll;
  }

  updateCheckedCount() {
    this.checkedCount = this.rawData.filter(ts => (ts.checked && ts.isUserInProject)).length;
  }

  onTotalCheckedChange($e: MatCheckboxChange) {
    if (this.viewBy == this.APP_CONSTANT.TimesheetViewBy.Project) {
      this.timesheetsGroup.forEach(project => {
        this.onProjectCheckedChange($e, project);
      });

    }
    else {
      this.timesheetsGroup.forEach(user => {
        this.onUserCheckedChange($e, user);
      });
    }
    // this.updateCheckedCount();
    this.checkedCount = $e.source.checked ? this.totalCount : 0;
  }

  onProjectCheckedChange($e: MatCheckboxChange, project: TimesheetGroupByProjectDto) {
    let isCheckedAll = $e.source.checked;
    project.checkStatus = isCheckedAll ? this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL : this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE;
    project.userTasks.forEach(ut => {
      ut.checkStatus = project.checkStatus;
      ut.selectedCount = isCheckedAll ? ut.totalCount : 0;
      ut.dayTasks.forEach(dt => {
        dt.checkStatus = project.checkStatus;
        dt.selectedCount = isCheckedAll ? dt.totalCount : 0;
        dt.timesheets.forEach(t => t.checked = t.isUserInProject ? isCheckedAll : false);
      });
    });
    project.selectedCount = isCheckedAll ? project.totalCount : 0;
    this.updateCheckedCount();
  }

  onUserCheckedChange($e: MatCheckboxChange, user: TimesheetGroupByUserDto, project?: TimesheetGroupByProjectDto) {
    //Toggle status
    let isCheckedAll = $e.source.checked;
    user.checkStatus = isCheckedAll ? this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL : this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE;
    user.dayTasks.forEach(dt => {
      dt.checkStatus = user.checkStatus;
      dt.timesheets.forEach(t => t.checked = t.isUserInProject ? isCheckedAll : false);
      dt.selectedCount = isCheckedAll ? dt.totalCount : 0;
    });
    if (project != undefined) {
      this.afterChildChangeWhatStatusParentAre(project, project.userTasks);
    }
    user.selectedCount = isCheckedAll ? user.totalCount : 0;
    this.updateCheckedCount();
  }

  onDayTaskCheckedChange($e: MatCheckboxChange, dayTask: TimesheetGroupByDayDto, user: TimesheetGroupByUserDto, project?: TimesheetGroupByProjectDto) {
    //Toggle status
    let isCheckedAll = $e.source.checked;
    dayTask.checkStatus = isCheckedAll ? this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL : this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE;
    dayTask.timesheets.forEach(t => t.checked = t.isUserInProject ? isCheckedAll : false);
    this.afterChildChangeWhatStatusParentAre(user, user.dayTasks);
    if (project != undefined) {
      this.afterChildChangeWhatStatusParentAre(project, project.userTasks);
    }
    dayTask.selectedCount = isCheckedAll ? dayTask.totalCount : 0;
    this.updateCheckedCount();
  }

  onTimesheetCheckedChange($e: MatCheckboxChange, timesheet: TimeSheetDto, dayTask: TimesheetGroupByDayDto, user: TimesheetGroupByUserDto, project?: TimesheetGroupByProjectDto) {
    let isCheckedAll = $e.source.checked;
    let countAll = 0;
    let countNone = 0;
    timesheet.checked = isCheckedAll;
    dayTask.timesheets.forEach(item => {
      if (item.checked) countAll++;
      else countNone++;
    });

    if (countNone === dayTask.timesheets.length) dayTask.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_NONE;
    else if (countAll === dayTask.timesheets.length) dayTask.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_ALL;
    else dayTask.checkStatus = this.APP_CONSTANT.CHECK_STATUS.CHECKED_SOME;

    dayTask.selectedCount = countAll;

    this.afterChildChangeWhatStatusParentAre(user, user.dayTasks);
    if (project != undefined)
      this.afterChildChangeWhatStatusParentAre(project, project.userTasks);

    this.updateCheckedCount();
  }

  handleDateSelectorChange(date) {
    const { fromDate, toDate } = date;
    this.setFromAndToDate(fromDate, toDate);
    this.getTimesheets();
  }

  setFromAndToDate(fromDate, toDate) {
    this.fromDate = fromDate;
    this.toDate = toDate;
  }


  showTimesheetWarning(data: TimeSheetWarningDto[],approveTimesheetIds) {
    var diaLogRef = this._dialog.open(TimesheetWarningComponent, {
      disableClose: true,
      width: "1300px",
      data: {item : data ,approveTimesheetIds:approveTimesheetIds}
    });
    diaLogRef.afterClosed().subscribe((res) => {
      if(res){
        this.getTimesheets();
      }
    });
  }

  btnApprove() {
    let selectedTimesheets = [];
    this.rawData.forEach(item => {
      if (item.checked && item.isUserInProject && item.status != this.APP_CONSTANT.TimesheetStatus.Draft)
        selectedTimesheets.push(item);
    });
    let approveTimesheetIds = selectedTimesheets.filter(x => x.status != this.APP_CONSTANT.TimesheetStatus.Approve && x.status != this.APP_CONSTANT.TimesheetStatus.Draft).map(s => s.id);

    let timesheetNormalWorkingIds = selectedTimesheets.filter(x => x.status != this.APP_CONSTANT.TimesheetStatus.Approve &&
                                                        x.status != this.APP_CONSTANT.TimesheetStatus.Draft &&
                                                        x.typeOfWork == this.APP_CONSTANT.EnumTypeOfWork.Normalworkinghours).map(s => s.id);

    if (approveTimesheetIds.length == 0) {
      abp.message.error(
        "You have to select at least one Rejected or Pending timesheet"
      )
      return;
    }
    var msg = approveTimesheetIds.length == 1 ? `${approveTimesheetIds.length} Timesheet` : `${approveTimesheetIds.length} Timesheets`

    if(timesheetNormalWorkingIds.length == 0){
      this.doApprove(msg,approveTimesheetIds);
      return;
    }

    this.timesheetService.getTimesheetWarning(timesheetNormalWorkingIds).subscribe((res: any) => {
      if (res.result.length > 0) {
        this.listTimeSheetWarningDto = res.result;
        this.showTimesheetWarning(this.listTimeSheetWarningDto,approveTimesheetIds);
        return;
      }
      this.doApprove(msg,approveTimesheetIds);
    });
  }

  doApprove(msg,approveTimesheetIds) {
    abp.message.confirm(
      `Approve ${msg}`,
      (result: boolean) => {
        if (result) {
          this.timesheetService.approveTimesheet(approveTimesheetIds).subscribe((res: any) => {
            if (res) {
              let successMessage = res.result.successCount > 0 ? `<font color='#029720'>${res.result.success} <br /> </font>` : ''
              let failMessage = res.result.failedCount > 0 ? `<font color='#e70d0d'>${res.result.fail} <br /> </font>` : ''
              abp.message.info(successMessage + failMessage + res.result.lockDate, "APPROVED", true)
              this.getTimesheets();
            } else {
              this.notify.warn(this.l(`Approve ${msg} failed`));
            }
          });
        }
      }
    );
  }

  btnReject() {
    let selectedTimesheets = [];
    this.rawData.forEach(item => {
      if (item.checked)
        selectedTimesheets.push(item);
    });
    let rejectTimesheetIds = selectedTimesheets.filter(x => x.status != this.APP_CONSTANT.TimesheetStatus.Reject)
      .map(s => s.id);
    if (rejectTimesheetIds.length == 0) {
      abp.message.error(
        "You have to select at least one Approved or Pending timesheet"
      )
      return;
    }
    var msg = rejectTimesheetIds.length == 1 ? `${rejectTimesheetIds.length} Timesheet` : `${rejectTimesheetIds.length} Timesheets`

    abp.message.confirm(
      `Reject ${msg}`,
      (result: boolean) => {
        if (result) {
          this.timesheetService.rejectTimesheet(rejectTimesheetIds).subscribe((res: any) => {
            if (res) {
              this.getTimesheets();
              this.notify.info('REJECTED <br />' + res.result.success + '<br /> ' + res.result.fail + '<br /> ' + res.result.lockDate);
            } else {
              this.notify.warn(this.l(`Reject ${msg} failed`));
            }
          });
        }
      }
    );
  }

  btnExport(type): void {
    var fileName = "Invoice_TimeSheets_" + this.fromDate + "_" + this.toDate + "_";
    let selectedTimesheets: TimeSheetDto[] = [];
    this.rawData.forEach(item => {
      if (item.checked)
        selectedTimesheets.push(item);
    });
    if (selectedTimesheets.length == 0) {
      abp.message.error(
        "You have to select at least one timesheet"
      )
      return;
    }
    this.exportService.exportTimeSheet(selectedTimesheets, fileName, type);
  }

  convertNumberOfStringStatus(status: number, typeOfWork: number) {
    let html = (typeOfWork != 1) ? '' : '<span class="label label-danger" style="padding: 3px 5px; margin-left: 2npx">OT</span>';
    switch (status) {
      case 0: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-warning'>Draft</span>" + html;
        break;
      }
      case 1: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-info'>Pending</span>" + html;
        break;
      }
      case 2: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-success'>Approved</span>" + html;
        break;
      }
      case 3: {
        html = "<span style='padding: 3px 5px;font-size: 12px;border-radius: 10px;color: #fff;font-weight: 600;' class='label-default'>Rejected</span>" + html;
        break;
      }
    }
    return this.domSanitizer.bypassSecurityTrustHtml(html);
  }
  exportType: number;
  titleExport: string;
  valueSelectedDay: Date = new Date();
  currentDate: Date = new Date();
  notify: NotifyService;
  dialogSelectDay(type: number) {
    this.exportType = type;
    type == 1 ? this.titleExport = "Export Timesheet" : this.titleExport = "Export Remote Request";
    $('#modalSelectDay').modal('show');
  }
  getAlltTimeSheetOrRemote() {
    if (this.valueSelectedDay > this.currentDate) {
      this.notify.error("Không thể chọn ngày lớn hơn ngày hiện tại.")
      return;
    }
    if (this.valueSelectedDay == null) return;
    var datePipe = new DatePipe("en-US");
    var date = datePipe.transform(this.valueSelectedDay, "dd/MM/yyyy")
    this.timesheetService.getAllTimeSheetOrRemote(date, this.exportType).subscribe((res: any) => {
      if (res.result == null) {
        this.notify.warn("Không có ai trong danh sách");
        return;
      }
      this.exportExcel(res.result, this.exportType);
    });
  }
  exportExcel(data, type) {
    let fileName = this.titleExport + " Day " + this.currentDate.getUTCDay() + "- Month " + (this.currentDate.getMonth() + 1) + " - Year " + this.currentDate.getUTCFullYear();
    if (type == 1) this.exportService.exportLogTimeSheetDay(data, fileName);
    else this.exportService.exportRemoteRequestDay(data, fileName);
    $('#modalSelectDay').modal('hide');
  }
}

// export interface DataDTO {
//   customerName?: string,
//   dateAt?: string,
//   id?: number,
//   projectCode?: string,
//   projectName?: string,
//   projectNote?: string,
//   status?: number,
//   taskName?: string,
//   user?: string,
//   workingTime?: number,
// }

export class TimesheetGroupByDayDto {
  dateAt: string;
  totalWorkingTime: number;
  timesheets: TimeSheetDto[];

  checkStatus: number;
  totalCount: number;
  selectedCount: number;
  isThanDefaultWorkingHourPerDay: boolean;
  totalWorkingTimeNormalworkinghours : number;
}

export class TimesheetGroupByUserDto {
  userId: number;
  userName: string;
  avatarFullPath: string;
  branch: number;
  branchName: string;
  type: number;
  level: number;

  totalUserWorkingTime: number;
  dayTasks: TimesheetGroupByDayDto[];

  checkStatus: number;
  totalCount: number;
  selectedCount: number;
  isThanDefaultWorkingHourPerDay: boolean;
}

export class TimesheetGroupByProjectDto {
  projectId: number;
  projectName: string;
  totalProjectWorkingTime: number;
  userTasks: TimesheetGroupByUserDto[];

  checkStatus: number;
  totalCount: number;
  selectedCount: number;
}
export class TimeSheetWarningDto {
  emailAddress: string;
  dateAt: string;
  id: number;
  projectName: string;
  taskName: string;
  mytimesheetNote: string;
  hourOff: number;
  workingTimeHour
}
