import { AbstractControl } from '@angular/forms';
import { Observable, Subject } from 'rxjs';
import { EventEmitter } from '@angular/core';

// export interface IInputType {
//   formControlInput: AbstractControl;
//   data: IDataSelect | IDataText | IDataCheckbox | any;
//   selected?: EventEmitter<any>;
// }
export abstract class InputTypeBase<T> {
  protected refreshSource: Subject<any> = new Subject();
  formControlInput: AbstractControl;
 
  _data: any = {};

  get data() {
    return this._data;
  }

  set data(data: T) {
    // console.log('set data base');
    for (let key of Object.keys(data)) {
      this._data[key] = data[key];
    }

    this.dataChange(this._data);
  }

  dataChange(data: T) {
    // console.log('dataChange base');
  }

  refresh(){
    this.refreshSource.next(this.formControlInput.value || '');
  }
}

export interface IDataInput {
  placeholder?: string;
  labelTop?: any;
}

export interface IDataSelect extends IDataInput {
  selected?: EventEmitter<any>;
  removed?: EventEmitter<any>;
  options?: Array<any>;
  valueType?: string;
  idType?: string;
  outputType?: string;
  multiple?: boolean;
  disabled?: boolean;
}
export interface IDataAutocomplete extends IDataInput {
  valueType?: string;
  idType?: string;
  api: Function;
}

export interface IDataSelectBasic extends IDataInput {
  options?: Array<any>;
  multiple?: boolean;
}

export interface IDataText extends IDataInput {
  idType: any;
  search?: Function;
  selected: EventEmitter<any>;
  valueType?: any;
  options?: Array<any>;
  step?: number;
  textRight?: boolean;
  displayStep?: boolean;
}

export interface IDataTextarea extends IDataInput {
  rows?: any;
}

export interface IDataDate extends IDataInput {

}

export interface IDataCheckbox extends IDataInput {
  valueType: any;
  idType: any;
  text?: string;
  list?: any[];
}

export interface IDataFile extends IDataInput {
  type?: number;
}

export interface IDataRadio extends IDataInput {
  inline?: boolean;
  options?: Array<IDataRadioOption>;
}

export interface IDataRadioOption {
  text?: string;
  value?: any;
  class?: string;
}
