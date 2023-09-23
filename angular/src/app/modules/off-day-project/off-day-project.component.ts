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
import { moment } from 'ngx-bootstrap/chronos/test/chain';
import { Subject } from 'rxjs';
import { dayOffDTO } from '../day-off/day-off.component';
import { OffDayProjectDetailComponent } from './off-day-project-detail/off-day-project-detail.component';
import { PermissionCheckerService } from 'abp-ng2-module/dist/src/auth/permission-checker.service';

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
  dayAbsentStatusList = Object.keys(this.APP_CONSTANT.AbsenceStatusFilter)

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
  }

  ngOnInit() {
    this.getAllAbsenceType();
  }
  getAllAbsenceType() {
    this.dayOffService.getAllDayOffType().subscribe(resp => {
      this.dayOffTypes = resp.result as DayOffType[];
    });
  }

  onDayOffTypeChange() {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.getDayOff();
  }

  onLeaveDayTypeChange(): void {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.getDayOff();
  }
  onDayTypeChange() {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate();
    this.getDayOff();
  }
  onChangeSelect(event?): void {
    this.listProjectSelected = event.value;
    localStorage.setItem('manageRequest_Off_Remote_Onsite_ListProjectIdSelected', this.listProjectSelected.toString());
    this.getDayOff();
  }

  getListProject() {
    this.isLoading = true;
    this.projectService.getProjectPM().subscribe(res => { // get list project cua PM
      this.listProject = res.result;
      let data = localStorage.getItem("manageRequest_Off_Remote_Onsite_ListProjectIdSelected");
      this.listProject.forEach(item => {
        if (data == null || data == "") {
          if(!this._permissionChecker.isGranted('AbsenceDayByProject.ViewByBranch')) {
            this.listProjectSelected.push(item.id);
          }
        }
        if (item.code) {
          item.name = item.code + " - " + item.name;
        }
      });
      if (data !== null && data !== '') {
        data.split(",").forEach((value: string) => {
          if (this.listProject.some(project => project.id === Number.parseInt(value))) {
            if(!this._permissionChecker.isGranted('AbsenceDayByProject.ViewByBranch')) {
              this.listProjectSelected.push(Number.parseInt(value));
            }
          }
        });
      }
      this.getDayOff();

    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    });
  }

  onChangeListProjectIdSelected(event) {
    this.listProjectSelected = event;
    localStorage.setItem('manageRequest_Off_Remote_Onsite_ListProjectIdSelected', event.toString());
    this.getDayOff();
  }


  onFilter() {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.getDayOff();
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
    this.absenceService.getAllRequestAbsence(startDate, endDate, this.listProjectSelected, this.searchText, this.absentDayType, this.dayOffType, this.dayAbsentStatus, this.dayType).subscribe(res => {
      this.isLoading = false;
      this.absenceRequestList = res.result;
      this.absenceRequestList.forEach(item => {
          this.events.push({
            start : moment(item.dateAt, 'YYYY-MM-DD').toDate(),
            end: moment(item.dateAt, 'YYYY-MM-DD').toDate(),
            avatarFullPath: item.avatarFullPath,
            color: { primary: item.dateType.toString() + ' | ' + item.hour, secondary: item.name },
            meta: item.leavedayType,
            absenceTime: item.absenceTime,
          });
      })
      this.refresh.next();
    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    })
  }

  dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }) {
    if (!this.permission.isGranted(PERMISSIONS_CONSTANT.ViewDetailAbsenceDayByProject)) return;
    const eventOfDay = this.absenceRequestList.filter(event => moment(event.dateAt, 'YYYY-MM-DD').toDate().getDate() == date.getDate() && moment(event.dateAt, 'YYYY-MM-DD').toDate().getMonth() == date.getMonth());
    if (eventOfDay && eventOfDay.length) {
      const dialogRef = this.diaLog.open(OffDayProjectDetailComponent, {
        disableClose: true,
        width: "1320px",
        data: { events: eventOfDay, date: date }
      });

      dialogRef.afterClosed().subscribe(() => {
        this.refreshData();
      })
    }
    // }
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

}
