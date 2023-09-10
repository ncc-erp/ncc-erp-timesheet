import { IDataAutocomplete } from './../input-type.interface';
import { Component, OnInit } from '@angular/core';
import { InputTypeBase } from '../input-type.interface';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { startWith, map, concat, debounceTime, merge } from 'rxjs/operators';

@Component({
  selector: 'app-input-autocomplete',
  templateUrl: './input-autocomplete.component.html',
  styleUrls: ['./input-autocomplete.component.less']
})
export class InputAutocompeteComponent extends InputTypeBase<IDataAutocomplete> implements OnInit {
  filteredOptions: Array<any> = [];

  _data = {
    valueType: 'name',
    idType: 'id'
  }

  ngOnInit() {
    this.formControlInput.valueChanges
      .pipe(
        startWith(''),
        merge(this.refreshSource),
        debounceTime(300)
      ).subscribe(val => {
        if (this.data.api) {
          this.data.api(val).subscribe(res => {
            this.filteredOptions = (res || []).map(r => {
              r.empty = '';
              return r;
            });
          });
        }
      });
  }

  dataChange(data: IDataAutocomplete) {
    // console.log('dataChange base');
  }
}
