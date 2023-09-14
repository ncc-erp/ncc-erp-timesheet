import { AppConsts } from '@shared/AppConsts';
import { UtilsService } from '@abp/utils/utils.service';

export class SignalRAspNetCoreHelper {
    static setupMyTimekeepingHub(): void {
        var timekeepingHub = null;

        abp.signalr.startConnection(abp.appPath + 'signalr-timekeepingHub', function (connection) {
            timekeepingHub = connection; // Save a reference to the hub
            connection.on('process', function (message) { // Register for incoming messages
                abp.event.trigger('timekeepingHub.process', message);
            });
        }).then(function (connection) {
            abp.log.debug('Connected to timekeepingHub server!');
            abp.event.trigger('timekeepingHub.connected');
        });

        abp.event.on('timekeepingHub.connected', function () { // Register for connect event
            timekeepingHub.invoke('syncData', "Hi everybody, I'm connected to the chat!"); // Send a message to the server
        });

        abp.event.on('timekeepingHub.syncData', (message) => {
            console.log("SyncData: ");
            timekeepingHub.invoke('syncData', message);
        });
    }

    static initSignalR(): void {

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
            this.setupMyTimekeepingHub();
        });
    }
}
