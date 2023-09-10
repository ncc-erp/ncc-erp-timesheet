import { Injectable } from '@angular/core'
import { NativeDateAdapter } from '@angular/material';
import * as moment from 'moment';
@Injectable({
    providedIn: 'root'
})
export class CustomDateAdapter extends NativeDateAdapter {
    format(date: Date, displayFormat: Object): string {
        return moment(date).format('DD/MM/YYYY')
    }
}
export const APP_DATE_FORMATS = {
    parse: {
        dateInput: 'DD/MM/YYYY',
      },
      display: {
        dateInput: 'DD/MM/YYYY',
        monthYearLabel: 'MMMM YYYY',
        dateA11yLabel: 'DD/MM/YYYY',
        monthYearA11yLabel: 'MMMM YYYY',
      },
};