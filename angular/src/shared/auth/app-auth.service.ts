import { Injectable } from '@angular/core';
import { Subject } from '@node_modules/rxjs';
import { AppConsts } from '@shared/AppConsts';
// import { MezonAppEvent, MezonWebViewEvent } from 'types/webview';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';
import { STORAGE_KEYS } from '@app/constant/storage-keys.constant';


@Injectable()
export class AppAuthService {
    private userHashData = new Subject<string>();
    private isInMezon = new Subject<boolean>();

    userHashData$ = this.userHashData.asObservable();
    isInMezon$ = this.isInMezon.asObservable();

    constructor(
        private _mezonWebViewService: MezonWebViewService
    ) { }

    clearStorageKeys = (keys) => {
        if (!Array.isArray(keys)) {
            console.error('Input phải là một mảng (array)');
            return;
        }
    
        keys.forEach((key) => {
            if (key) {
                localStorage.removeItem(key);
            }
        });
    }
    logout(reload?: boolean): void {
        if (this._mezonWebViewService.checkIfInMezon()) {
            abp.message.warn('Không thể đăng xuất khi đang sử dụng Mezon WebView');
            return;
        }

        this._mezonWebViewService.clearMezonStatus();
        this.clearStorageKeys([
            STORAGE_KEYS.AUTH_TOKEN,
            STORAGE_KEYS.MEZON_USER_ID,
            STORAGE_KEYS.KEY_PAIR,
            STORAGE_KEYS.ZK_PROOF,
            STORAGE_KEYS.SENDER_ADDRESS
        ]);
        abp.auth.clearToken();
        abp.utils.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, undefined, undefined, abp.appPath);

        if (reload !== false) {
            location.href = AppConsts.appBaseUrl;
        }
    }
}
