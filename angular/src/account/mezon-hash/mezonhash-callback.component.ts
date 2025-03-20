import { Component, Injector, OnInit } from '@angular/core';
import { MezonWebViewService } from '@app/service/api/mezon-webview-service';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { IHashMezonAuthModel } from '@shared/service-proxies/service-proxies';
import { LoginService } from '../login/login.service';
import { Base64 } from '@node_modules/js-base64/base64';

@Component({
  selector: 'mezonhash-callback',
  templateUrl: './mezonhash-callback.component.html',
  styleUrls: ['./mezonhash-callback.component.css']
})
export class MezonhashCallbackComponent extends AppComponentBase implements OnInit {
  currentUser: any;
  userHash: any;

  hashData: string;
  isMezonApp: boolean = false;
  isAuthenFailed: boolean = false;
  isAuthenticating: boolean = false;
  isLoading: boolean = false;

  constructor(
    injector: Injector,
    private mezonWebViewService: MezonWebViewService,
    private route: ActivatedRoute,
    public loginService: LoginService,


  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.mezonWebViewService.ping();
    this.mezonWebViewService.sendBotId();
    this.mezonWebViewService.listenToPong();
    this.mezonWebViewService.listenToUserHashInfo();


    this.mezonWebViewService.isInMezon$.subscribe((status) => {
      this.isMezonApp = status;
    });

    this.mezonWebViewService.userHashData$.subscribe((userHashData) => {
      this.hashData = userHashData;
      this.isAuthenticating = true;
      this.signInWithHash(userHashData);
    });
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


  // Hàm logout
  logout() {
    this.mezonWebViewService.logout();
  }

}
