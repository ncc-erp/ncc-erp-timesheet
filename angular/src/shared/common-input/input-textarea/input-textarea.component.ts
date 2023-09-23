import { IDataTextarea } from './../input-type.interface';
import { Component, OnInit } from '@angular/core';
import { InputTypeBase } from '../input-type.interface';

@Component({
  selector: 'app-input-textarea',
  templateUrl: './input-textarea.component.html',
  styleUrls: ['./input-textarea.component.less']
})
export class InputTextareaComponent extends InputTypeBase<IDataTextarea> implements OnInit {
//data: IDataTextarea
placeholder: any = '';
rows: number = 1;
labelTop: any;
label: any = 'never';

set data(data: IDataTextarea) {
if (!data) return;
if (data.placeholder) {
  this.placeholder = data.placeholder;
}
if (data.labelTop) {
  this.labelTop = data.labelTop;
  this.label = 'always';
}
if (data.rows) {
  this.rows = data.rows;
}
}

ngOnInit(): void {

}

onBlur(event) {
this.formControlInput.setValue((event.target.value + '').trim(), { emitEvent: false });
}
}
