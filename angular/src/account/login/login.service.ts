import { TokenService } from '@abp/auth/token.service';
import { LogService } from '@abp/log/log.service';
import { MessageService } from '@abp/message/message.service';
import { UtilsService } from '@abp/utils/utils.service';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { UrlHelper } from '@shared/helpers/UrlHelper';
import { catchError, finalize, map, startWith } from 'rxjs/operators';
import { PermissionCheckerService } from 'abp-ng2-module/dist/src/auth/permission-checker.service';
import { AppPreBootstrap } from 'AppPreBootstrap';
import { GoogleLoginService } from '@app/service/api/goole-login.service';
import { Observable, of } from '@node_modules/rxjs';
import { HttpErrorResponse } from '@node_modules/@angular/common/http';
import { MezonLoginService } from '@app/service/api/mezon-api.service';
import { AuthenticateModel, AuthenticateResultModel, IHashMezonAuthModel, TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';
import { STORAGE_KEYS } from '@app/constant/storage-keys.constant';
import { IEphemeralKeyPair, IZkProof } from '@node_modules/mmn-client-js/dist';
import { mmnClient, zkClient } from '@shared/mmn-clients';


@Injectable()
export class LoginService {
    static readonly twoFactorRememberClientTokenName = 'TwoFactorRememberClientToken';
    private mmn = mmnClient;
    private zk = zkClient;
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
        private router: Router,
        private mezonWebViewService: MezonWebViewService
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
            map(async data => {
                var result = await this.processAuthenticateResult(data.result);
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

    private async processAuthenticateResult(authenticateResult: AuthenticateResultModel) {
        this.authenticateResult = authenticateResult;
        let senderAddress: string | undefined;
        let keyPair: IEphemeralKeyPair | undefined;
        let zkProof: IZkProof | undefined;

        if (authenticateResult.mezonUserId) {
            senderAddress = this.mmn.getAddressFromUserId(authenticateResult.mezonUserId);
            keyPair = this.mmn.generateEphemeralKeyPair();
            zkProof = await this.zk.getZkProofs({
                userId: authenticateResult.mezonUserId,
                ephemeralPublicKey: keyPair.publicKey,
                jwt: authenticateResult.authToken,
                address: senderAddress,
            });
        }
        if (authenticateResult.accessToken) {
            this.login(
                authenticateResult.accessToken,
                authenticateResult.encryptedAccessToken,
                authenticateResult.expireInSeconds,
                authenticateResult.authToken,
                authenticateResult.mezonUserId,
                senderAddress,
                keyPair,
                zkProof,
                this.rememberMe);

        } else {
            // Unexpected result!
            this._logService.warn('Unexpected authenticateResult!');
            this.mezonWebViewService.ping();
            this.mezonWebViewService.isInMezon$.subscribe((status) => {
                if (!status) {
                    this._router.navigate(['account/login']);
                }
            });
        }
    }

    private login(
        accessToken: string,
        encryptedAccessToken: string,
        expireInSeconds: number,
        authToken: string,
        mezonUserId?: string,
        senderAddress?: string,
        keyPair?: IEphemeralKeyPair,
        zkProof?: IZkProof,
        rememberMe?: boolean
    ): void {

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

        if (AppConsts.urlBeforeLogin != "") {
            initialUrl = AppConsts.urlBeforeLogin;
        }

        AppPreBootstrap.getUserConfiguration(() => {
            location.href = `${AppConsts.appBaseUrl}${this.selectBestRoute()}`;
        });
      
        if (mezonUserId) {
            localStorage.setItem(STORAGE_KEYS.MEZON_USER_ID, mezonUserId);
        }
        if (authToken) {
            localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, authToken);
        }
        if (senderAddress) {
            localStorage.setItem(STORAGE_KEYS.SENDER_ADDRESS, senderAddress);
        }
        if (keyPair) {
            localStorage.setItem(STORAGE_KEYS.KEY_PAIR, JSON.stringify(keyPair));
        }
        if (zkProof) {
            localStorage.setItem(STORAGE_KEYS.ZK_PROOF, JSON.stringify(zkProof));
        }
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

  
    authenticateMezonHash(authDto: IHashMezonAuthModel, errorHandller?: (error?: any) => any): void {
        this._tokenAuthService
            .mezonHashAuthenticate(authDto)
            .pipe(
                finalize(() => { }),
                catchError((error) => {
                    return errorHandller(error);
                })
            )
            .subscribe((result: AuthenticateResultModel) => {
                console.log(result)
                this.processAuthenticateResult(result);
            })
    }
}
