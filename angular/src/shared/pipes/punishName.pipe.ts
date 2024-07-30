import { APP_CONSTANT } from '@app/constant/api.constants';
import { AppComponentBase } from 'shared/app-component-base';
import { Pipe, PipeTransform } from '@angular/core';
import { TimekeepingDto } from '@app/service/api/model/report-timesheet-Dto';
@Pipe({
    name: 'punishName'
})
    
export class PunishNamePipe implements PipeTransform {
    transform(data: TimekeepingDto, PunishSetting): string | undefined {
        let PunishString = "";
        if(data.statusPunish != APP_CONSTANT.PunishType.NoPunish)PunishString+=APP_CONSTANT.PunishRules.find(rule => rule.value === data.statusPunish).name + ": "+ this.MoneyToK(PunishSetting.find(rule => rule.id === data.statusPunish).money);
        if(data.dailyPunish != null && data.dailyPunish > 0) PunishString+="\n Daily: " + this.MoneyToK(PunishSetting.find(rule => rule.id === APP_CONSTANT.PunishType.NoDaily).money * data.dailyPunish);
        if(data.mentionPunish != null && data.mentionPunish > 0) PunishString+="\n Mention: " + this.MoneyToK(PunishSetting.find(rule => rule.id === APP_CONSTANT.PunishType.NoReplyMention).money  * data.mentionPunish);
        return PunishString;
    }
    MoneyToK(num: number): string {
        if (num === 0) {
          return '';
        }
        if (num >= 1000) {
          return (num / 1000).toFixed(1).replace(/\.0$/, '') + 'K';
        }
        return num.toString();
    }
}