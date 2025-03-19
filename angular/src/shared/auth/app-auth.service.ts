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


    // ping() {
    //     window.Mezon.WebView.postEvent("PING" as MezonWebViewEvent, { message: "PING" }, () => { })
    // }

    // listenToPong() {
    //     window.Mezon.WebView.onEvent("PONG" as MezonAppEvent, () => {
    //         this.isInMezon.next(true);
    //     });
    // }

    // sendBotId() {
    //     window.Mezon.WebView.postEvent("SEND_BOT_ID" as MezonWebViewEvent, { appId: AppConsts.mezonAppId }, () => { })
    // }

    // listenToUserHashInfo() {
    //     window.Mezon.WebView.onEvent("USER_HASH_INFO" as MezonAppEvent, async (_, data: any) => {
    //         this.userHashData.next(data.message.web_app_data);
    //     });
    // }

    // removeEventListeners() {
    //     window.Mezon.WebView.offEvent("CURRENT_USER_INFO" as MezonAppEvent, () => { })
    //     window.Mezon.WebView.offEvent("USER_HASH_INFO" as MezonAppEvent, () => { })
    // }

    logout(reload?: boolean): void {
        abp.auth.clearToken();
        abp.utils.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, undefined, undefined, abp.appPath);
        if (reload !== false) {
            location.href = AppConsts.appBaseUrl;
        }
    }
}
