import { Component, OnInit, Injector, Input, Output, EventEmitter, Optional, Inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import * as moment from 'moment';
import { AppComponentBase } from '@shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialog } from '@angular/material';
import { PopupComponent } from './popup/popup.component';

@Component({
  selector: 'date-selector',
  templateUrl: './date-selector.component.html',
  styles: []
})
export class DateSelectorComponent extends AppComponentBase implements OnInit {
  viewChange: FormControl = new FormControl();
  activeView;

  // tslint:disable-next-line: no-input-rename
  @Input('type') defaultDateType = this.APP_CONSTANT.TypeViewHomePage.Month;

  @Input() defaultFromDate = undefined;
  @Input() defaultToDate = undefined;

  @Output() onDateSelectorChange: EventEmitter<{ fromDate, toDate }> = new EventEmitter<{ fromDate, toDate }>();

  distanceFromAndToDate: string;

  constructor(
    injector: Injector,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any,
    private _dialog: MatDialog,
  ) {
    super(injector);
  }

  ngOnInit() {
    this.viewChange.setValue(this.defaultDateType);
    if(this.defaultFromDate !== undefined && this.defaultToDate !== undefined) {
      this.changeView(false, this.defaultFromDate, this.defaultToDate);
    }
    else {
      this.changeView(true);
    }
  }

  changeView(reset?: boolean, fDate?: any, tDate?: any) {
    if (reset) {
      this.activeView = 0;
    }
    let fromDate, toDate, typeDate;

    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Week) {
      fromDate = moment().startOf('isoWeek').add(this.activeView, 'w');
      toDate = moment(fromDate).endOf('isoWeek');
      typeDate = 'Week';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Month) {
      fromDate = moment().startOf('M').add(this.activeView, 'M');
      toDate = moment(fromDate).endOf('M');
      typeDate = 'Month';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Quater) {
      fromDate = moment().startOf('Q').add(this.activeView, 'Q');
      toDate = moment(fromDate).endOf('Q');
      typeDate = 'Quater';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.Year) {
      fromDate = moment().startOf('y').add(this.activeView, 'y');
      toDate = moment(fromDate).endOf('y');
      typeDate = 'Years';
    }
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.AllTime) {
      fromDate = '';
      toDate = '';
      this.distanceFromAndToDate = 'All Time';
    }

    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.CustomTime) {
      fromDate = '';
      toDate = '';
      if (!reset && fDate && tDate) {
        fromDate = fDate;
        toDate = tDate;

      } else {
        this.distanceFromAndToDate = 'Custom Time';
        return;
      }
    }

    if (fromDate && toDate) {
      let tmpfDate = '', tmptDate = '';
      const list = [];
      list[0] = { value: fromDate.isSame(toDate, 'year'), type: 'YYYY' };
      list[1] = { value: fromDate.isSame(toDate, 'month'), type: 'MMM' };
      list[2] = { value: fromDate.isSame(toDate, 'day'), type: 'D' };
      list.map(value => {
        if (value.value) {
          tmptDate = toDate.format(value.type) + ' ' + tmptDate;
        } else {
          tmpfDate = fromDate.format(value.type) + ' ' + tmpfDate;
          tmptDate = toDate.format(value.type) + ' ' + tmptDate;
        }
      });
      this.distanceFromAndToDate = (this.viewChange.value !== this.APP_CONSTANT.TypeViewHomePage.CustomTime) ? `${typeDate}: ` : '';
      this.distanceFromAndToDate += tmpfDate + ' - ' + tmptDate;
    }

    fromDate = fromDate === '' ? '' : fromDate.format('YYYY-MM-DD');
    toDate = toDate === '' ? '' : toDate.format('YYYY-MM-DD');

    this.onDateSelectorChange.emit({ fromDate, toDate });
  }

  nextOrPre(title: any) {
    if (this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.CustomTime
      || this.viewChange.value === this.APP_CONSTANT.TypeViewHomePage.AllTime) {
      return;
    }
    if (title === 'pre') {
      this.activeView--;
    }
    if (title === 'next') {
      this.activeView++;
    }
    this.changeView();
  }

  showPopup(): void {
    const popup = this._dialog.open(PopupComponent);
    popup.afterClosed().subscribe(result => {
      if (result !== undefined) {
        if (result.result) {
          this.changeView(false, result.data.fromDateCustomTime, result.data.toDateCustomTime);
        }
      }
    });
  }

}
