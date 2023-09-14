import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { UtilsService } from 'abp-ng2-module/dist/src/utils/utils.service';
import { BehaviorSubject } from 'rxjs';

@Injectable({
    providedIn: 'root'
  })

export class TimekeepingSignalRService {

    timekeepingHub = null;
    public listProcessingDays: string[] = [];
    public timekeepingProcess =  new BehaviorSubject<any>({})

    setupMyTimekeepingHub(callback : Function): void {

        const self = this;
        abp.signalr.startConnection(abp.appPath + 'signalr-timekeepingHub', function (connection) {
            console.log(connection);
            self.timekeepingHub = connection; // Save a reference to the hub

            connection.on('connectedSuccess', function (prevSyncDays: string[]) {
                self.listProcessingDays = prevSyncDays;
                self.timekeepingProcess.next({event:"connectedSuccess",data: self.listProcessingDays});
            });

            connection.on('sentRequestSuccess', function (prevSyncDays: string[]) {
                self.listProcessingDays = prevSyncDays;
                self.timekeepingProcess.next({event:"requestsuccess",data: self.listProcessingDays});
            });

            connection.on('syncDataSuccess', function (prevSyncDays: string[]) {
                self.listProcessingDays = prevSyncDays;
                self.timekeepingProcess.next({event:"syncDataSuccess",data: self.listProcessingDays});
            });

            connection.on('syncDataFailed', function (prevSyncDays: string[]) {
                self.listProcessingDays = prevSyncDays;
                self.timekeepingProcess.next({event:"syncDataFailed",data: self.listProcessingDays});
            });

        }).then(function (connection) {
            abp.log.debug('Connected to timekeepingHub server!');
            if(typeof callback === "function"){
                callback();
            }
        }).catch((err) => {
            console.log(err);
            self.timekeepingProcess.next({event:"errorOccured",data: null});
        });
    }

    invokeSyncData(date:string) {
        if(this.timekeepingHub !== null){
            return this.timekeepingHub.invoke('syncData', date);
        } else {
            return Promise.reject("Timekeeping hub is not exist.");
        }
    }

    initSignalR(callback): void {

        const encryptedAuthToken = new UtilsService().getCookieValue(AppConsts.authorization.encrptedAuthTokenName);

        abp.signalr = {
            autoConnect: true,
            connect: undefined,
            hubs: undefined,
            qs: AppConsts.authorization.encrptedAuthTokenName + '=' + encodeURIComponent(encryptedAuthToken),
            remoteServiceBaseUrl: AppConsts.remoteServiceBaseUrl,
            startConnection: undefined,
            url: '/signalr'
        };

        jQuery.getScript(AppConsts.appBaseUrl + '/assets/abp/abp.signalr-client.js', () => {
            this.setupMyTimekeepingHub(callback);
        });
    }
}
