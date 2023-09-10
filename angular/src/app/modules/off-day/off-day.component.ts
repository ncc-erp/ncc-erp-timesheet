import { moment } from 'ngx-bootstrap/chronos/test/chain';
import { Component, ViewChild, TemplateRef, OnInit, ChangeDetectionStrategy, Injector, ChangeDetectorRef, OnChanges, AfterViewChecked } from '@angular/core';
import { isSameDay, isSameMonth } from 'date-fns';
import { Subject } from 'rxjs';
import { CalendarView, CalendarEvent, CalendarMonthViewBeforeRenderEvent } from 'angular-calendar';
import { DayOffService } from '@app/service/api/day-off.service';
import { AppComponentBase } from '@shared/app-component-base';
import { MatDialog, MatMenuTrigger } from '@angular/material';
import { CreateEditOffDayComponent } from './create-edit-off-day/create-edit-off-day.component';
import * as _ from 'lodash';
import { DatePipe } from '@angular/common';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Component({
  selector: 'app-off-day',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './off-day.component.html',
  styleUrls: ['./off-day.component.css'],
  providers: [DatePipe]
})
export class OffDayComponent extends AppComponentBase implements OnInit {
  ADD_DAY_OFF = PERMISSIONS_CONSTANT.AddDayOff;
  EDIT_DAY_OFF = PERMISSIONS_CONSTANT.EditDayOff;
  DELETE_DAY_OFF = PERMISSIONS_CONSTANT.DeleteDayOff;

  @ViewChild('modalContent') modalContent: TemplateRef<any>;
  @ViewChild(MatMenuTrigger)

  contextMenuPosition = { x: '0px', y: '0px' };

  view: CalendarView = CalendarView.Month;

  dayOffs: dayOffDTO[] = [];

  CalendarView = CalendarView;

  viewDate: Date = new Date();

  isEdit = true;

  isClicked = false;
  stopLop: boolean = true;
  day;
  month;
  year;
  title;
  listYear = APP_CONSTANT.ListYear;
  listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  branchId;
  modalData: {
    action: string;
    event: CalendarEvent;
  };

  refresh: Subject<any> = new Subject();

  events: CalendarEvent[] = [];

  activeDayIsOpen: boolean = false;


  constructor(
    private dayOffService: DayOffService,
    injector: Injector,
    private diaLog: MatDialog,
    private cdr: ChangeDetectorRef,
    private datePipe: DatePipe
  ) {
    super(injector);
    this.branchId = this.appSession.user.branchId;
    this.updateDay();
  }

  onContextMenu(event: MouseEvent, temp, day) {
    if (this.permission.isGranted(this.DELETE_DAY_OFF) || this.permission.isGranted(this.EDIT_DAY_OFF)) {
      if (day.badgeTotal == 0) {
        return;
      }
      event.preventDefault();
      this.contextMenuPosition.x = event.clientX + 'px';
      this.contextMenuPosition.y = event.clientY + 'px';
      temp.openMenu();
    }
  }

  ngOnInit(): void {
    this.refreshData();
  }

  // setDayOff(): void {
  //   var temp = $('[aria-label]').mouseenter();
  //   for (let x of temp.toArray()) {
  //     if (x.attributes[1].value.length > 30) {
  //       x.classList.add("back-red");
  //     }
  //     else {
  //       x.classList.remove("back-red");
  //     }
  //   }
  // }


  // ngAfterViewChecked(): void {
  //   this.setDayOff();
  //   var temp = $('[aria-label~="Sunday"]');
  //   for (let x of temp.toArray()) {
  //     if (new Date(x.attributes[1].value).getMonth() == new Date(this.viewDate).getMonth()) {
  //       x.classList.add("back-red");
  //     }
  //   }
  // }

  ngAfterViewChecked(): void {
    $(".mat-form-field-wrapper").css("padding", "0px");
    $(".mat-form-field-wrapper").css("margin", "0px");
  }

  edit(): void {
    this.isEdit = !this.isEdit;
  }

  updateDay(): void {
    this.day = this.viewDate.getDate();
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }

  selectionChange(): void {
    let date = new Date(this.year, this.month, this.day);
    this.viewDate = moment(date, 'YYYY-MM-DD').toDate()
    this.refreshData();
  }

  // updateListYear(): void {
  //   this.listYear = [];
  //   for (let i = this.viewDate.getFullYear() - 5; i <= this.viewDate.getFullYear() + 5; i++) {
  //     this.listYear.push(i);
  //   }
  // }

  delete(date, temp): void {
    abp.message.confirm(this.l("Delete DayOff In " + this.datePipe.transform(date, "MM-dd-yyyy") + " ?"),
      (result: boolean) => {
        if (result) {
          this.dayOffService.deleteDayOff(temp.id).subscribe(res => {
            if (res) {
              this.notify.success(this.l("Delete Successfully!"));
            }
            this.ngOnInit();
          })
        }
      });
  }

  refreshData(): void {
    this.dayOffs = [];
    this.events = [];
    this.updateDay();
    this.title = this.datePipe.transform(this.viewDate, "MM-yyyy");
    // this.updateListYear();
    this.dayOffService.getAll(this.viewDate.getMonth() + 1, this.viewDate.getFullYear(), this.branchId).subscribe(data => {
      this.dayOffs = data.result;
      this.dayOffs.forEach(day => {
        this.events.push({ start: moment(day.dayOff, 'YYYY-MM-DD').toDate(), end: moment(day.dayOff, 'YYYY-MM-DD').toDate(), title: day.name, id: day.id, color: { primary: day.coefficient.toString(), secondary: "" } } as CalendarEvent)
      });
      this.refresh.next();
    });
  }

  getData(renderEvent: CalendarMonthViewBeforeRenderEvent): void {
    renderEvent.body.forEach(day => {
      if (this.events.findIndex(data => data.start.getDate() == day.date.getDate()) >= 0 && day.inMonth) {
        day.cssClass = 'back-red';
      }
    });
  }

  isSunday(date): boolean {
    return moment(date.date, 'YYYY-MM-DD').toDate().getDay() == 0 && date.inMonth;
  }

  dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }): void {
    if (isSameMonth(date, this.viewDate)) {
      // if (date.getDay() == 0) {
      //   return;
      // }
      var temp = events.find(data => data.start.getDay() == date.getDay());
      if (!temp) {
        this.createOrEditDayOff(date, temp);
      }
      this.viewDate = date;
    }
  }

  createOrEditDayOff(date: Date, event?: CalendarEvent): void {
    if (this.permission.isGranted(this.ADD_DAY_OFF) || this.permission.isGranted(this.EDIT_DAY_OFF)) {
      var temp = event == null ? {} as CalendarEvent : event;
      var diaLogRef = this.diaLog.open(CreateEditOffDayComponent, {
        disableClose: true,
        width: "500px",
        data: { item: temp, date: date }
      });
      diaLogRef.afterClosed().subscribe(() => {
        this.ngOnInit();
      });
    }

  }

  setView(view: CalendarView) {
    this.view = view;
  }

  closeOpenMonthViewDay() {
    this.activeDayIsOpen = false;
    this.ngOnInit();
  }
}

export class dayOffDTO {
  dayOff: string;
  name: string;
  coefficient: number;
  id: number;
  branch: number;
}
