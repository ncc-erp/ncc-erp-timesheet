import { Component, OnInit, ViewEncapsulation, Injector } from '@angular/core';
import { LoginService } from './login/login.service';
import { AppComponentBase } from '@shared/app-component-base';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';
import { ActivatedRoute, Router } from '@node_modules/@angular/router';
import { IHashMezonAuthModel } from '@shared/service-proxies/service-proxies';
import { Base64 } from '@node_modules/js-base64/base64';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
    templateUrl: './account.component.html',
    styleUrls: [
        './account.component.less'
    ],
    encapsulation: ViewEncapsulation.None
})
export class AccountComponent extends AppComponentBase implements OnInit {
    userHash: any;

    hashData: string;
    isMezonApp: boolean = false;
    isAuthenFailed: boolean = false;
    isAuthenticating: boolean = false;
    isLoading: boolean = false;
    versionText: string;
    currentYear: number;

    public constructor(
        injector: Injector,
        private _authService: AppAuthService,
        private mezonWebViewService: MezonWebViewService,
        private route: ActivatedRoute,
        private _router: Router,
        public loginService: LoginService,
    ) {
        super(injector);

        this.currentYear = new Date().getFullYear();
        this.versionText = this.appSession.application.version + ' [' + this.appSession.application.releaseDate.format('YYYYDDMM') + ']';
    }

    showTenantChange(): boolean {
        return abp.multiTenancy.isEnabled;
    }

    ngOnInit(): void {
        const hashFromUrl = this.mezonWebViewService.getHashDataFromUrl();
        if (hashFromUrl) {
            this.isMezonApp = true;
            this.hashData = hashFromUrl;
            this.isAuthenticating = true;
            this.signInWithHash(hashFromUrl);
            return;
        }

        this.mezonWebViewService.isInMezon$.subscribe((status) => {
        this.isMezonApp = status;
        if (!this.isMezonApp) {
            $('body').attr('class', 'login-page');
        }
    });
    }

    signInWithHash(hashData: string) {
        if (hashData) {
          this.isAuthenticating = true;
          const hashAuthData: IHashMezonAuthModel = {
            hashData: Base64.encode(hashData),
          }
          this.loginService.authenticateMezonHash(hashAuthData, (error) => {
            this.isAuthenFailed = true;
          })
        }
      }
    
    
      // Hàm logout
      logout() {
        this._authService.logout();
      }
}
