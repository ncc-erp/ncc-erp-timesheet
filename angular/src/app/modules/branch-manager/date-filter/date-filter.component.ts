import { Component, EventEmitter, Injector, OnInit, Output } from '@angular/core';
import { MatDatepicker } from '@angular/material';
import * as moment from 'moment';
import { AppComponentBase } from 'shared/app-component-base';

export interface DateInfo {
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-date-filter',
  templateUrl: './date-filter.component.html',
  styleUrls: ['./date-filter.component.css']
})
export class DateFilterComponent extends AppComponentBase implements OnInit {
  @Output() dateSelected = new EventEmitter<DateInfo>();

  typeOfView = this.APP_CONSTANT.TypeViewBranchManager.Month;
  startView = "year";
  activeDay: any;
  startDate: string;
  endDate: string;
  displayDay: any;
  TimeType = Object.keys(this.APP_CONSTANT.TypeViewBranchManager);

  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit() {
    this.startDate = moment().startOf('M').format('YYYY-MM-DD');
    this.endDate = moment().endOf('M').format('YYYY-MM-DD');
    this.activeDay = this.startDate;
    this.displayDay = this.startDate + " - " + this.endDate
    this.dateSelected.emit({startDate:this.startDate,endDate:this.endDate});
  }

  customDate() {
    switch(this.typeOfView){
      case this.APP_CONSTANT.TypeViewBranchManager.Day:
        this.displayDay = moment(this.activeDay).format('YYYY-MM-DD');
        this.startDate = this.displayDay;
        this.endDate = this.displayDay;
        this.startView = "month";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Week:
        this.startDate = moment(this.activeDay).startOf('isoWeek').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('isoWeek').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "month";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Month:
        this.startDate = moment(this.activeDay).startOf('M').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('M').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "year";
        break;
      case this.APP_CONSTANT.TypeViewBranchManager.Year:
        this.startDate = moment(this.activeDay).startOf('y').format('YYYY-MM-DD');
        this.endDate = moment(this.activeDay).endOf('y').format('YYYY-MM-DD');
        this.activeDay = this.startDate;
        this.displayDay = this.startDate + " - " + this.endDate;
        this.startView = "multi-year";
        break;
      default:
        this.startDate = "";
        this.endDate = "";
    }
    this.dateSelected.emit({startDate:this.startDate,endDate:this.endDate});
  }

  FilterByTime() {
    this.activeDay=moment().format('YYYY-MM-DD');
    this.customDate();
  }
  setMonth(normalizedMonthAndYear: moment.Moment, datepicker: MatDatepicker<moment.Moment>) {
    if(this.typeOfView == this.APP_CONSTANT.TypeViewBranchManager.Month){
      this.activeDay=moment(normalizedMonthAndYear).format('YYYY-MM-DD');
      datepicker.close();
      this.customDate();
    }
  }
  setYear(normalizedMonthAndYear: moment.Moment, datepicker: MatDatepicker<moment.Moment>) {
    if(this.typeOfView == this.APP_CONSTANT.TypeViewBranchManager.Year){
      this.activeDay=moment(normalizedMonthAndYear).format('YYYY-MM-DD');
      datepicker.close();
      this.customDate();
    }
  }
}