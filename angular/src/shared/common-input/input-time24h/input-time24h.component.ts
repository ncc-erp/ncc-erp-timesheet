import { IDataText } from '../input-type.interface';
import { Component, OnInit } from '@angular/core';
import { InputTypeBase } from '../input-type.interface';
import { convertMinuteToHour } from '@shared/common-time';

@Component({
  selector: 'app-input-time24h',
  templateUrl: './input-time24h.component.html',
  styleUrls: ['./input-time24h.component.less']
})
export class InputTime24hComponent extends InputTypeBase<IDataText> implements OnInit {
  // data: IDataText;
  placeholder: any = '';
  labelTop: any;
  label: any = 'never';
  oldValue: any;
  regularTime = new RegExp(/^([01]?\d|2[0-3]):([0-5]\d)$/);
  regularTimeTest = new RegExp(/^([01]?\d|2[0-3])?:(([0-5]?\d){1})?$/);
  set data(data: IDataText) {
    if (!data) return;
    if (data.placeholder) {
      this.placeholder = data.placeholder;
    }
    if (data.labelTop) {
      this.labelTop = data.labelTop;
      this.label = 'always';
    }
  }

  ngOnInit() {
    this.formControlInput.setValue(convertMinuteToHour(0));
    this.oldValue = this.formControlInput.value;
    this.formControlInput.valueChanges.subscribe(value => {
      if (this.regularTime.test(value) == true) {
        this.oldValue = value;
      }
      if (value == undefined || value == '') {
        this.formControlInput.setValue(convertMinuteToHour(0), { emitEvent: false });
      }
    })
  }

  checkReg(event: any) {
    let value = event.target.value;
    if (/\d/.test(event.key) == false || this.regularTimeTest.test(value) == false) {
        if (this.regularTimeTest.test(value) == false) {
          this.formControlInput.setValue(this.oldValue);
        }
    }
  }
}
