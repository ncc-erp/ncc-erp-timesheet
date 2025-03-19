import { Component, Injector, OnInit } from '@angular/core';
import { AbpSessionService } from '@abp/session/abp-session.service';
import { AppComponentBase } from '@shared/app-component-base';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { LoginService } from './login.service';
import { AuthService, SocialUser } from "angularx-social-login";
import { GoogleLoginProvider } from "angularx-social-login";
import { AppConsts } from '@shared/AppConsts';
import { MezonLoginService } from '@app/service/api/mezon-api.service';
import { IHashMezonAuthModel } from '@shared/service-proxies/service-proxies';
import { Base64 } from '@node_modules/js-base64/base64';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.less'],
  animations: [accountModuleAnimation()]
})
export class LoginComponent extends AppComponentBase implements OnInit {
  submitting = false;
  nccCode: string;
  isShowPassword = true;
  enableNormalLogin: boolean;

  hashData: string;
  isMezonApp: boolean = false;
  isAuthenFailed: boolean = false;
  isAuthenticating: boolean = false;



  constructor(
    injector: Injector,
    public loginService: LoginService,
    private _sessionService: AbpSessionService,
    private authService: AuthService,
    private mezonLoginService: MezonLoginService,
    private _appAuthService: AppAuthService

  ) {
    super(injector);
  }


  ngOnInit() {
    console.log("In constructor of login")
    this._appAuthService.isInMezon$.subscribe((status) => {
      this.isMezonApp = status;
    });

    this._appAuthService.userHashData$.subscribe((userHashData) => {
      this.hashData = userHashData;
      this.isAuthenticating = true;
      console.log('userHashData: ', userHashData);
      this.signInWithHash(this.hashData);
    });

    console.log('Sign in with hash is coming')

    this.enableNormalLogin = AppConsts.enableNormalLogin;

    // Disable Auto Login By Google
    this.authService.authState.subscribe((user) => {
      // if (user) {
      //   this.loginService.authenticateGoogle(user.idToken, this.nccCode);
      // }
    }, err => this.authService.signOut());
  }


  signInWithHash(hashData: string) {
    if (hashData) {
      this.isAuthenticating = true;
      const hashAuthData: IHashMezonAuthModel = {
        hashData: Base64.encode(hashData),
      }
      this.loginService.authenticateMezonHash(hashAuthData, (error) => {
        console.log("Error: ", error);
        this.isAuthenFailed = true;
      })
    }
  }


  checkShowpass() {
    this.isShowPassword = !this.isShowPassword;
  }
  get multiTenancySideIsTeanant(): boolean {
    return this._sessionService.tenantId > 0;
  }

  get isSelfRegistrationAllowed(): boolean {
    if (!this._sessionService.tenantId) {
      return false;
    }

    return true;
  }

  login(): void {
    this.submitting = true;
    this.loginService.authenticate(() => (this.submitting = false));
  }

  signInWithGoogle(): void {
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }

  signInWithMezon(): void {
    this.mezonLoginService.redirectToOAuth();
  }

  signOut(): void {
    this.authService.signOut();
  }
}
