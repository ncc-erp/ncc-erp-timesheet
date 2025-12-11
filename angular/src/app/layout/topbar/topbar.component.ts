import { Component, Injector, ViewEncapsulation, HostListener } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';
import { AuthService } from 'angularx-social-login';

@Component({
    templateUrl: './topbar.component.html',
    selector: 'top-bar',
    styleUrls: ['./topbar.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class TopBarComponent extends AppComponentBase {
    isUserMenuOpen = false;

    constructor(
        injector: Injector,
        private _authService: AppAuthService,
        private googleAuthService: AuthService,
    ) {
        super(injector);
    }

    toggleUserMenu(): void {
        this.isUserMenuOpen = !this.isUserMenuOpen;
    }

    logout(): void {
        this.isUserMenuOpen = false;
        this._authService.logout();
        this.googleAuthService.signOut();
    }

    @HostListener('document:click', ['$event'])
    clickOutside(event: Event): void {
        const target = event.target as HTMLElement;
        if (!target.closest('.topbar__user-wrapper')) {
            this.isUserMenuOpen = false;
        }
    }
}