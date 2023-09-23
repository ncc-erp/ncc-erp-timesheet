import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { MatInput } from '@angular/material';
import { FormControl } from '@angular/forms';
import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatDatepicker } from '@angular/material/datepicker';
import { AppComponentBase } from '@shared/app-component-base';
import * as _moment from 'moment';

export const MY_FORMATS = {
  parse: {
    dateInput: 'MM/YYYY'
  },
  display: {
    dateInput: 'MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY'
  }
};

@Component({
  selector: 'app-new-review',
  templateUrl: './create-review.component.html',
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS]
    },

    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS }
  ]
})
export class CreateReviewComponent extends AppComponentBase implements OnInit {
  public date = new FormControl(_moment());
  public review = {} as ReviewListDto;
  public saving: boolean = false;
  public title: string;
  public active: boolean = true;
  @ViewChild('input', { read: MatInput }) input: MatInput;

  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit() {
    this.review.isActive = true;
  }

  chosenYearHandler(normalizedYear: _moment.Moment) {
    const ctrlValue = this.date.value;
    ctrlValue.year(normalizedYear.year());
    this.date.setValue(ctrlValue);
  }

  chosenMonthHandler(normalizedMonth: _moment.Moment, datepicker: MatDatepicker<_moment.Moment>) {
    const ctrlValue = this.date.value;
    ctrlValue.month(normalizedMonth.month());
    this.date.setValue(ctrlValue);
    datepicker.close();
    let reviewDate = new Date(ctrlValue._d);
    this.review.year = reviewDate.getFullYear();
    this.review.month = reviewDate.getMonth() + 1;
  }

  clearInput() {}
}

export interface ReviewListDto {
  year?: number;
  month?: number;
  isActive?: boolean;
}
