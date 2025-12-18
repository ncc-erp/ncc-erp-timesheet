import { Component, Injector, OnInit } from "@angular/core";
import { AbpSessionService } from "@abp/session/abp-session.service";
import { AppComponentBase } from "@shared/app-component-base";
import { accountModuleAnimation } from "@shared/animations/routerTransition";
import { LoginService } from "./login.service";
import { AuthService, SocialUser } from "angularx-social-login";
import { GoogleLoginProvider } from "angularx-social-login";
import { AppConsts } from "@shared/AppConsts";
import { MezonLoginService } from "@app/service/api/mezon-api.service";
import { ActivatedRoute } from "@angular/router";
import { IHashMezonAuthModel } from "@shared/service-proxies/service-proxies";
import { Base64 } from "js-base64";

@Component({
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.less"],
  animations: [accountModuleAnimation()],
})
export class LoginComponent extends AppComponentBase implements OnInit {
  submitting = false;
  nccCode: string;
  isShowPassword = true;
  enableNormalLogin: boolean;

  hashData: string;
  isMezonApp: boolean = false;

  constructor(
    injector: Injector,
    public loginService: LoginService,
    private _sessionService: AbpSessionService,
    private authService: AuthService,
    private mezonLoginService: MezonLoginService,
    private route: ActivatedRoute
  ) {
    super(injector);
  }

  ngOnInit() {
    this.enableNormalLogin = AppConsts.enableNormalLogin;

    // Disable Auto Login By Google
    this.authService.authState.subscribe(
      (user) => {},
      (err) => this.authService.signOut()
    );

    this.route.queryParams.subscribe((params) => {
      const hashFromUrl = params["data"];
      if (hashFromUrl) {
        this.isMezonApp = true;
        this.hashData = hashFromUrl;
        const hashAuthData: IHashMezonAuthModel = {
          hashData: Base64.encode(hashFromUrl),
        };
        console.log("Mezon hash data from URL: ", hashFromUrl);
        this.loginService.authenticateMezonHash(hashAuthData, () => {
          this.isMezonApp = true;
        });
      }
    });
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
