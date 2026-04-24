  import { DayOffType } from './../../service/api/model/absence-day-dto';
  import { DatePipe } from '@angular/common';
  import { Component, Injector, OnInit, TemplateRef, ViewChild } from '@angular/core';
  import { MatDialog, MatMenuTrigger } from '@angular/material';
  import { APP_CONSTANT } from '@app/constant/api.constants';
  import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
  import { AbsenceRequestService } from '@app/service/api/absence-request.service';
  import { DayOffService } from '@app/service/api/day-off.service';
  import { AbsenceRequestDto, CountRequestDto } from '@app/service/api/model/absence.dto';
  import { GetProjectDto } from '@app/service/api/model/project-Dto';
  import { ProjectManagerService } from '@app/service/api/project-manager.service';
  import { AppComponentBase } from '@shared/app-component-base';
  import { AppConsts } from '@shared/AppConsts';
  import { CalendarEvent, CalendarMonthViewBeforeRenderEvent, CalendarView } from 'angular-calendar';
  import * as moment from 'moment';
  import { Subject } from 'rxjs';
  import { dayOffDTO } from '../day-off/day-off.component';
  import { PopupComponent } from './popup/popup.component';
  import { ExportDataComponent } from './export-data/export-data.component';
  import { AdvancedFilterComponent } from './advanced-filter/advanced-filter.component';

  @Component({
    selector: 'app-off-day-project-for-user',
    templateUrl: './off-day-project-for-user.component.html',
    styleUrls: ['./off-day-project-for-user.component.css']
  })

export class OffDayProjectForUserComponent extends AppComponentBase implements OnInit {
  Export_Team_Working_Calender = PERMISSIONS_CONSTANT.ExportTeamWorkingCalender;
  @ViewChild('modalContent') modalContent: TemplateRef<any>;
  @ViewChild(MatMenuTrigger)

  contextMenuPosition = { x: '0px', y: '0px' };
  view: CalendarView = CalendarView.Month;
  absenceRequestList: AbsenceRequestDto[] = [];
  countRequestList: CountRequestDto [] = [];
  CalendarView = CalendarView;
  viewDate: Date = new Date();
  isEdit = true;
  isLoading = false;
  isDisabled = false;
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
  dayAbsentTypeList = Object.keys(this.APP_CONSTANT.DayAbsenceType)
  dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType)
  remoteOfWeekList = [1,2,3,4,5];
  absentDayType = -1;
  dayType = -1;
  remoteOfWeek=-1;
  dayOffType = -1;
  dayOffTypes = [] as DayOffType[];
  modalData: {
    action: string;
    event: CalendarEvent;
  };
  dayAbsentStatus = APP_CONSTANT.AbsenceStatusFilter['Pending'];
  dayAbsentStatusList = Object.keys(this.APP_CONSTANT.AbsenceStatusFilter)
  refresh: Subject<any> = new Subject();
  events: any[] = [];
  activeDayIsOpen: boolean = false;
  listProject: GetProjectDto[] = [];
  listProjectFiltered: GetProjectDto[] = [];
  listProjectSelected: number[] = [];
  searchText = "";
  branchId;
  notifyPmAbsenceDayOfTeam = PERMISSIONS_CONSTANT.NotifyPmAbsenceDayOfTeam;
  previousAbsentDayType = -1;

  constructor(
    injector: Injector,
    private projectService: ProjectManagerService,
    private absenceService: AbsenceRequestService,
    private dayOffService: DayOffService,
    private diaLog: MatDialog
  ) {
    super(injector);
    // this.updateListYears();
    this.updateDay();
    this.getListProject();
    this.branchId = this.appSession.user.branchId;
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

  public groupedRequests: { date: string, rawDate: Date, requests: AbsenceRequestDto[], isLoading: boolean }[] = [];

  private buildGroupedRequests() {
    let map = new Map<string, Date>();
    this.countRequestList.forEach(item => {
      if (item.count > 0) {
        const itemDate = moment(item.date, 'YYYY-MM-DD');
        if (itemDate.month() === this.month && itemDate.year() === this.year) {
          let dateStr = itemDate.format('DD/MM/YYYY');
          if (!map.has(dateStr)) {
            map.set(dateStr, itemDate.toDate());
          }
        }
      }
    });
    let result = [];
    map.forEach((value, key) => {
      result.push({ date: key, rawDate: value, requests: [], isLoading: false });
    });
    result.sort((a, b) => b.rawDate.getTime() - a.rawDate.getTime());
    this.groupedRequests = result;
  }

  loadGroupRequests(group: any) {
    if (group.requests.length === 0 && !group.isLoading) {
      group.isLoading = true;
      let typeAbsenceDay = this.absentDayType;
      if(this.absentDayType === 3){
        typeAbsenceDay = 0;
      }
      this.absenceService.getAllRequestForUserByDay(group.rawDate, this.listProjectSelected, this.searchText, typeAbsenceDay, this.dayOffType, this.dayAbsentStatus, this.dayType, this.remoteOfWeek).subscribe(res => {
        let eventOfDay = res.result;
        if (this.absentDayType === 0) {
          eventOfDay = eventOfDay.filter(event => event.absenceTime !== 1 && event.absenceTime !== 2 && event.absenceTime !== 3);
        }
        group.requests = eventOfDay;
        group.isLoading = false;
      }, () => {
        group.isLoading = false;
      });
    }
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

  openAdvancedFilter() {
    this.diaLog.open(AdvancedFilterComponent, {
      data: this,
      width: '100%',
      maxWidth: '100vw',
      panelClass: 'user-team-filter-dialog',
      restoreFocus: false
    });
  }

  onDayOffTypeChange(event?, isRefresh = true) {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate();
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

  onRemoteOfWeekChange(isRefresh = true) {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate();
    if (isRefresh) {
      this.getDayOff();
    }
  }

  selectionChange(event?, isRefresh = true): void {
    let date = new Date(this.year, this.month);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate();
    if (isRefresh) {
      this.getDayOff();
    }
  }

  onChangeSelect(event?): void {
    this.listProjectSelected = event.value;
    let unselectedIds = this.listProject.map(p => p.id).filter(id => this.listProjectSelected.indexOf(id) === -1);
    localStorage.setItem('listProjectIdsOfUserUnselected', unselectedIds.toString());
    this.refreshData();
  }

  onFilter() {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.getDayOff();
  }

  getListProject() {
    this.isLoading = true;
    this.projectService.getProjectUser().subscribe(res => { // get list project cua PM
      this.listProject = res.result;
      let data = localStorage.getItem("listProjectIdsOfUserUnselected");
      this.listProject.forEach(item => {
        if (data == null || data == "") {
          this.listProjectSelected.push(item.id);
        }
        if (item.code) {
          item.name = item.code + " - " + item.name;
        }
      });

      if (data !== null && data !== '') {
        let unselectedIds = data.split(",").map(v => Number.parseInt(v));
        this.listProject.forEach(project => {
          if (unselectedIds.indexOf(project.id) === -1) {
            this.listProjectSelected.push(project.id);
          }
        });
      }

      this.getDayOff();

    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    });
  }

  onChangeListProjectIdSelected(event, isRefresh = true) {
    this.listProjectSelected = event;
    let unselectedIds = this.listProject.map(p => p.id).filter(id => this.listProjectSelected.indexOf(id) === -1);
    localStorage.setItem('listProjectIdsOfUserUnselected', unselectedIds.toString());
    if (isRefresh) {
      this.refreshData();
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
    this.getDayOff();
  }

  getData(renderEvent: CalendarMonthViewBeforeRenderEvent) {
    renderEvent.body.forEach(day => {
      if (this.dayOffs.findIndex(data => moment(data.dayOff, 'YYYY-MM-DD').toDate().getDate() == day.date.getDate() && moment(data.dayOff, 'YYYY-MM-DD').toDate().getMonth() == day.date.getMonth()) >= 0) {
        day.cssClass = 'back-red';
      }
    });
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
    if(this.absentDayType !== APP_CONSTANT.DayAbsenceType['Remote']){
      this.remoteOfWeek = APP_CONSTANT.FILTER_DEFAULT.All;
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
    this.absenceService
      .getCountAllRequestAbsenceOfTeam(
        startDate,
        endDate,
        this.listProjectSelected,
        this.searchText,
        typeAbsenceDay,
        this.dayOffType,
        this.dayAbsentStatus,
        this.dayType,
        this.remoteOfWeek
      )
      .subscribe(
        (res) => {
          this.isLoading = false;
          this.countRequestList = res.result;
          if ($) {
            $(".mobile-requests.collapse").collapse("hide");
          }
          this.countRequestList.forEach((item) => {
            this.events.push({
              count: item.count,
              date: moment(item.date, "YYYY-MM-DD").toDate(),
              meta: item.type,
              absenceType: item.absenceType,
            });
          });
          this.sortEvents();
          this.buildGroupedRequests();
          this.refresh.next();
        },
        () => {
          this.isLoading = false;
          this.notify.error("An error has occured!");
        }
      );
  }

  sortEvents() {
    this.events.sort((a, b) => {
      if (a.meta == 0 && (a.absenceType != 1 && a.absenceType != 2 && a.absenceType != 3)) {
        return -1;
      }
      if (b.meta == 0 && (b.absenceType != 1 && b.absenceType != 2 && b.absenceType != 3)) {
        return 1;
      }
      return 0;
    });
  }

  dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }) {
    let typeAbsenceDay = this.absentDayType;
    if(this.absentDayType === 3){
      this.dayType = 4;
      typeAbsenceDay = 0;
      this.absentDayType = 3;
    }
    if(this.absentDayType !== APP_CONSTANT.DayAbsenceType['Remote']){
      this.remoteOfWeek = APP_CONSTANT.FILTER_DEFAULT.All;
    }
    if (!this.permission.isGranted(PERMISSIONS_CONSTANT.ViewDetailAbsenceDayOfTeam)) return;
    const countRequestForDay = this.countRequestList.find(item => moment(item.date).isSame(date, 'day'));
    if (countRequestForDay && countRequestForDay.count > 0) {
      this.isLoading = true;
      this.absenceService.getAllRequestForUserByDay(date, this.listProjectSelected, this.searchText, typeAbsenceDay, this.dayOffType, this.dayAbsentStatus, this.dayType, this.remoteOfWeek).subscribe(res => {
        this.absenceRequestList = res.result;
        let eventOfDay = this.absenceRequestList.filter(event => moment(event.dateAt, 'YYYY-MM-DD').toDate().getDate() == date.getDate() && moment(event.dateAt, 'YYYY-MM-DD').toDate().getMonth() == date.getMonth());
        if (this.absentDayType === 0) {
          eventOfDay = eventOfDay.filter(event => event.absenceTime !== 1 && event.absenceTime !== 2 && event.absenceTime !== 3);
        }
        if (eventOfDay && eventOfDay.length) {
          this.diaLog.open(PopupComponent, {
            disableClose: true,
            width: "1240px",
            data: { events: eventOfDay, date: date }
          });
        }
        this.isLoading = false;
      })
    }
  }

  onExport() {
    this.diaLog.open(ExportDataComponent, {
      width: "500px",
    });
  }

  getAbsenceText(type, hour, meta) {

    if (type == this.APP_CONSTANT.AbsenceType.FullDay) {
      return 'Full day';
    }
    if (type == this.APP_CONSTANT.AbsenceType.Morning) {
      return 'Morning';
    }
    if (type == this.APP_CONSTANT.AbsenceType.Afternoon) {
      return 'Afternoon';
    }
    return `${hour}h`;
  }

  getAbsenceClasses(type) {
    if (type == this.APP_CONSTANT.AbsenceType.FullDay) {
      return ['day-chip-full-day', 'pin-wrap'];
    }
    if (type == this.APP_CONSTANT.AbsenceType.Morning) {
      return ['day-chip-morning', 'pin-wrap'];
    }
    if (type == this.APP_CONSTANT.AbsenceType.Custom) {
      return ['day-chip-custom', 'pin-wrap'];
    }
    return ['day-chip-afternoon', 'pin-wrap'];
  }

  onChangeShowRejected(event: any) {
    this.isShowRejected = event.checked;
    this.refreshData();
  }

  shouldShowNotifyPmButton(): boolean {
    const hasNotifyPmRole = this.permission.isGranted(this.notifyPmAbsenceDayOfTeam);
    const hasPendingStatus = this.dayAbsentStatus === APP_CONSTANT.AbsenceStatusFilter.Pending;

    return hasNotifyPmRole && hasPendingStatus;
  }


  notifyApproveRequestOffToPM() {
    const startDate = moment(this.viewDate).startOf("M").subtract(7, "d").format("YYYY-MM-DD");
    const endDate = moment(this.viewDate).endOf("M").add(7, "d").format("YYYY-MM-DD");
    abp.message.confirm("Confirm sending notification to PM?",
      (result: boolean) => {
        if (result) {
          this.absenceService.notifyApproveRequestOffToPM(startDate, endDate, this.listProjectSelected, this.searchText, this.absentDayType, this.dayOffType, this.dayAbsentStatus, this.dayType).subscribe(resp => {
            if (resp.success == true) {
              abp.message.success("Send successful!");
            }
          });
        }
      }
    )
  }
}
