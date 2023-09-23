import { Component, OnInit, Injector, Input, Output, EventEmitter, Optional, Inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import * as moment from 'moment';
import { MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { PopupComponent } from '@shared/date-selector/popup/popup.component';
import { AppComponentBase } from '@shared/app-component-base';
import { DateSelectorEnum } from '@shared/AppEnums';
import { DATE_TIME_OPTIONS } from '@shared/AppConsts';

@Component({
  selector: 'app-date-selector-new',
  templateUrl: './date-selector-new.component.html',
  styleUrls: ['./date-selector-new.component.css'],

})
export class DateSelectorNewComponent extends AppComponentBase implements OnInit {
  selectChanges: FormControl = new FormControl();
  customizeView: number = 0;
  isFistHalfYear: boolean = true;
  initOptionHalfYear: boolean = true;
  dateType: DateSelectorEnum;
  dateText: string;
  isBtnPrev: boolean;
  currentDate = moment();

  public readonly DateSelectorEnum = DateSelectorEnum;
  @Input('type') defaultDateType = DateSelectorEnum.YEAR;
  @Input() dateTimeOptions = DATE_TIME_OPTIONS;
  @Input() label: string;
  @Output() onDateSelectorChange: EventEmitter<DateTimeSelector> = new EventEmitter<DateTimeSelector>();

  distanceFromAndToDate: string;

  constructor(
    injector: Injector,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any,
    private _dialog: MatDialog,
  ) {
    super(injector)
  }

  ngOnInit() {
    this.selectChanges.setValue(this.defaultDateType);
    this.changeOptions(true);
  }

  changeOptions(reset?: boolean) {
    if (reset) {
      this.customizeView = 0;
    }
    const { unitOfTime, keyOfTime, typeDate } = this.getUnitOfTime(this.defaultDateType);
    this.dateType = typeDate;
    if (this.defaultDateType === DateSelectorEnum.HALF_YEAR) {
      this.selectOptionHalfYear();
      return;
    }
    if (this.defaultDateType == DateSelectorEnum.CUSTOM) {
      //this.showPopup();
      return;
    }
    const fromDate = moment().clone().startOf(unitOfTime).add(this.customizeView, keyOfTime);
    const toDate = moment(fromDate).clone().endOf(unitOfTime);
    this.dateText = this.getDisplayText(typeDate, fromDate, toDate);
    this.onDateSelectorChange.emit({ dateType: this.dateType, fromDate, toDate, dateText: this.dateText });
    return;
  }

  nextOrPre(title: any) {
    if (this.defaultDateType === DateSelectorEnum.CUSTOM) {
      return;
    }
    if (title === 'pre') {
      this.customizeView--;
      this.isBtnPrev = true;
    }
    if (title === 'next') {
      this.customizeView++;
      this.isBtnPrev = false;
    }
    this.changeOptions();
  }


  showPopup(): void {
    const popup = this._dialog.open(PopupComponent);
    popup.afterClosed().subscribe(res => {
      if (res === undefined) return;
      if (res.result) {
        this.dateText = this.getDisplayText(DateSelectorEnum.CUSTOM, res.data.fromDateCustomTime, res.data.toDateCustomTime) || 'No date select'
        this.onDateSelectorChange.emit({ dateType: this.dateType, fromDate: res.data.fromDateCustomTime, toDate: res.data.toDateCustomTime, dateText: this.dateText });
      }
    });
  }

  private selectOptionHalfYear() {
    let fromDate;
    let toDate;
    if (this.initOptionHalfYear) {
      fromDate = moment().startOf('year');;
      toDate = moment(fromDate).endOf('year').add(-6, 'months');
      if (moment().quarter() > 2) {
        fromDate = moment().startOf('year').add(6, 'months');;
        toDate = moment(fromDate).endOf('year');
        this.isFistHalfYear = false;
      }
      this.initOptionHalfYear = false;
    }
    else {
      if (this.isFistHalfYear) {
        if (this.isBtnPrev) {
          this.currentDate = this.currentDate.add(-1, 'years');
        }
        fromDate = this.currentDate.startOf('year').add(6, 'months');
        toDate = moment(fromDate).endOf('year');
        this.isFistHalfYear = false;
      }
      else {
        if (this.isBtnPrev === false) {
          this.currentDate = this.currentDate.add(1, 'years');
        }
        fromDate = this.currentDate.startOf('year');
        toDate = moment(fromDate).endOf('year').add(-6, 'months');
        this.isFistHalfYear = true;
      }
    }

    this.dateText = this.getDisplayText(DateSelectorEnum.HALF_YEAR, fromDate, toDate);
    this.onDateSelectorChange.emit({ dateType: this.dateType, fromDate, toDate, dateText: this.dateText });
  }

  private getUnitOfTime(type: DateSelectorEnum): any {
    switch (type) {
      case DateSelectorEnum.DAY: {
        return {
          unitOfTime: DateSelectorEnum.DAY.toLowerCase(),
          keyOfTime: DateSelectorEnum.DAY.toLowerCase(),
          typeDate: DateSelectorEnum.DAY
        }
      }

      case DateSelectorEnum.WEEK: {
        return {
          unitOfTime: 'isoWeek',
          keyOfTime: 'w',
          typeDate: DateSelectorEnum.WEEK
        }
      }
      case DateSelectorEnum.MONTH: {
        return {
          unitOfTime: DateSelectorEnum.MONTH.toLowerCase(),
          keyOfTime: DateSelectorEnum.MONTH.toLowerCase(),
          typeDate: DateSelectorEnum.MONTH
        }
      }
      case DateSelectorEnum.QUARTER: {
        return {
          unitOfTime: DateSelectorEnum.QUARTER.toLowerCase(),
          keyOfTime: DateSelectorEnum.QUARTER.toLowerCase(),
          typeDate: DateSelectorEnum.QUARTER
        }
      }
      case DateSelectorEnum.HALF_YEAR: {
        return {
          unitOfTime: DateSelectorEnum.HALF_YEAR.toLowerCase(),
          keyOfTime: DateSelectorEnum.HALF_YEAR.toLowerCase(),
          typeDate: DateSelectorEnum.HALF_YEAR
        }
      }
      case DateSelectorEnum.YEAR: {
        return {
          unitOfTime: DateSelectorEnum.YEAR.toLowerCase(),
          keyOfTime: DateSelectorEnum.YEAR.toLowerCase(),
          typeDate: DateSelectorEnum.YEAR
        }

      }
      case DateSelectorEnum.CUSTOM: {
        return {
          unitOfTime: DateSelectorEnum.CUSTOM.toLowerCase(),
          keyOfTime: DateSelectorEnum.CUSTOM.toLowerCase(),
          typeDate: DateSelectorEnum.CUSTOM
        }
      }
      default: {
        return {
          unitOfTime: DateSelectorEnum.YEAR.toLowerCase(),
          keyOfTime: DateSelectorEnum.YEAR.toLowerCase(),
          typeDate: DateSelectorEnum.YEAR
        }
      }
    }
  }

  private getDisplayText(typeDate: DateSelectorEnum, fromDate: moment.Moment = null, toDate: moment.Moment = null) {
    const spaceFormat = ' - ';
    if (!fromDate || !toDate) {
      return '';
    }
    const toDateFormat = toDate.format(DateFormat.DD_MM_YYYY);
    switch (typeDate) {
      case DateSelectorEnum.DAY: {
        return toDateFormat;
      }
      case DateSelectorEnum.WEEK: {
        if (fromDate.format(DateFormat.MM) == toDate.format(DateFormat.MM)) {
          return `${fromDate.format(DateFormat.DD)}${spaceFormat}${toDate.format(DateFormat.DD_MM_YYYY)}`
        }

        return `${fromDate.format(DateFormat.DD_MM)}${spaceFormat}${toDate.format(DateFormat.DD_MM_YYYY)}`
      }
      case DateSelectorEnum.MONTH: {
        return toDate.format(DateFormat.MM_YYYY);
      }
      case DateSelectorEnum.QUARTER: {
        return `${fromDate.format(DateFormat.DD_MM)}${spaceFormat}${toDateFormat}`;
      }
      case DateSelectorEnum.HALF_YEAR: {
        return `${fromDate.format(DateFormat.DD_MM)}${spaceFormat}${toDate.format(DateFormat.DD_MM_YYYY)}`;
      }
      case DateSelectorEnum.YEAR: {
        return toDate.format(DateFormat.YYYY);
      }
      case DateSelectorEnum.CUSTOM: {
        return `${fromDate.format(DateFormat.DD_MM_YYYY)}${spaceFormat}${toDate.format(DateFormat.DD_MM_YYYY)}`
      }
      default: {
        return `${fromDate.format(DateFormat.DD_MM_YYYY)}${spaceFormat}${toDate.format(DateFormat.DD_MM_YYYY)}`
      }
    }
  }
}

export interface DateTimeSelector {
  dateType: string,
  fromDate: moment.Moment,
  toDate: moment.Moment,
  dateText: string
}

export const DateFormat = {
  YYYY_MM_DD: 'YYYY/MM/DD',
  YYYY_MM_DD_H_MM_SS: 'YYYY/MM/DD h:mm:ss',
  YYYY_MM_DD_HH_MM_SS: 'YYYY/MM/DD H:mm:ss',
  DD_MM_YYYY: 'DD/MM/YYYY',
  DD_MM_YYYY_H_MM: 'DD/MM/YYYY H:mm',
  H_MM_SS: 'h:mm:ss',
  MM_YYYY: 'MM/YYYY',
  DD_MM: 'DD/MM',
  YYYY: 'YYYY',
  DD: 'DD',
  MM: 'MM',
}

