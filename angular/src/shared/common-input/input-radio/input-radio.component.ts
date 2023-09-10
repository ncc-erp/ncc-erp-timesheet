import { IDataRadio } from './../input-type.interface';
import { Component, OnInit } from '@angular/core';
import { InputTypeBase } from '../input-type.interface';

@Component({
  selector: 'app-input-radio',
  templateUrl: './input-radio.component.html',
  styleUrls: ['./input-radio.component.less']
})
export class InputRadioComponent extends InputTypeBase<IDataRadio> implements OnInit {
// data: IDataRadio
inline: any = true;
options: any[] = [];

set data(data: IDataRadio) {
if (!data) return;
if (data.inline) {
  this.inline = data.inline;
}
if (data.options) {
  this.options = data.options;
}
}

ngOnInit(): void {
}

onClick(item){
this.formControlInput.setValue(item);
}
}
