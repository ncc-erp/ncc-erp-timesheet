import { Component, OnInit} from '@angular/core';
import { Observable } from 'rxjs';
import { IDataText, InputTypeBase } from '../input-type.interface';
import { map, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-input-text',
  templateUrl: './input-text.component.html',
  styleUrls: ['./input-text.component.less']
})
export class InputTextComponent extends InputTypeBase<IDataText> implements OnInit{
  // data: IDataText;
  placeholder: any = '';
  options: any;
  labelTop: any;
  filteredOptions: Observable<any[]>;
  label: any = 'never';

  set data(data: IDataText) {
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
  }

  ngOnInit(){
    if(this.options != undefined && this.options != null && this.options != ''){
      this.filteredOptions = this.formControlInput.valueChanges
      .pipe(
        startWith(''),
        map(value => this._filter(value))
      )
    }
  }

  _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.options.filter(option => option.toLowerCase().includes(filterValue));
  }

}
