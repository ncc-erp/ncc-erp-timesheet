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
        let remainingAmount = data.moneyPunish;
        if(data.statusPunish != APP_CONSTANT.PunishType.NoPunish){
          let checkInOutPunish = PunishSetting.find(rule => rule.id === data.statusPunish).money;
          remainingAmount-=checkInOutPunish;
          PunishString+=APP_CONSTANT.PunishRules.find(rule => rule.value === data.statusPunish).name + ": "+ this.MoneyToK(checkInOutPunish);
        }
        if(data.dailyPunish != null && data.dailyPunish > 0){
          let dailyPunish = PunishSetting.find(rule => rule.id === APP_CONSTANT.PunishType.NoDaily).money * data.dailyPunish;
          remainingAmount-=dailyPunish;
          PunishString+="\n Daily: " + this.MoneyToK(dailyPunish);
        }
        if(data.mentionPunish != null && data.mentionPunish > 0){
          let mentionPunish = PunishSetting.find(rule => rule.id === APP_CONSTANT.PunishType.NoReplyMention).money  * data.mentionPunish;
          remainingAmount-=mentionPunish;
          PunishString+="\n Mention: " + this.MoneyToK(mentionPunish);
        }
        if(remainingAmount != 0) PunishString+="\n Tracker: " + this.MoneyToK(remainingAmount);
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