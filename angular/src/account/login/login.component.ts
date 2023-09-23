import { Component, Injector, OnInit } from '@angular/core';
import { AbpSessionService } from '@abp/session/abp-session.service';
import { AppComponentBase } from '@shared/app-component-base';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { LoginService } from './login.service';
import { AuthService, SocialUser } from "angularx-social-login";
import { GoogleLoginProvider } from "angularx-social-login";
import { AppConsts } from '@shared/AppConsts';
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
  // private user: SocialUser;
  // private loggedIn: boolean;

  constructor(
    injector: Injector,
    public loginService: LoginService,
    private _sessionService: AbpSessionService,
    private authService: AuthService,
  ) {
    super(injector);
  }


  ngOnInit() {
    this.enableNormalLogin = AppConsts.enableNormalLogin;
    this.authService.authState.subscribe((user) => {
      if (user) {
        this.loginService.authenticateGoogle(user.idToken, this.nccCode);
      }
    }, err => this.authService.signOut());
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
    //alert(GoogleLoginProvider.PROVIDER_ID);
    //console.log('signInWithGoogle', GoogleLoginProvider.PROVIDER_ID)
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID)
  }

  signOut(): void {
    this.authService.signOut();
  }
}
