import { InputRadioComponent } from './input-radio/input-radio.component';
import { InputSelectComponent } from './input-select/input-select.component';
import { InputTextareaComponent } from './input-textarea/input-textarea.component';
import { InputTextComponent } from './input-text/input-text.component';
import { Injectable } from '@angular/core';
import { InputAutocompeteComponent } from './input-autocomplete/input-autocomplete.component';
import { InputTime24hComponent } from './input-time24h/input-time24h.component';

@Injectable({
  providedIn: 'root'
})
export class CommonInputService {

  constructor() { }

  getInputComponentByType(type: any): any {
    let component;
      switch(type){
        case 'text':
        component = InputTextComponent;
        break;

        case 'textarea':
        component = InputTextareaComponent;
        break;
        
        case 'select':
        component = InputSelectComponent;
        break;

        case 'radio':
        component = InputRadioComponent;
        break;
        
        case 'autocomplete':
        component = InputAutocompeteComponent;
        break;

        case 'time24h':
        component = InputTime24hComponent;
      }

    return component;
  }
}
