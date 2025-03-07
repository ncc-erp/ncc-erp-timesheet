import { TokenService } from '@abp/auth/token.service';
import { LogService } from '@abp/log/log.service';
import { MessageService } from '@abp/message/message.service';
import { UtilsService } from '@abp/utils/utils.service';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { UrlHelper } from '@shared/helpers/UrlHelper';
import { AuthenticateModel, AuthenticateResultModel, TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { catchError, finalize, map, startWith } from 'rxjs/operators';
import { PermissionCheckerService } from 'abp-ng2-module/dist/src/auth/permission-checker.service';
import { AppPreBootstrap } from 'AppPreBootstrap';
import { GoogleLoginService } from '@app/service/api/goole-login.service';
import { Observable, of, throwError } from '@node_modules/rxjs';
import { HttpErrorResponse } from '@node_modules/@angular/common/http';
import { MezonLoginService } from '@app/service/api/mezon-api.service';


@Injectable()
export class LoginService {
    static readonly twoFactorRememberClientTokenName = 'TwoFactorRememberClientToken';

    authenticateModel: AuthenticateModel;
    authenticateResult: AuthenticateResultModel;

    rememberMe: boolean;

    constructor(
        private _tokenAuthService: TokenAuthServiceProxy,
        private _router: Router,
        private _utilsService: UtilsService,
        private _messageService: MessageService,
        private _tokenService: TokenService,
        private _logService: LogService,
        private _googleLoginService: GoogleLoginService,
        private _permissionChecker: PermissionCheckerService,
        private _message: MessageService,
        private _mezonService: MezonLoginService,
        private router: Router
    ) {
        this.clear();
    }

    authenticate(finallyCallback?: () => void): void {
        finallyCallback = finallyCallback || (() => { });

        this._tokenAuthService
            .authenticate(this.authenticateModel)
            .pipe(finalize(() => { finallyCallback(); }))
            .subscribe((result: AuthenticateResultModel) => {
                this.processAuthenticateResult(result);
            });
    }

    authenticateMezon(token: string, scope: string): Observable<any> {
        return this._mezonService.mezonAuthenticate(token).pipe(
            map(data => {
                var result = this.processAuthenticateResult(data.result);
                return { ...data, loading: false }
            }),
            startWith({ loading: true, success: false }),
            catchError((err: HttpErrorResponse) => {
                this.router.navigate(['']);
                return of({ loading: false, success: false, error: err.error.error });
            }),
        );
    }

    authenticateGoogle(googleToken: string, secretCode: string, finallyCallback?: () => void): void {
        finallyCallback = finallyCallback || (() => { });

        this._googleLoginService.googleAuthenticate(googleToken, secretCode)
            .subscribe((result: any) => {
                this.processAuthenticateResult(result.result);
            });
    }

    private processAuthenticateResult(authenticateResult: AuthenticateResultModel) {
        this.authenticateResult = authenticateResult;

        if (authenticateResult.accessToken) {
            // Successfully logged in
            this.login(
                authenticateResult.accessToken,
                authenticateResult.encryptedAccessToken,
                authenticateResult.expireInSeconds,
                this.rememberMe);

        } else {
            // Unexpected result!

            this._logService.warn('Unexpected authenticateResult!');
            this._router.navigate(['account/login']);
        }
    }

    private login(accessToken: string, encryptedAccessToken: string, expireInSeconds: number, rememberMe?: boolean): void {

        const tokenExpireDate = rememberMe ? (new Date(new Date().getTime() + 1000 * expireInSeconds)) : undefined;

        this._tokenService.setToken(
            accessToken,
            tokenExpireDate
        );

        this._utilsService.setCookieValue(
            AppConsts.authorization.encrptedAuthTokenName,
            encryptedAccessToken,
            tokenExpireDate,
            abp.appPath
        );

        let initialUrl = UrlHelper.initialUrl;
        if (initialUrl.indexOf('/login') > 0) {
            initialUrl = AppConsts.appBaseUrl;
        }

        if(AppConsts.urlBeforeLogin != ""){
            initialUrl = AppConsts.urlBeforeLogin;
         }

        AppPreBootstrap.getUserConfiguration(() => {
            location.href = `${AppConsts.appBaseUrl}${this.selectBestRoute()}`;
        });

        location.href = initialUrl;
    }



    selectBestRoute(): string {
        if (this._permissionChecker.isGranted('Timesheet')) {
            return '/app/main/timesheets';
        }
        if (this._permissionChecker.isGranted('MyTimesheet')) {
            return '/app/main/mytimesheets';
        }
        return '/app';
    }



    private clear(): void {
        this.authenticateModel = new AuthenticateModel();
        this.authenticateModel.rememberClient = false;
        this.authenticateResult = null;
        this.rememberMe = false;
    }
}
