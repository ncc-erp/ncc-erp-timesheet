import { Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { AuthService } from 'angularx-social-login';

@Component({
    templateUrl: './sidebar-footer.component.html',
    selector: 'sidebar-footer',
    encapsulation: ViewEncapsulation.None
})
export class SideBarFooterComponent extends AppComponentBase {

    versionText: string;
    currentYear: number;

    constructor(
        injector: Injector,
        private _authService: AppAuthService,
        private googleAuthService: AuthService,
    ) {
        super(injector);

        this.currentYear = new Date().getFullYear();
        this.versionText = this.appSession.application.version + ' [' + this.appSession.application.releaseDate.format('YYYYDDMM') + ']';
    }

    
    logout(): void {
        this._authService.logout();
        this.googleAuthService.signOut();
    }
}
