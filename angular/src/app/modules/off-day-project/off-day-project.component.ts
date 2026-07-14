import { DayOffType } from './../../service/api/model/absence-day-dto';
import { DatePipe } from '@angular/common';
import { Component, Injector, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog, MatMenuTrigger } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { DayOffService } from '@app/service/api/day-off.service';
import { AbsenceRequestDto } from '@app/service/api/model/absence.dto';
import { GetProjectDto } from '@app/service/api/model/project-Dto';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CalendarEvent, CalendarMonthViewBeforeRenderEvent, CalendarView } from 'angular-calendar';
import * as moment from 'moment';
import { Subject } from 'rxjs';
import { dayOffDTO } from '../day-off/day-off.component';
import { OffDayProjectDetailComponent } from './off-day-project-detail/off-day-project-detail.component';
import { PermissionCheckerService } from 'abp-ng2-module/dist/src/auth/permission-checker.service';
import { AdvancedFilterComponent } from './advanced-filter/advanced-filter.component';

@Component({
  selector: 'app-off-day-project',
  templateUrl: './off-day-project.component.html',
  styleUrls: ['./off-day-project.component.css'],
  providers: [DatePipe]
})
export class OffDayProjectComponent extends AppComponentBase implements OnInit {

  @ViewChild('modalContent') modalContent: TemplateRef<any>;
  @ViewChild(MatMenuTrigger)

  contextMenuPosition = { x: '0px', y: '0px' };

  view: CalendarView = CalendarView.Month;

  absenceRequestList: AbsenceRequestDto[] = [];

  CalendarView = CalendarView;

  viewDate: Date = new Date();

  isEdit = true;

  isLoading = false;

  isShowRejected = false;

  dayOffs: dayOffDTO[] = [];
  isClicked = false;
  stopLop: boolean = true;
  day;
  month;
  year;
  title;
  listYear = APP_CONSTANT.ListYear;
  listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

  modalData: {
    action: string;
    event: CalendarEvent;
  };

  refresh: Subject<any> = new Subject();

  events: any[] = [];

  activeDayIsOpen: boolean = false;

  listProject: GetProjectDto[] = [];
  listProjectFiltered: GetProjectDto[] = [];

  listProjectSelected: number[] = [];

  searchText = "";
  branchId;
  dayAbsentTypeList = Object.keys(this.APP_CONSTANT.DayAbsenceType)
  dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType)
  absentDayType = -1
  dayType = -1
  dayOffType = -1;
  dayOffTypes = [] as DayOffType[];
  dayAbsentStatus = APP_CONSTANT.AbsenceStatusFilter["Pending"];
  dayAbsentStatusList = Object.keys(this.APP_CONSTANT.AbsenceStatusFilter);
  previousAbsentDayType = -1;
  public isRadiocheckfilterbyBranch: boolean;

  constructor(
    injector: Injector,
    private projectService: ProjectManagerService,
    private absenceService: AbsenceRequestService,
    private dayOffService: DayOffService,
    private diaLog: MatDialog,
    private _permissionChecker: PermissionCheckerService,
  ) {
    super(injector);
    // this.updateListYears();
    this.updateDay();
    this.getListProject();
    this.branchId = this.appSession.user.branchId;
    this.isRadiocheckfilterbyBranch = true;
  }

  APPROVAL_ABSENCE_DAY_PROJECT = PERMISSIONS_CONSTANT.ApprovalAbsenceDayByProject;

  public groupedRequests: { date: string, requests: AbsenceRequestDto[] }[] = [];

  private groupRequests() {
    let map = new Map<string, AbsenceRequestDto[]>();
    this.absenceRequestList.forEach(item => {
      const itemDate = moment(item.dateAt, 'YYYY-MM-DD');
      if (itemDate.month() === this.month && itemDate.year() === this.year) {
        let dateStr = itemDate.format('DD/MM/YYYY');
        if (!map.has(dateStr)) {
          map.set(dateStr, []);
        }
        map.get(dateStr).push(item);
      }
    });
    let result = [];
    map.forEach((value, key) => {
      result.push({ date: key, requests: value });
    });
    result.sort((a, b) => moment(b.date, 'DD/MM/YYYY').toDate().getTime() - moment(a.date, 'DD/MM/YYYY').toDate().getTime());
    this.groupedRequests = result;
  }

  trackByDate(index: number, group: any) {
    return group.date;
  }

  trackByRequestId(index: number, item: AbsenceRequestDto) {
    return item.id;
  }

  getLeaveTypeText(member: AbsenceRequestDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay) {
        return "Đi muộn";
      }
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return "Về sớm";
      }
      return "Off";
    } else if(member.leavedayType == 1) {
      return "Onsite";
    } else if(member.leavedayType == 2) {
      return "Remote";
    }
  }

  getLeaveText(member: AbsenceRequestDto) {
    if (member.dateType == 1) {
      return "Full Day";
    }
    if (member.dateType == 2) {
      return "Morning";
    }
    if (member.dateType == 3) {
      return "Afternoon";
    }
    return member.hour + "h";
  }

  getLabel(userType: number): string {
    const userTypes = [
      { value: 0, label: 'Staff' },
      { value: 1, label: 'Intern' },
      { value: 2, label: 'CTV' },
      { value: 3, label: 'Probation' },
      { value: 5, label: 'Vendor' }
    ];
    const found = userTypes.find(u => u.value === userType);
    return found ? found.label : 'Unknown';
  }

  getListTypeClasses(member: AbsenceRequestDto) {
    if (member.leavedayType == 0) {
      if (member.absenceTime == this.APP_CONSTANT.OnDayType.BeginOfDay
        || member.absenceTime == this.APP_CONSTANT.OnDayType.EndOfDay) {
        return ['text-primary', 'day-chip-tardiness-leave-early'];
      }
      return ['text-primary', 'day-chip-full-day'];
    }  else if (member.leavedayType == 1) {
      return ['text-danger', 'onsite'];
    }
    return ['text-primary', 'day-chip-morning'];
  }

  getListClasses(member: AbsenceRequestDto) {
    if (member.dateType == 1) {
      return ['text-primary', 'day-chip-full-day'];
    }
    if (member.dateType == 2) {
      return ['text-primary', 'day-chip-morning'];
    }
    if (member.dateType == 3) {
      return ['text-primary', 'day-chip-afternoon'];
    }
    return ['text-primary', 'day-chip-custom'];
  }

  onApproveAbsence(item: AbsenceRequestDto) {
    let data = [];
    data.push(item.id);
    this.isLoading = true;
    this.absenceService.approveAbsenceRequest(data).subscribe((res) => {
      if (res) {
        this.absenceRequestList.forEach(abs => {
          if (abs.id === item.id) {
            abs.status = 2;
          }
        });
        this.events.forEach(ev => {
          if (ev.meta && ev.meta.id === item.id) {
            ev.meta.status = 2;
          }
        });
        this.groupRequests();
        this.notify.success(this.l("Approve Successfully!"));
      }
      this.isLoading = false;
      this.refresh.next();
    }, (error) => {
      this.isLoading = false;
    });
  }

  onRejectAbsence(item: AbsenceRequestDto) {
    let data = [];
    data.push(item.id);
    this.isLoading = true;
    this.absenceService.rejectAbsenceRequest(data).subscribe((res) => {
      if (res) {
        this.absenceRequestList.forEach(abs => {
          if (abs.id === item.id) {
            abs.status = 3;
          }
        });
        this.events.forEach(ev => {
          if (ev.meta && ev.meta.id === item.id) {
            ev.meta.status = 3;
          }
        });
        this.groupRequests();
        this.notify.success(this.l("Reject Successfully!"));
      }
      this.isLoading = false;
      this.refresh.next();
    }, (error) => {
      this.isLoading = false;
    });
  }

  ngOnInit() {
    this.getAllAbsenceType();
    this.dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType).filter(key => this.APP_CONSTANT.AbsenceType[key] !== 4);
  }
  getAllAbsenceType() {
    this.dayOffService.getAllDayOffType().subscribe(resp => {
      this.dayOffTypes = resp.result as DayOffType[];
    });
  }

  onDayOffTypeChange(event?, isRefresh = true) {
    let date = new Date(this.year, this.month);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    if (isRefresh) {
      this.getDayOff();
    }
  }

  onLeaveDayTypeChange(isRefresh = true): void {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    if (isRefresh) {
      this.getDayOff();
    }
  }
  onDayTypeChange(isRefresh = true) {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate();
    if (isRefresh) {
      this.getDayOff();
    }
  }
  onChangeSelect(event?, isRefresh = true): void {
    this.listProjectSelected = event.value;
    let unselectedIds = this.listProject.map(p => p.id).filter(id => this.listProjectSelected.indexOf(id) === -1);
    localStorage.setItem('manageRequest_Off_Remote_Onsite_ListProjectIdUnselected', unselectedIds.toString());
    if (isRefresh) {
      this.getDayOff();
    }
  }

  getListProject(isRefresh = true) {
    this.isLoading = true;
    this.projectService.getProjectPM().subscribe(res => { // get list project cua PM
      this.listProject = res.result;
      let data = localStorage.getItem("manageRequest_Off_Remote_Onsite_ListProjectIdUnselected");
      this.listProject.forEach(item => {
        if (data == null || data == "") {
          //if(!this._permissionChecker.isGranted('AbsenceDayByProject.ViewByBranch')) {
            this.listProjectSelected.push(item.id);
          //}
        }
        if (item.code) {
          item.name = item.code + " - " + item.name;
        }
      });
      if (data !== null && data !== '') {
        let unselectedIds = data.split(",").map(v => Number.parseInt(v));
        this.listProject.forEach(project => {
          if (unselectedIds.indexOf(project.id) === -1) {
            //if(!this._permissionChecker.isGranted('AbsenceDayByProject.ViewByBranch')) {
              this.listProjectSelected.push(project.id);
            //}
          }
        });
      }
      if (isRefresh) {
        this.getDayOff();
      } else {
        this.isLoading = false;
      }

    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    });
  }

  onChangeListProjectIdSelected(event, isRefresh = true) {
    this.listProjectSelected = event;
    let unselectedIds = this.listProject.map(p => p.id).filter(id => this.listProjectSelected.indexOf(id) === -1);
    localStorage.setItem('manageRequest_Off_Remote_Onsite_ListProjectIdUnselected', unselectedIds.toString());
    if (isRefresh) {
      this.getDayOff();
    }
  }


  onFilter() {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.getDayOff();
  }

  onFilterBranchDirector(isRefresh = true) {
    this.listProjectSelected = [];
    if (isRefresh) {
      this.getDayOff();
    }
  }

  updateDay(): void {
    this.day = this.viewDate.getDate();
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }

  // updateListYears() {
  //   this.listYear = [];
  //   for (let i = this.viewDate.getFullYear() - 5; i <= this.viewDate.getFullYear() + 5; i++) {
  //     this.listYear.push(i);
  //   }
  // }

  closeOpenMonthViewDay() {
    this.activeDayIsOpen = false;
    this.isLoading = true;
    this.updateDay();
    // this.updateListYears();
    this.getDayOff();
  }

  getData(renderEvent: CalendarMonthViewBeforeRenderEvent) {
    renderEvent.body.forEach(day => {
      if (this.dayOffs.findIndex(data => moment(data.dayOff, 'YYYY-MM-DD').toDate().getDate() == day.date.getDate() && moment(data.dayOff, 'YYYY-MM-DD').toDate().getMonth() == day.date.getMonth()) >= 0) {
        day.cssClass = 'back-red';
      }
    });
    ///
  }
  getDayOff(){
    this.dayOffService.getAll(this.month + 1, this.year, this.branchId).subscribe(res => { // get day off
      this.isLoading = false;
      this.dayOffs = res.result;
      this.refreshData();
    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    });
  }

  refreshData() {
    let typeAbsenceDay = this.absentDayType;
    this.updateDay();
    // this.updateListYears();
    this.events = [];
    const startDate = moment(this.viewDate).startOf("M").subtract(7, "d").format("YYYY-MM-DD");
    const endDate = moment(this.viewDate).endOf("M").add(7, "d").format("YYYY-MM-DD");
    this.isLoading = true;
    if(this.absentDayType !== APP_CONSTANT.DayAbsenceType['Off']){
      this.dayOffType = APP_CONSTANT.FILTER_DEFAULT.All;
      this.dayType = APP_CONSTANT.FILTER_DEFAULT.All;
    }
    if(this.absentDayType === 3){
      this.dayType = 4;
      typeAbsenceDay = 0;
      this.absentDayType = 3;
    }
    if (this.previousAbsentDayType === 3 && this.absentDayType === APP_CONSTANT.DayAbsenceType["Off"]) {
      this.dayType = APP_CONSTANT.FILTER_DEFAULT.All;
    }
    this.previousAbsentDayType = this.absentDayType;
    this.absenceService.getAllRequestAbsence(startDate, endDate, this.listProjectSelected, this.searchText, typeAbsenceDay, this.dayOffType, this.dayAbsentStatus, this.dayType).subscribe(res => {
      this.isLoading = false;
      this.absenceRequestList = res.result;
      this.groupRequests();
      this.absenceRequestList.forEach(item => {
        if (!(this.absentDayType === 0 && item.dateType === 4)) {
          this.events.push({
            start : moment(item.dateAt, 'YYYY-MM-DD').toDate(),
            end: moment(item.dateAt, 'YYYY-MM-DD').toDate(),
            avatarFullPath: item.avatarFullPath,
            color: { primary: item.dateType.toString() + ' | ' + item.hour, secondary: item.name },
            meta: item,
            absenceTime: item.absenceTime,
          });
        }
      })
      this.refresh.next();
    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    })
  }

  dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }) {
    if (!this.permission.isGranted(PERMISSIONS_CONSTANT.ViewDetailAbsenceDayByProject)) return;
    const eventOfDay = this.absenceRequestList.filter(event => (moment(event.dateAt, 'YYYY-MM-DD').toDate().getDate() == date.getDate() && moment(event.dateAt, 'YYYY-MM-DD').toDate().getMonth() == date.getMonth()) && !(this.absentDayType === 0 && event.dateType === 4));
    if (eventOfDay && eventOfDay.length) {
      const dialogRef = this.diaLog.open(OffDayProjectDetailComponent, {
        //disableClose: true,
        width: "1320px",
        data: { events: eventOfDay, date: date }
      });

      dialogRef.afterClosed().subscribe(() => {
        this.refreshData();
      })
    }
    // }
  }



  onChangeShowRejected(event: any) {
    this.isShowRejected = event.checked;
    this.refreshData();
  }

  onChangeShowFilterByBranch(isRefresh = true) {
    this.isRadiocheckfilterbyBranch = this.isRadiocheckfilterbyBranch == true ? false : true;
    if(this.isRadiocheckfilterbyBranch == true) {
      this.getListProject(isRefresh);
    } else if (isRefresh) {
      this.getDayOff();
    }
  }

  openAdvancedFilter() {
    this.diaLog.open(AdvancedFilterComponent, {
      data: this,
      width: '100%',
      maxWidth: '100vw',
      panelClass: 'manage-team-filter-dialog',
      restoreFocus: false
    });
  }
}
