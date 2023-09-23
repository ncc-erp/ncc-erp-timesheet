import * as moment from 'moment';
import { AppConsts } from '@shared/AppConsts';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { Type, CompilerOptions, NgModuleRef } from '@angular/core';
import { environment } from './environments/environment';
import abpsetting from 'abpsetting';

export class AppPreBootstrap {
    static run(appRootUrl: string, callback: () => void): void {
        AppPreBootstrap.getApplicationConfig(appRootUrl, () => {
            AppPreBootstrap.getUserConfiguration(callback);
            AppPreBootstrap.getGoogleClientAppId(callback);
        });
    }

    static bootstrap<TM>(moduleType: Type<TM>, compilerOptions?: CompilerOptions | CompilerOptions[]): Promise<NgModuleRef<TM>> {
        return platformBrowserDynamic().bootstrapModule(moduleType, compilerOptions);
    }

    private static getApplicationConfig(appRootUrl: string, callback: () => void) {
        return abp.ajax({
            url: appRootUrl + 'assets/' + environment.appConfig,
            method: 'GET',
            headers: {
                'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
            }
        }).done(result => {
            AppConsts.appBaseUrl = result.appBaseUrl;
            AppConsts.remoteServiceBaseUrl = result.remoteServiceBaseUrl;
            AppConsts.localeMappings = result.localeMappings;
            AppConsts.enableNormalLogin = result.enableNormalLogin;
            AppConsts.backendIsNotABP = result.backendIsNotABP;
            callback();
        });
    }

    private static getGoogleClientAppId(callback: () => void) {
        if (AppConsts.backendIsNotABP){
            callback();
        }else{
            return abp.ajax({
                url: AppConsts.remoteServiceBaseUrl + '/api/services/app/Configuration/GetGoogleClientAppId',
                method: 'GET',
                headers: {
                    'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
                }
            }).done(result => {
                AppConsts.googleClientAppId = result;
                callback();
            });
        }
        
    }

    private static getCurrentClockProvider(currentProviderName: string): abp.timing.IClockProvider {
        if (currentProviderName === 'unspecifiedClockProvider') {
            return abp.timing.unspecifiedClockProvider;
        }

        if (currentProviderName === 'utcClockProvider') {
            return abp.timing.utcClockProvider;
        }

        return abp.timing.localClockProvider;
    }

    public static getUserConfiguration(callback: () => void): JQueryPromise<any> {
        if (AppConsts.backendIsNotABP){
            $.extend(true, abp, abpsetting.result);
            abp.clock.provider = abp.timing.utcClockProvider;    
            moment.locale('en');
            callback();
        }else{
            return abp.ajax({
                url: AppConsts.remoteServiceBaseUrl + '/AbpUserConfiguration/GetAll',
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + abp.auth.getToken(),
                    '.AspNetCore.Culture': abp.utils.getCookieValue('Abp.Localization.CultureName'),
                    'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
                }
            }).done(result => {            
                
                $.extend(true, abp, AppConsts.backendIsNotABP ? abpsetting.result : result);                  
                
                abp.clock.provider = abp.timing.utcClockProvider;
    
                moment.locale('en');    
               
                callback();
            });
        }
        
    }

}
