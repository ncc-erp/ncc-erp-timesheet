import { Injectable } from '@angular/core';
import { Subject } from '@node_modules/rxjs';
import { AppConsts } from '@shared/AppConsts';
// import { MezonAppEvent, MezonWebViewEvent } from 'types/webview';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';


@Injectable()
export class AppAuthService {
    private userHashData = new Subject<string>();
    private isInMezon = new Subject<boolean>();

    userHashData$ = this.userHashData.asObservable();
    isInMezon$ = this.isInMezon.asObservable();

    constructor(
        private _mezonWebViewService: MezonWebViewService
    ) { }


    logout(reload?: boolean): void {
        if (this._mezonWebViewService.checkIfInMezon()) {
            abp.message.warn('Không thể đăng xuất khi đang sử dụng Mezon WebView');
            return;
        }

        this._mezonWebViewService.clearMezonStatus();

        abp.auth.clearToken();
        abp.utils.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, undefined, undefined, abp.appPath);

        if (reload !== false) {
            location.href = AppConsts.appBaseUrl;
        }
    }
}
