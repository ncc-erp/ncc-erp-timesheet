import { Injectable } from '@angular/core';
import { Subject } from '@node_modules/rxjs';
import { AppConsts } from '@shared/AppConsts';
// import { MezonAppEvent, MezonWebViewEvent } from 'types/webview';

@Injectable()
export class AppAuthService {
    private userHashData = new Subject<string>();
    private isInMezon = new Subject<boolean>();

    userHashData$ = this.userHashData.asObservable();
    isInMezon$ = this.isInMezon.asObservable();


    logout(reload?: boolean): void {
        abp.auth.clearToken();
        abp.utils.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, undefined, undefined, abp.appPath);
        if (reload !== false) {
            location.href = AppConsts.appBaseUrl;
        }
    }
}
