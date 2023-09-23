import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'formatDisplayNumberToK'
})
    
export class FormatDisplayNumberToK implements PipeTransform {
  transform(num: number): string {
    if (num === 0) {
      return '';
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1).replace(/\.0$/, '') + 'K';
    }
    return num.toString();
  }
}