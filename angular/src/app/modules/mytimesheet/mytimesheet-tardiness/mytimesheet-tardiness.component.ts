import { TimekeepingService } from './../../../service/api/timekeeping.service';
import { DatePipe } from '@angular/common';
import { TimekeepingDto, UpdateTimekeepingDto } from './../../../service/api/model/report-timesheet-Dto';
import { userDTO } from './../../check-board/create-check-board/create-check-board.component';
import { CalendarView } from 'angular-calendar';
import { FormControl } from '@angular/forms';
import { PERMISSIONS_CONSTANT } from './../../../constant/permission.constant';
import { AppComponentBase } from 'shared/app-component-base';
import { Component, OnInit, Injector } from '@angular/core';

@Component({
  selector: 'app-mytimesheet-tardiness',
  templateUrl: './mytimesheet-tardiness.component.html',
  styleUrls: ['./mytimesheet-tardiness.component.css'],
  providers: [DatePipe]

})
export class MytimesheetTardinessComponent extends AppComponentBase implements OnInit {

  EDIT_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.EditTardinessLeaveEarly;
  VIEW_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewTardinessLeaveEarly;
  Timekeeping_UserNote = PERMISSIONS_CONSTANT.Timekeeping_UserNote;
  // listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  // listYear = APP_CONSTANT.ListYear;
  month;
  months;
  year;
  years;
  view: CalendarView;
  viewDate: Date;
  calendarView;
  userControl: FormControl;
  listTimekeeping: TimekeepingDto[] = [];
  userId: number;
  userName: string;
  isTableLoading: boolean = false;
  selectedDay: number = -1;
  dayList: any = []
  public countLate: number = 0;

  constructor(
    private timekeepingService: TimekeepingService,
    injector: Injector,
  ) {
    super(injector);
    this.view = CalendarView.Month;
    this.viewDate = new Date();
    this.viewDate.setMonth(new Date().getMonth());
    this.calendarView = CalendarView;
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;
    this.userId = this.appSession.userId
    this.userControl = new FormControl(this.userId);
    this.updateDay();
    this.userName = this.appSession.user.surname + ' ' + this.appSession.user.name;
  }

  ngOnInit() {
    this.getData();
  }

  getData() {
    this.isTableLoading = true;
    this.timekeepingService.getMyDetails(this.year, this.month + 1).subscribe(res => {
      this.listTimekeeping = res.result;
      this.isTableLoading = false;
      this.countLate = this.countPunish(res.result)
    });
  }
  countPunish(data) {
    return data.filter((item) => {
      return item.statusPunish != 0 || item.status == 1
    }).length;
  }

  updateDay(): void {
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
    this.getDayByMonthAndYear(this.month, this.year)
  }
  getDayByMonthAndYear(month: number, year: number) {
    let numOfday: number = new Date(year, month + 1, 0).getDate();
    this.dayList = []
    for (let i = 1; i <= numOfday; i++) {
      this.dayList.push(i)
    }
  }

  formatDate(date: string) {
    return new Date(date).toLocaleDateString("vi");
  }

  getCss1(value) {
    value = Number.parseInt(value);
    if (value > 15) {
      return "red";
    }
    return "green";
  }

  onDateChange() {
    this.viewDate = new Date(this.year, this.month, this.selectedDay);
    this.getDayByMonthAndYear(this.month, this.year)
    this.getData();
    this.countLate = this.countPunish(this.listTimekeeping)
  }

  onSave(item: TimekeepingDto) {
    let requestBody = {
      userNote: item.userNote,
      id: item.strTimekeepingId
    }
    this.timekeepingService.addComplain(requestBody).subscribe(res => {
      if (res.success) {
        abp.notify.success("add complain successfully");
        this.getData();
        item.isComplain = false;
      }
    })
  }

  onCancel(item: TimekeepingDto) {
    item.isComplain = false;
    this.getData();
  }
  trackerTimeFormat(time) {
    if (time == ""|| time == null) {
      return "";
    }
    if (time == 0) {
      return 0;
    }
    const [hours, minutes] = time.split(":").slice(0, 2);
    const formattedHours = hours.length === 1 ? `0${hours}` : hours;
    const formattedMinutes = minutes.length === 1 ? `0${minutes}` : minutes;
    return `${formattedHours}:${formattedMinutes}`;
  }

  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]
}
