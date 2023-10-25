import { AbpMultiTenancyService } from '@abp/multi-tenancy/abp-multi-tenancy.service';
import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import * as Sentry from "@sentry/browser";

@Injectable()
export class SentryService {

    private isInit: boolean = false;

    init() {
        if(AppConsts.sentryDsn && !this.isInit){
            Sentry.init({
                dsn: AppConsts.sentryDsn,
                environment: window.location.hostname,
                beforeSend(event, hint){
                    const originEx = hint.originalException;
                    if(typeof(originEx) === "object" && "isCustomError" in originEx && originEx["isCustomError"]){
                        return null;
                    }
                    return event;
                },
            });
            this.isInit = true;
        }
    }

    postErrorLog(message: string, extraData?: Object) {
        if (this.isInit) {
            Sentry.withScope((scope) => {
                if(extraData){
                    scope.setExtras(extraData);
                }
              Sentry.captureMessage(message);
            });
          }
    }

    setUserInfo(user: Sentry.User){
        Sentry.setUser(user);
    }
}
