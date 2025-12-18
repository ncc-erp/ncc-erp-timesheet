import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { MezonAppEvent, MezonWebViewEvent } from 'types/webview';
import { AppConsts } from '@shared/AppConsts';

@Injectable({
    providedIn: 'root'
})
export class MezonWebViewService {
    private userHashData = new Subject<any>();
    private isInMezon = new Subject<boolean>();
    private _isInMezon = false;

    userHashData$ = this.userHashData.asObservable();
    isInMezon$ = this.isInMezon.asObservable();

    constructor() {
        this._isInMezon = localStorage.getItem('isInMezon') === 'true';
    }

    private setInMezonStatus(isInMezon: boolean): void {
        this._isInMezon = isInMezon;
        localStorage.setItem('isInMezon', isInMezon.toString());
    }

    checkIfInMezon(): boolean {
        return this._isInMezon;
    }

    ping() {
        window.Mezon.WebView.postEvent("PING" as MezonWebViewEvent, { message: "PING" }, () => {
        });
    }

    listenToPong() {
        window.Mezon.WebView.onEvent("PONG" as MezonAppEvent, () => {
            this.setInMezonStatus(true);
            this.isInMezon.next(true);
        });
    }

    clearMezonStatus(): void {
        this.setInMezonStatus(false);
        localStorage.removeItem('isInMezon');
    }

    sendBotId() {
        window.Mezon.WebView.postEvent("SEND_BOT_ID" as MezonWebViewEvent, { appId: AppConsts.mezonAppId }, () => {
        });
    }

    listenToUserHashInfo() {
        window.Mezon.WebView.onEvent("USER_HASH_INFO" as MezonAppEvent, async (_, userHashData: any) => {
            this.userHashData.next(userHashData.message.web_app_data);
        });
    }

    getHashDataFromUrl(): string | null {
        const url = new URL(window.location.href);
        const queryData = url.searchParams.get('data');
        return queryData ? queryData : null;
    }

    removeEventListeners() {
        window.Mezon.WebView.offEvent("CURRENT_USER_INFO" as MezonAppEvent, () => { });
        window.Mezon.WebView.offEvent("USER_HASH_INFO" as MezonAppEvent, () => { });
    }

    logout() {
        console.log('User logged out');
    }

}
