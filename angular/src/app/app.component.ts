import { Component, ViewContainerRef, Injector, OnInit, AfterViewInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { STORAGE_KEYS } from '@app/constant/storage-keys.constant';

import { SignalRAspNetCoreHelper } from '@shared/helpers/SignalRAspNetCoreHelper';

@Component({
    templateUrl: './app.component.html'
})
export class AppComponent extends AppComponentBase implements OnInit, AfterViewInit {

    private viewContainerRef: ViewContainerRef;

    constructor(
        injector: Injector,
        private ngZone: NgZone
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.checkZkProof();
        //SignalRAspNetCoreHelper.initSignalR();

        abp.event.on('abp.notifications.received', userNotification => {
            abp.notifications.showUiNotifyForUserNotification(userNotification);

            // Desktop notification
            Push.create('AbpZeroTemplate', {
                body: userNotification.notification.data.message,
                icon: abp.appPath + 'assets/app-logo-small.png',
                timeout: 6000,
                onClick: function () {
                    window.focus();
                    this.close();
                }
            });
        });
    }

    private checkZkProof(): void {
        const zkProof = localStorage.getItem(STORAGE_KEYS.ZK_PROOF);
        if (!zkProof || zkProof === 'undefined') {
            this.redirectToLogin();
        }
    }

    private redirectToLogin(): void {
        try {
            localStorage.removeItem(STORAGE_KEYS.ZK_PROOF);
            localStorage.removeItem(STORAGE_KEYS.KEY_PAIR);
            localStorage.removeItem(STORAGE_KEYS.MEZON_USER_ID);
            localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
        } catch (e) {
            console.error('Error clearing localStorage during redirect to login:', e);
        }

        window.location.href = '/account/login';
    }

    ngAfterViewInit(): void {
        $.AdminBSB.activateAll();
        $.AdminBSB.activateDemo();
    }

    onResize(event) {
        this.ngZone.runOutsideAngular(() => {
            // exported from $.AdminBSB.activateAll
            $.AdminBSB.leftSideBar.setMenuHeight();
            $.AdminBSB.leftSideBar.checkStatuForResize(false);

            // exported from $.AdminBSB.activateDemo
            $.AdminBSB.demo.setSkinListHeightAndScroll();
            $.AdminBSB.demo.setSettingListHeightAndScroll();
        })
    }
}
