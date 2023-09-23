import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_LOCALE, MAT_DATE_FORMATS } from '@angular/material/core';
import { MatDatepicker } from '@angular/material/datepicker';
import { FormControl } from '@angular/forms';
import { Component, OnInit, ViewChild } from '@angular/core';
import * as _moment from 'moment';
import { MatInput } from '@angular/material';
import { DATE_FORMATS, InputForGenerateDto } from '../../const/const';

@Component({
  selector: 'app-generate-data',
  templateUrl: './generate-data.component.html',
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS]
    },

    { provide: MAT_DATE_FORMATS, useValue: DATE_FORMATS }
  ]
})
export class GenerateDataComponent implements OnInit {
  public date = new FormControl(_moment());
  public inputForGenerate = {} as InputForGenerateDto;
  public saving: boolean = false;
  public title: string;
  public active: boolean = true;
  @ViewChild('input', { read: MatInput }) input: MatInput;
  constructor() { }

  ngOnInit() {
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
    this.inputForGenerate.year = reviewDate.getFullYear();
    this.inputForGenerate.month = reviewDate.getMonth() + 1;
  }

  clearInput() {}
}


