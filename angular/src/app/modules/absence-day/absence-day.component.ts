import { DatePipe } from '@angular/common';
import { Component, Injector, OnInit } from '@angular/core';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ShowDetailDialogComponent } from "@app/modules/absence-day/show-detail-dialog/show-detail-dialog.component";
import { SubmitAbsenseDayComponent } from '@app/modules/absence-day/submit-absense-day/submit-absense-day.component';
import { AbsenceDayService } from '@app/service/api/absence-day.service';
import { DayOffService } from '@app/service/api/day-off.service';
import { AbsenceRequestDto } from '@app/service/api/model/absence-day-dto';
import { WorkingTimeDto } from '@app/service/api/model/working-time-dto';
import { MatDialog } from '@node_modules/@angular/material';
import { MonthViewDay } from '@node_modules/calendar-utils';
import { AppComponentBase } from '@shared/app-component-base';
import { CalendarEvent, CalendarMonthViewBeforeRenderEvent, CalendarView } from 'angular-calendar';
import { Session } from 'inspector';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Subject } from 'rxjs';
import { dayOffDTO } from '../off-day/off-day.component';
import { CustomAbsenceTimeDialogComponent } from './custom-absence-time-dialog/custom-absence-time-dialog.component';
import { DetailRequestComponent } from './detail-request/detail-request.component';
import { TardinessLeaveEarlyDialogComponent } from './tardiness-leave-early-dialog/tardiness-leave-early-dialog.component';

@Component({
  selector: 'app-absence-day',
  templateUrl: './absence-day.component.html',
  styleUrls: ['./absence-day.component.css'],
  providers: [DatePipe]
})
export class AbsenceDayComponent extends AppComponentBase implements OnInit {
  SEND_REQUEST = PERMISSIONS_CONSTANT.SendMyAbsenceDay;

  isLoading: boolean;
  view: CalendarView;
  viewDate: Date;
  calendarView;
  title;
  year;
  years;
  month;
  months;
  day;
  branchId;
  refresh: Subject<any> = new Subject();
  selectedDays: Map<string, any>;//map date to DateType
  mapChanegRequest = new Map<string, any>();
  absenceReqs: AbsenceRequestDto[];
  listDayShow: AbsenceRequestDto[];
  monthViewBody: MonthViewDay[];
  events: CalendarEvent[] = [];
  dayOffs: dayOffDTO[] = [];
  activeDayIsOpen: boolean = false;
  workingTime: WorkingTimeDto = new WorkingTimeDto;
  dayAbsentTypeList = Object.keys(this.APP_CONSTANT.DayAbsenceType)
  dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType)
  absentDayType = -1
  dayType = -1

  fullDay = this.APP_CONSTANT.AbsenceType.FullDay;
  morning = this.APP_CONSTANT.AbsenceType.Morning;
  afternoon = this.APP_CONSTANT.AbsenceType.Afternoon;
  custom = this.APP_CONSTANT.AbsenceType.Custom;
  off = this.APP_CONSTANT.DayAbsenceType.Off;
  remote = this.APP_CONSTANT.DayAbsenceType.Remote;

  tardiness = this.APP_CONSTANT.OnDayType.BeginOfDay;
  leaveEarly = this.APP_CONSTANT.OnDayType.EndOfDay;

 

  constructor(injector: Injector,
    private dayOffService: DayOffService,
    private datePipe: DatePipe,
    private absenceDayService: AbsenceDayService,
    private diaLog: MatDialog) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.view = CalendarView.Month;
    this.viewDate = new Date();
    this.calendarView = CalendarView;
    this.absenceReqs = [] as AbsenceRequestDto[];
    this.listDayShow = [] as AbsenceRequestDto[];
    this.selectedDays = new Map<string, any>();
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;
    this.initMapChangeRequest();
  }

  ngOnInit() {
    this.updateDay();
    this.refreshData();
    this.workingTime.morningWorking = this.appSession.user.morningWorking;
    this.workingTime.morningStartAt = this.appSession.user.morningStartAt;
    this.workingTime.morningEndAt = this.appSession.user.morningEndAt;
    this.workingTime.afternoonWorking = this.appSession.user.afternoonWorking;
    this.workingTime.afternoonStartAt = this.appSession.user.afternoonStartAt;
    this.workingTime.afternoonEndAt = this.appSession.user.afternoonEndAt;
  }

  updateDay(): void {
    this.day = this.viewDate.getDate();
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }

  selectionChange(resetDayType: boolean) {
    if (resetDayType) this.dayType = -1;
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.updateDay();
    this.refreshData();
  }

  refreshData() {
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
          start: moment(day.dayOff, 'YYYY-MM-DD').toDate(),
          end: moment(day.dayOff, 'YYYY-MM-DD').toDate(),
          title: day.name,
          id: day.id,
          color: { primary: day.coefficient.toString(), secondary: "" }
        } as CalendarEvent)
      });
      this.refresh.next();
      let sDate = moment(new Date(this.year, this.month - 1, 1)).format("YYYY-MM-DD");
      let tDate = moment(new Date(this.year, this.month + 2, 0)).format("YYYY-MM-DD")
      this.absenceDayService.getAllAbsenceReqs(sDate, tDate, this.absentDayType, this.dayType).subscribe(resp => {
        this.absenceReqs = resp.result as AbsenceRequestDto[];
        this.monthViewBody.forEach((d: any) => {
          if (moment().isAfter(d.date) || d.date.getDay() === 0) {
            d['isOut'] = true;
          }

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
          d['countEventInDay'] = d.events.length;
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

  dayClicked(event: any): void {
    let selectedDate = moment(event.day.date).format("YYYY-MM-DD")
    this.removeRejectedRequests(event);
    const sentItems = event.day.events.filter(s => s.status != this.APP_CONSTANT.AbsenceStatus.New);
    let currentRequest = event.day.events.find(s => s.status == this.APP_CONSTANT.AbsenceStatus.New);
    if (currentRequest == null) {
      currentRequest = this.getFirstItem(sentItems);
      if (!currentRequest) {
        return;
      }

      event.day.events.push(currentRequest);
      event.day["isNew"] = true;
    } else {
      this.changeRequestItem(currentRequest, sentItems);
    }

    if (currentRequest.absenceTime > 0) {
      this.showPopupDiMuonVeSom(currentRequest, selectedDate);
    }
    this.selectedDays.set(selectedDate, currentRequest.dateType);
    if(currentRequest.dateType == -1 || 
      (currentRequest.cssClass == 'custom-request-time' && currentRequest.hour == "")){
      //empty
      this.selectedDays.delete(selectedDate);
    }
  }

  private removeRejectedRequests(event: any) {
    event.day.events = event.day.events.filter(s => s.status != this.APP_CONSTANT.AbsenceStatus.Rejected);
  }

  private initMapChangeRequest() {
    this.mapChanegRequest.set('empty', [this.FULLDAY, this.MORMING, this.AFTERNOON, this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('off-full', null);
    this.mapChanegRequest.set('off-morning', [this.AFTERNOON, this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('off-afternoon', [this.MORMING, this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('remote-full', [this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('remote-morning', [this.AFTERNOON, this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('remote-afternoon', [this.MORMING, this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('di-muon', [this.FULLDAY, this.MORMING, this.AFTERNOON, this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('ve-som', [this.FULLDAY, this.MORMING, this.AFTERNOON, this.EVENT_DI_MUON_ONLY, this.EMPTY]);

    this.mapChanegRequest.set('off-morning__remote-afternoon', [this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('off-morning__di-muon', [this.AFTERNOON, this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('off-morning__ve-som', [this.AFTERNOON, this.EVENT_DI_MUON_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('off-afternoon__remote-morning', [this.EVENT_DI_MUON_VA_VE_SOM, this.EMPTY]);
    this.mapChanegRequest.set('off-afternoon__di-muon', [this.MORMING, this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('off-afternoon__ve-som', [this.MORMING, this.EVENT_DI_MUON_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-full__di-muon', [this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-full__ve-som', [this.EVENT_DI_MUON_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-morning__di-muon', [this.AFTERNOON, this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-morning__ve-som', [this.AFTERNOON, this.EVENT_DI_MUON_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-afternoon__di-muon', [this.MORMING, this.EVENT_VE_SOM_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('remote-afternoon__ve-som', [this.MORMING, this.EVENT_DI_MUON_ONLY, this.EMPTY]);
    this.mapChanegRequest.set('di-muon__ve-som', [this.FULLDAY, this.MORMING, this.AFTERNOON, this.EMPTY]);
  }

  getKey(sentItems: any[]) {
    if (!sentItems || sentItems.length == 0) {
      return 'empty'
    }

    if (sentItems.length == 1) {

      let dataSendItem = sentItems[0];
      let sendItem = {
        type: dataSendItem['type'],//0 - off, 1-remote, 2 - onsite,0 - di muon ve som
        dateType: dataSendItem['dateType'],//1 - fullday,2- morning, 3- afternoon, 4-custom
        absenceTime: dataSendItem['absenceTime'],//1 - dimuon, 3 - ve muon
      }

      if (_.isEqual(sendItem, this.OFF_FULLDAY)) {
        return 'off-full'
      }
      if (_.isEqual(sendItem, this.OFF_MORMING)) {
        return 'off-morning'
      }
      if (_.isEqual(sendItem, this.OFF_AFTERNOON)) {
        return 'off-afternoon'
      }
      if (_.isEqual(sendItem, this.REMOTE_FULLDAY)) {
        return 'remote-full'
      }
      if (_.isEqual(sendItem, this.REMOTE_MORMING)) {
        return 'remote-morning'
      }
      if (_.isEqual(sendItem, this.REMOTE_AFTERNOON)) {
        return 'remote-afternoon'
      }
      if (_.isEqual(sendItem, this.DI_MUON)) {
        return 'di-muon'
      }
      if (_.isEqual(sendItem, this.VE_SOM)) {
        return 've-som'
      }
    }

    if (sentItems.length >= 2) {

      let dataSendItem1 = sentItems[0];
      let sendItem1 = {
        type: dataSendItem1['type'],
        dateType: dataSendItem1['dateType'],
        absenceTime: dataSendItem1['absenceTime'],
      }

      let dataSendItem2 = sentItems[1];
      let sendItem2 = {
        type: dataSendItem2['type'],
        dateType: dataSendItem2['dateType'],
        absenceTime: dataSendItem2['absenceTime'],
      }

      if (
        (_.isEqual(sendItem1, this.OFF_MORMING) && _.isEqual(sendItem2, this.REMOTE_AFTERNOON)) ||
        (_.isEqual(sendItem2, this.OFF_MORMING) && _.isEqual(sendItem1, this.REMOTE_AFTERNOON))
      ) {
        return 'off-morning__remote-afternoon'
      }

      if (
        (_.isEqual(sendItem1, this.OFF_MORMING) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.OFF_MORMING) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'off-morning__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.OFF_MORMING) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.OFF_MORMING) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'off-morning__ve-som'
      }

      if (
        (_.isEqual(sendItem1, this.OFF_AFTERNOON) && _.isEqual(sendItem2, this.REMOTE_MORMING)) ||
        (_.isEqual(sendItem2, this.OFF_AFTERNOON) && _.isEqual(sendItem1, this.REMOTE_MORMING))
      ) {
        return 'off-afternoon__remote-morning'
      }

      if (
        (_.isEqual(sendItem1, this.OFF_AFTERNOON) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.OFF_AFTERNOON) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'off-afternoon__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.OFF_AFTERNOON) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.OFF_AFTERNOON) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'off-afternoon__ve-som'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_FULLDAY) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.REMOTE_FULLDAY) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'remote-full__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_FULLDAY) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.REMOTE_FULLDAY) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'remote-full__ve-som'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_MORMING) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.REMOTE_MORMING) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'remote-morning__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_MORMING) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.REMOTE_MORMING) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'remote-morning__ve-som'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_MORMING) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.REMOTE_MORMING) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'remote-morning__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_MORMING) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.REMOTE_MORMING) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'remote-morning__ve-som'
      }


      if (
        (_.isEqual(sendItem1, this.REMOTE_AFTERNOON) && _.isEqual(sendItem2, this.DI_MUON)) ||
        (_.isEqual(sendItem2, this.REMOTE_AFTERNOON) && _.isEqual(sendItem1, this.DI_MUON))
      ) {
        return 'remote-afternoon__di-muon'
      }

      if (
        (_.isEqual(sendItem1, this.REMOTE_AFTERNOON) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.REMOTE_AFTERNOON) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'remote-afternoon__ve-som'
      }

      if (
        (_.isEqual(sendItem1, this.DI_MUON) && _.isEqual(sendItem2, this.VE_SOM)) ||
        (_.isEqual(sendItem2, this.DI_MUON) && _.isEqual(sendItem1, this.VE_SOM))
      ) {
        return 'di-muon__ve-som'
      }
    }
  }

  private changeRequestItem(currentRequest: any, sentItems: any) {
    const key = this.getKey(sentItems);

    let allowRequestItems = this.mapChanegRequest.get(key);

    if (currentRequest.clickIndex == null) {
      currentRequest.clickIndex = 0
    } else {
      currentRequest.clickIndex++;
      currentRequest.clickIndex = currentRequest.clickIndex % allowRequestItems.length;
    }

    const nextRequest = allowRequestItems[currentRequest.clickIndex];
    currentRequest.cssClass = nextRequest.cssClass;
    currentRequest.type = nextRequest.type;
    currentRequest.dateType = nextRequest.dateType;
    currentRequest.absenceTime = nextRequest.absenceTime;
    currentRequest.cssHour = nextRequest.cssHour;
    currentRequest.hour = nextRequest.hour;
  }

  private getFirstItem(sentItems: any[]): any {
    const key = this.getKey(sentItems);

    let allowRequestItems = this.mapChanegRequest.get(key);
    if (allowRequestItems) {
      const currentRequest = { ...allowRequestItems[0], clickIndex: 0 };

      return currentRequest;
    }
    return null;

  }


  showPopupDiMuonVeSom(currentRequest: any, selectedDate: string) {
    let absenceTime = currentRequest.absenceTime;
    let dateType = currentRequest.dateType;

    const dialogRef = this.diaLog.open(TardinessLeaveEarlyDialogComponent, {
      data: {
        data: this.workingTime,
        absenceTime: absenceTime,
        dateType: dateType,
        selectedDate : selectedDate,
      },
      disableClose: true,
      width: "500px"
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (res) this.refreshData();
    })
  }
  showDialogCustomTime(data: any) {
    let seletedDay = moment(data.day.date).format("YYYY-MM-DD")
    const dialogRef = this.diaLog.open(CustomAbsenceTimeDialogComponent, {
      data: this.workingTime,
      disableClose: true,
      width: "300px"
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        if (res.hour.split('').length != 3) {
          if (res.hour.substring(1, 2) == '.') {
            res.hour = res.hour.substring(1, 0)
          }
        }
        data.day.isTime = true
        data.day.hour = res.hour
        data.day.absenceTime = res.absenceTime
        this.selectedDays.set(seletedDay, { type: 4, hour: res.hour, absenceTime: res.absenceTime });
      } else {
        this.selectedDays.set(seletedDay, 4)
      }
    })
  }
  getClassByStatus(status: number) {
    if (status == 1)
      return "day-off-state-pending";

    if (status == 2)
      return "day-off-state-approved";

    if (status == 3)
      return "day-off-state-reject";
  }
  getData(renderEvent: CalendarMonthViewBeforeRenderEvent): void {
    this.monthViewBody = renderEvent.body;
    this.monthViewBody.forEach(day => {
      if (this.events.findIndex(data => data.start.getDate() == day.date.getDate()) >= 0 && day.inMonth) {
        day.cssClass = 'back-red';
      }
    });
  }


  requestOff() {
    this.submitButtonClick(this.APP_CONSTANT.DayAbsenceType.Off, 0);
  }

  requestRemote() {
    this.submitButtonClick(this.APP_CONSTANT.DayAbsenceType.Remote, 0);
  }

  requestOnsite() {
    this.submitButtonClick(this.APP_CONSTANT.DayAbsenceType.Onsite, 0);
  }

  requestDiMuonVeSom() {
    this.submitButtonClick(this.APP_CONSTANT.DayAbsenceType.Off, 1);
  }

  private submitButtonClick(type: number, absenceTime: number) {
    if (this.selectedDays.size <= 0) {
      abp.message.error('Bạn chưa chọn ngày nào')
    } else {
      const data = {
        selectedDays: this.selectedDays,
        type: type,
        absenceTime: absenceTime
      }
      const dialogRef = this.diaLog.open(SubmitAbsenseDayComponent, { disableClose: true, data: data });
      dialogRef.afterClosed().subscribe((res) => {
        if (res) this.refreshData();
      });
    }
  }
  closeOpenMonthViewDay() {
    this.activeDayIsOpen = false;
    this.ngOnInit();
  }

  handleViewDetail(dateAt: string) {
    const dialogRef = this.diaLog.open(DetailRequestComponent, {
      disableClose: true,
      width: "1240px",
      data: dateAt
    });
    dialogRef.afterClosed().subscribe((res) => {
      if (res == 1) {
        this.refreshData();
      }

    })
  }

  EMPTY = {
    cssClass: "",
    dateType: -1,
    absenceTime: 0,
    cssHour: "",
    hour: '',
    status: 0
  }

  FULLDAY = {
    cssClass: "all-day-request",
    dateType: 1,
    absenceTime: 0,
    cssHour: "",
    hour: '',
    status: 0
  }

  MORMING = {
    cssClass: "morning-request",
    dateType: 2,
    absenceTime: 0,
    cssHour: "",
    hour: '',
    status: 0
  }

  AFTERNOON = {
    cssClass: "afternoon-request",
    dateType: 3,
    absenceTime: 0,
    cssHour: "",
    hour: '',
    status: 0
  }

  EVENT_DI_MUON_VA_VE_SOM = {
    cssClass: "custom-request-time",
    type: 0,
    dateType: 4,
    absenceTime: 10,//di muon hoac ve som
    cssHour: "hour",
    hour: '',
    status: 0
  }

  EVENT_VE_SOM_ONLY = {
    cssClass: "custom-request-time",
    type: 0,
    dateType: 4,
    absenceTime: 3,
    cssHour: "hour",
    hour: '',
    status: 0
  }

  EVENT_DI_MUON_ONLY = {
    cssClass: "custom-request-time",
    type: 0,
    dateType: 4,
    absenceTime: 1,
    cssHour: "hour",
    hour: '',
    status: 0
  }


  ////


  OFF_FULLDAY = {
    type: 0,
    dateType: 1,
    absenceTime: null
  }

  OFF_MORMING = {
    type: 0,
    dateType: 2,
    absenceTime: null
  }

  OFF_AFTERNOON = {
    type: 0,
    dateType: 3,
    absenceTime: null
  }

  DI_MUON = {
    type: 0,
    dateType: 4,
    absenceTime: 1
  }

  VE_SOM = {
    type: 0,
    dateType: 4,
    absenceTime: 3

  }


  REMOTE_FULLDAY = {
    type: 2,
    dateType: 1,
    absenceTime: null
  }

  REMOTE_MORMING = {
    type: 2,
    dateType: 2,
    absenceTime: null
  }

  REMOTE_AFTERNOON = {
    type: 2,
    dateType: 3,
    absenceTime: null
  }
}

export class TimeStartWork {
  hour: number;
  minute: number;
}

export class GetRequestOfUserDto {
  id: number;
  userId: number;
  dateAt: string;
  dateType: number;
  hour: number;
  dayOffName: string;
  status: number;
  reason: string;
  leavedayType: number;
}