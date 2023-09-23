import { IDataSelectBasic } from './../input-type.interface';
import { Component, OnInit } from '@angular/core';
import { InputTypeBase } from '../input-type.interface';

@Component({
  selector: 'app-input-select',
  templateUrl: './input-select.component.html',
  styleUrls: ['./input-select.component.less']
})
export class InputSelectComponent extends InputTypeBase<IDataSelectBasic> implements OnInit {
  placeholder: any = ''
  options: any[] = [];
  labelTop: any;
  label: any = 'never';
  multiple: any = false;

  set data(data: IDataSelectBasic) {
    if (!data) return;
    if (data.placeholder) {
      this.placeholder = data.placeholder;
    }
    if (data.options) {
      this.options = data.options;
    }
    if (data.labelTop){
      this.labelTop = data.labelTop;
      this.label = 'always';
    }
    if (data.multiple) {
      this.multiple = data.multiple;
    }
  }

  ngOnInit(): void {
  }

}