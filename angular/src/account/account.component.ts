import { Component, ViewContainerRef, OnInit, ViewEncapsulation, Injector } from '@angular/core';
import { LoginService } from './login/login.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';
@Component({
    templateUrl: './account.component.html',
    styleUrls: [
        './account.component.less'
    ],
    encapsulation: ViewEncapsulation.None
})
export class AccountComponent extends AppComponentBase implements OnInit {

    versionText: string;
    currentYear: number;

    private viewContainerRef: ViewContainerRef;

    public constructor(
        injector: Injector,
        private _loginService: LoginService,
        private _appAuthService: AppAuthService,
        private _mezonWebViewService: MezonWebViewService,
    ) {
        super(injector);

        console.log("I'm in constructor of account componentttttt constructor");
        this._mezonWebViewService.ping();
        this._mezonWebViewService.sendBotId();
        this._mezonWebViewService.listenToPong();
        this._mezonWebViewService.listenToUserHashInfo();

        this.currentYear = new Date().getFullYear();
        this.versionText = this.appSession.application.version + ' [' + this.appSession.application.releaseDate.format('YYYYDDMM') + ']';

    }

    showTenantChange(): boolean {
        return abp.multiTenancy.isEnabled;
    }

    ngOnInit(): void {
        $('body').attr('class', 'login-page');
    }
}
