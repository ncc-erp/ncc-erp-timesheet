import { Injector, ElementRef } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { LocalizationService } from '@abp/localization/localization.service';
import { PermissionCheckerService } from '@abp/auth/permission-checker.service';
import { FeatureCheckerService } from '@abp/features/feature-checker.service';
import { NotifyService } from '@abp/notify/notify.service';
import { SettingService } from '@abp/settings/setting.service';
import { MessageService } from '@abp/message/message.service';
import { AbpMultiTenancyService } from '@abp/multi-tenancy/abp-multi-tenancy.service';
import { AppSessionService } from '@shared/session/app-session.service';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { APP_CONFIG } from '@app/constant/api-config.constant';
import { convertMinuteToHour, convertHourtoMinute ,convertFloatHourToMinute,convertMinuteToFloat } from './common-time';
import { Subscription } from 'rxjs'
import { DropdownData } from '@app/modules/capabilities/capability.component';
export abstract class AppComponentBase {
    public subscriptions: Subscription[] = []
    APP_CONSTANT = APP_CONSTANT;
    APP_CONFIG = APP_CONFIG;
    UserType = {
        Staff: 0,
        Intern: 1,
        Collaborator: 2
    };
    Level = {
        Intern_0: 0,
        Intern_1: 1,
        Intern_2: 2,
        Intern_3: 3,
        FresherMinus: 4,
        Fresher: 5,
        FresherPlus: 6,
        JuniorMinus: 7,
        Junior: 8,
        JuniorPlus: 9,
        MiddleMinus: 10,
        Middle: 11,
        MiddlePlus: 12,
        SeniorMinus: 13,
        Senior: 14,
        SeniorPlus: 15,
      }
    
      userLevels = [
        { value: this.Level.Intern_0, name: "Intern_0", style: {'background-color': '#B2BEB5'}},
        { value: this.Level.Intern_1, name: "Intern_1", style: {'background-color': '#8F9779'}},
        { value: this.Level.Intern_2, name: "Intern_2", style: {'background-color': '#665D1E'}},
        { value: this.Level.Intern_3, name: "Intern_3", style: {'background-color': '#777'}},
        { value: this.Level.FresherMinus, name: "Fresher-", style: {'background-color': '#2196f3'}},
        { value: this.Level.Fresher, name: "Fresher", style: {'background-color': '#89CFF0'}},
        { value: this.Level.FresherPlus, name: "Fresher+", style: {'background-color': '#318CE7'}},
        { value: this.Level.JuniorMinus, name: "Junior-", style: {'background-color': '#BFAFB2'}},
        { value: this.Level.Junior, name: "Junior", style: {'background-color': '#A57164'}},
        { value: this.Level.JuniorPlus, name: "Junior+", style: {'background-color': '#3B2F2F'}},
        { value: this.Level.MiddleMinus, name: "Middle-", style: {'background-color': '#A4C639'}},
        { value: this.Level.Middle, name: "Middle", style: {'background-color': '#8DB600'}},
        { value: this.Level.MiddlePlus, name: "Middle+", style: {'background-color': '#008000'}},
        { value: this.Level.SeniorMinus, name: "Senior-", style: {'background-color': '#F19CBB'}},
        { value: this.Level.Senior, name: "Senior", style: {'background-color': '#AB274F'}},
        { value: this.Level.SeniorPlus, name: "Senior+", style: {'background-color': '#E52B50'}},
      ];
      
    localizationSourceName = AppConsts.localization.defaultLocalizationSourceName;

    localization: LocalizationService;
    permission: PermissionCheckerService;
    feature: FeatureCheckerService;
    notify: NotifyService;
    setting: SettingService;
    message: MessageService;
    multiTenancy: AbpMultiTenancyService;
    appSession: AppSessionService;
    elementRef: ElementRef;

    constructor(injector: Injector) {
        this.localization = injector.get(LocalizationService);
        this.permission = injector.get(PermissionCheckerService);
        this.feature = injector.get(FeatureCheckerService);
        this.notify = injector.get(NotifyService);
        this.setting = injector.get(SettingService);
        this.message = injector.get(MessageService);
        this.multiTenancy = injector.get(AbpMultiTenancyService);
        this.appSession = injector.get(AppSessionService);
        this.elementRef = injector.get(ElementRef);
    }

    l(key: string, ...args: any[]): string {
        let localizedText = this.localization.localize(key, this.localizationSourceName);

        if (!localizedText) {
            localizedText = key;
        }

        if (!args || !args.length) {
            return localizedText;
        }

        args.unshift(localizedText);
        return abp.utils.formatString.apply(this, args);
    }

    getVersionReleaseDate(){
        return this.appSession.application.version + this.appSession.application.releaseDate
    }

    isGranted(permissionName: string): boolean {
        return this.permission.isGranted(permissionName);
    }

    convertMinuteToHour(minute: number) {
        return convertMinuteToHour(minute);               
    }
    convertHourtoMinute(hour:string){
        return convertHourtoMinute(hour)
    }
    convertFloatHourToMinute(floatHour:string){
        return convertFloatHourToMinute(floatHour)
    }
    convertMinuteToFloat(IntHour:number){
        return convertMinuteToFloat(IntHour)
    }
    getAvatar(member) {
        if (member.avatarFullPath) {
          return member.avatarFullPath;
        }
        if (member.sex == 1) {
          return 'assets/images/women.png';
        }
        return 'assets/images/men.png';
    }
    convertEnumToDropdown(typeEnum: Object):DropdownData[]{
        return Object.keys(typeEnum).filter(value => isNaN(Number(value)))
        .map(key => ({
          key: key,
          value: typeEnum[key] as number
        }))
    } 
    public getStarColorforReviewInternCapability(average, isClass){
        if (average < 2.5) {
          return 'grey'
        }
        if (average < 3.5) {
          return 'yellow'
        }
        if (average < 4.5) {
          return 'orange'
        }
        else {
          return ''
        }
    
      }
}
