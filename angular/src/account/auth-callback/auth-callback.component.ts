
import { Component, Injector, OnInit } from '@angular/core';
import { ActivatedRoute} from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { LoginService } from 'account/login/login.service';
import { MessageService } from '@abp/message/message.service';

@Component({
  selector: 'auth-callback',
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent extends AppComponentBase implements OnInit {
  isLoading: boolean = false;
  message: MessageService;
  constructor(
    injector: Injector,
    private route: ActivatedRoute,    
    public loginService: LoginService
  ) {
    super(injector)
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.isLoading = true;

      const code = params['code'];
      const scope = params['scope'];
      const state = params['state']
      if (code && scope && state) {
        this.message.error(this.l('something went wrong!'))
      }
      this.loginService.authenticateMezon(code, scope).subscribe(res => {
        this.isLoading = res.loading;
      });
    });
  }
}
