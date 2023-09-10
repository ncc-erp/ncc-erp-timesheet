import { APP_CONSTANT } from '@app/constant/api.constants';
import { AppComponentBase } from 'shared/app-component-base';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'punishName'
})
    
export class PunishNamePipe implements PipeTransform {
    transform(value: number): string | undefined {
        const found = APP_CONSTANT.PunishRules.find(rule => rule.value === value);
        return found ? found.name : undefined;
      }
}