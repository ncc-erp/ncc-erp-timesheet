import { Component, Injector, Input, OnInit, Output,EventEmitter } from '@angular/core';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { AbsenceDetaiInDay, GetNormalWorkingHourByUserLoginDto, WorkingHourDto } from '@app/service/api/model/report-timesheet-Dto';
import { ReportService } from '@app/service/api/report.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-mytimesheet-normal-working',
  templateUrl: './mytimesheet-normal-working.component.html',
  styleUrls: ['./mytimesheet-normal-working.component.scss']
})
export class MytimesheetNormalWorkingComponent extends AppComponentBase implements OnInit {
  @Input() isRefresh: boolean;
  @Output() refreshEvent: EventEmitter<boolean> = new EventEmitter<boolean>();


  month: number;
  months: number[] = [];
  year: number;;
  years: number[] = [];
  listWorkingHour: WorkingHourDto[] = [];
  myTimesheetStatus = APP_CONSTANT.MyTimesheetStatusFilter['Approved'];
  myTimesheetStatusList = Object.keys(this.APP_CONSTANT.MyTimesheetStatusFilter)
  OFF = this.APP_CONSTANT.DayAbsenceType.Off;
  REMOTE = this.APP_CONSTANT.DayAbsenceType.Remote;
  ONSITE = this.APP_CONSTANT.DayAbsenceType.Onsite;
  FULLDAY = this.APP_CONSTANT.AbsenceType.FullDay;
  MORMING = this.APP_CONSTANT.AbsenceType.Morning;
  AFTERNOON = this.APP_CONSTANT.AbsenceType.Afternoon;
  CUSTOM = this.APP_CONSTANT.AbsenceType.Custom;
  DI_MUON = this.APP_CONSTANT.OnDayType.BeginOfDay;
  VE_SOM = this.APP_CONSTANT.OnDayType.EndOfDay;
  isTableLoading: boolean = true;
  normalWorkingHourByUserLogin = {} as GetNormalWorkingHourByUserLoginDto;;
  constructor(
    injector: Injector,
    private reportService: ReportService,
  ) {
    super(injector);
    let today = new Date();
    this.month = today.getMonth();
    this.year = today.getFullYear();
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;

  }
  ngOnChanges() {
    if(this.isRefresh){
      this.getData();
    }
  }
  ngOnInit() {
    this.getData();
  }

  sendRefresh() {
    this.refreshEvent.emit(false);
  }

  getData() {
    this.isTableLoading = true;
    this.reportService.getNormalWorkingHourByUserLogin(this.year, this.month + 1, this.myTimesheetStatus).subscribe(res => {
      this.normalWorkingHourByUserLogin = res.result;
      this.isTableLoading = false;
      this.refreshEvent.emit(false);
    },()=>{
      this.isTableLoading = false;
    });
  }
  getCssClass(detail: WorkingHourDto) {
    if (detail.isOffDaySetting) {
      return 'dayOff'
    }
    if (!detail.isOnsite 
        && detail.workingHour + detail.offHour != 8 
        && (detail.workingHour != null || detail.offHour != null) 
        && !detail.isOpenTalk) {
      return 'dayWarning';
    }
    if (detail.isLock) {
      return 'dayLocked'
    }
    return 'defaultDay';
  }

  onDateChange() {
    this.getData();
  }
  
  getCssClass2(absenceDetaiInDay: AbsenceDetaiInDay) {
    if (!absenceDetaiInDay) return null;
    let resultClass = '';
    let cssClass = ' day-chip-';

    switch (absenceDetaiInDay.type) {
      case this.ONSITE:
        resultClass = 'onsite' + cssClass;
        break;
      case this.REMOTE:
        resultClass = 'remote' + cssClass;
        break;
      default:
        resultClass = 'absence' + cssClass;
    }
    if (absenceDetaiInDay.absenceType === this.FULLDAY) {
      resultClass += 'full-day';
    } else if (absenceDetaiInDay.absenceType === this.MORMING) {
      resultClass += 'morning';
    } else if (absenceDetaiInDay.absenceType === this.AFTERNOON) {
      resultClass += 'afternoon';
    } else if (absenceDetaiInDay.absenceType === this.CUSTOM) {
      resultClass += 'custom';
    }
    return resultClass;
  }

  showContentCell(absenceDetaiInDay: AbsenceDetaiInDay) {
    if (!absenceDetaiInDay) return null;
    let result = 'O-';
    switch (absenceDetaiInDay.type) {
      case this.ONSITE:
        result = 'OS-';
        break;
      case this.REMOTE:
        result = 'R-';
        break;
      default:
        result = 'O-';
    }

    if (absenceDetaiInDay.absenceType === this.FULLDAY) {
      return result += 'FD';
    }
    if (absenceDetaiInDay.absenceType === this.MORMING) {
      return result += 'M';
    }
    if (absenceDetaiInDay.absenceType === this.AFTERNOON) {
      return result += 'A';
    }
    if (absenceDetaiInDay.absenceType === this.CUSTOM &&
      absenceDetaiInDay.absenceTime === this.DI_MUON) {
      return "ĐM " + absenceDetaiInDay.hour;
    }
    return "VS " + absenceDetaiInDay.hour;
  }
}