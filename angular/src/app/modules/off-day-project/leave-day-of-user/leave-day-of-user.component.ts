import { DatePipe } from '@angular/common';
import { Component, Injector, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { dayOffDTO } from '@app/modules/day-off/day-off.component';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { DayOffService } from '@app/service/api/day-off.service';
import { AbsenceRequestDto } from '@app/service/api/model/absence-day-dto';
import { CalendarEvent, CalendarMonthViewBeforeRenderEvent, CalendarView } from 'angular-calendar';
import * as moment from 'moment';
import { Subject } from 'rxjs';
import { MonthViewDay } from '@node_modules/calendar-utils';
import { AppComponentBase } from '@shared/app-component-base';
import { OffDayProjectDetailComponent } from '../off-day-project-detail/off-day-project-detail.component';
import { MatDialog } from '@angular/material';
import { UserService } from '@app/service/api/user.service';
import { ConfirmAllRequestComponent } from '../confirm-all-request/confirm-all-request.component';

@Component({
  selector: 'app-leave-day-of-user',
  templateUrl: './leave-day-of-user.component.html',
  styleUrls: ['./leave-day-of-user.component.css'],
  providers: [DatePipe]
})

export class LeaveDayOfUserComponent extends AppComponentBase implements OnInit {

  userId: number;
  year;
  years;
  month;
  months;
  day;
  viewDate: Date;
  calendarView;
  absenceReqs: AbsenceRequestDto[];
  userName: string;
  activeDayIsOpen: boolean = false;
  view: CalendarView;
  title;
  listDayShow: AbsenceRequestDto[];
  selectedDays: Map<string, any>;
  events: CalendarEvent[] = [];
  dayOffs: dayOffDTO[] = [];
  isLoading: boolean;
  refresh: Subject<any> = new Subject();
  monthViewBody: MonthViewDay[];
  branchId;
  isShowRejected: boolean = false;
  dayAbsentTypeList = Object.keys(this.APP_CONSTANT.DayAbsenceType)
  dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType)
  dayAbsentStatus = this.APP_CONSTANT.AbsenceStatusFilter["Pending"];
  dayAbsentStatusList = Object.keys(this.APP_CONSTANT.AbsenceStatusFilter);
  absentDayType = -1
  dayType = -1
  multiSelect = false;
  selectAll = false;

  constructor(
    injector: Injector,
    private activatedRoute: ActivatedRoute,
    private absenceRequestService: AbsenceRequestService,
    private datePipe: DatePipe,
    private dayOffService: DayOffService,
    private diaLog: MatDialog,
    private userService: UserService,
  ) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.absenceReqs = [] as AbsenceRequestDto[];
    this.listDayShow = [] as AbsenceRequestDto[];
    this.selectedDays = new Map<string, any>();
    this.view = CalendarView.Month;
    this.viewDate = new Date();
    this.calendarView = CalendarView;
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;
  }

  ngOnInit() {
    this.userId = this.activatedRoute.snapshot.params.id;
    this.userService.getOneUser(this.userId).subscribe(res => {
      this.userName = res.result.fullName;
    })
    this.updateDay();
    this.refreshData();
  }

  updateDay(): void {
    this.day = this.viewDate.getDate();
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }

  refreshData() {
    this.multiSelect = false;
    this.selectAll = false;
    this.listDayShow = [];
    this.selectedDays.clear()
    this.isLoading = true
    this.updateDay();
    this.dayOffs = [];
    this.events = [];
    this.title = this.datePipe.transform(this.viewDate, "MM-yyyy");
    this.dayOffService.getAll(this.viewDate.getMonth() + 1, this.viewDate.getFullYear(), this.branchId).subscribe(data => {
      this.dayOffs = data.result;
      this.dayOffs.forEach(day => {
        this.events.push({
          start: new Date(day.dayOff),
          end: new Date(day.dayOff),
          title: day.name,
          id: day.id,
          color: { primary: day.coefficient.toString(), secondary: "" }
        } as CalendarEvent)
      });
      this.refresh.next();

      let sDate = moment(new Date(this.year, this.month - 1, 1)).format("YYYY-MM-DD");
      let tDate = moment(new Date(this.year, this.month + 2, 0)).format("YYYY-MM-DD");
      this.absenceRequestService.getAllRequestByUserId(sDate, tDate, this.userId, this.absentDayType, this.dayType, this.dayAbsentStatus).subscribe(resp => {
        this.absenceReqs = resp.result;
        this.monthViewBody.forEach((d: any) => {
          if (moment().isAfter(d.date) || d.date.getDay() === 0) {
            d['isOut'] = true;
          }

          let temp = [];
          this.absenceReqs.forEach(req => {
            if (moment(req.detail.dateAt, 'YYYY-MM-DD').toDate().toLocaleDateString() === d.date.toLocaleDateString()) {
              let item = {
                cssClass: (req.detail.dateType === 1 ? 'all-day-absence' : req.detail.dateType === 2 ? 'morning-absence' : (req.detail.dateType === 3 ? 'afternoon-absence' : req.detail.dateType === 4 ? 'custom-absence-time' : '')),
                status: req.status,
                type: req.type,
                dateType: req.detail.dateType,
                absenceTime: req.detail.absenceTime,
                hour: req.detail.hour
              };
              d.events.push(item);
            }
          });

          if (temp[temp.length - 1]) {
            this.listDayShow.push(temp[temp.length - 1]);
          }

          this.selectedDays.forEach((value, key) => {
            if (d.date.toLocaleDateString() === key) {
              d.cssClass = (value === 1 ? 'all-day-absence' : (value === 2 ? 'morning-absence' : value === 3 ? 'afternoon-absence' : ''));
            }
          });
        });
        this.isLoading = false
      });
    });
  }
  absentDayTypeChange() {
    this.dayType = -1;
  }

  checkApprove(data: AbsenceRequestDto[]): boolean {
    if (this.isShowRejected) {
      return true;
    }
    return data.some(d => d.status !== this.APP_CONSTANT.AbsenceStatus.Rejected);
  }

  dayClicked(day: any) {
    const formatDate = moment(day.date).format("YYYY-MM-DD");
    if (this.multiSelect && day.events.length > 0) {
      const haveData = this.absenceReqs.filter(day => day.detail.dateAt === formatDate);
      const haveDateId = haveData.map(s => s.id);
      if (this.selectedDays.has(formatDate)) {
        this.selectedDays.delete(formatDate);
      } else {
        this.selectedDays.set(formatDate, haveDateId);
      }
      return;
    }
    let eventOfDay;
    const haveData = this.absenceReqs.some(day => day.detail.dateAt === formatDate);
    if (haveData) {
      this.isLoading = true;
      this.absenceRequestService.getUserRequestByDate(formatDate, this.userId).subscribe(res => {
        eventOfDay = res.result as AbsenceRequestDto[];
        this.isLoading = false;
        if (eventOfDay && eventOfDay.length) {
          const dialogRef = this.diaLog.open(OffDayProjectDetailComponent, {
            disableClose: true,
            width: "1224px",
            data: { events: eventOfDay, date: day.date }
          });

          dialogRef.afterClosed().subscribe(() => {
            this.refreshData();
          })
        }
      });
    }
  }

  clickMultiSelect(value){
    this.multiSelect=value;
    if(!value){
      this.selectedDays.clear()
    }
  }

  clickSelectAll(value){
    this.selectedDays.clear();
    if(value){
      const currentMonth = new Date().getMonth();
      const absencePending = this.absenceReqs.filter(day => {
          const dateMonth = new Date(day.detail.dateAt).getMonth();
          return dateMonth === currentMonth && day.status === this.APP_CONSTANT.AbsenceStatus.Pending;
      });
      absencePending.forEach(s => {
        if (this.selectedDays.has(s.detail.dateAt)) {
          let existingArray = this.selectedDays.get(s.detail.dateAt);
          existingArray.push(s.id);
          this.selectedDays.set(s.detail.dateAt, existingArray);
        } else {
          this.selectedDays.set(s.detail.dateAt, [s.id]);
        }
      });
    }
  }

  getData(renderEvent: CalendarMonthViewBeforeRenderEvent): void {
    this.monthViewBody = renderEvent.body;
    this.monthViewBody.forEach(day => {
      if (this.events.findIndex(data => data.start.getDate() == day.date.getDate()) >= 0 && day.inMonth) {
        day.cssClass = 'back-red';
      }
    });
  }

  getClassByStatus(status: number) {
    switch(status){
      case this.APP_CONSTANT.AbsenceStatus.Pending: return "day-off-state-pending";
      case this.APP_CONSTANT.AbsenceStatus.Approved: return "day-off-state-approved";
      case this.APP_CONSTANT.AbsenceStatus.Rejected: return "day-off-state-reject";
    }
  }

  selectionChange(resetDayType: boolean) {
    if (resetDayType) this.dayType = -1;
    this.viewDate = new Date(this.year, this.month);
    this.updateDay();
    this.refreshData();
  }

  closeOpenMonthViewDay() {
    this.activeDayIsOpen = false;
    this.ngOnInit();
  }

  onChangeShowRejected(event) {
    this.isShowRejected = event.checked;
    this.refreshData();
  }

  onBack() {
    history.back();
  }

  ApproveOrReject(status : boolean) {
    const keysArray = new Set(this.selectedDays.keys());
    const filteredData = this.absenceReqs.filter(item => keysArray.has(item.detail.dateAt));
    const dialogRef = this.diaLog.open(ConfirmAllRequestComponent, {
      disableClose: true,
      width: "auto",
      data: { events: this.selectedDays, status, data: filteredData }
    });
    dialogRef.afterClosed().subscribe(status => {
      if(status) this.refreshData();
    })
  }

  isSelected(day : any){
    return this.selectedDays.has(moment(day).format("YYYY-MM-DD"));
  }
}
