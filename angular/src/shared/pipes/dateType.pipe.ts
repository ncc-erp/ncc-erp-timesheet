import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'dayType'
})
    
export class DayTypePipe implements PipeTransform {
    transform(value: string): string {
        value = value.replace('Custom', 'Đi muộn Về sớm')        
        return value;
      }
}