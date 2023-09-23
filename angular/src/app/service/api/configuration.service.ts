import { HRMConfigDto, KomuDto, ProjectConfigDto, NRITConfigDto, UnlockTimesheetConfigDto, RetroNotifyConfigDto, TeamBuildingConfigDto, ApproveTimesheetNotifyConfigDto, ApproveRequestOffNotifyConfigDto, SendMessageRequestPendingTeamBuildingToHRConfigDto, NotifyHRTheEmployeeMayHaveLeftConfigDto,MoneyPMUnlockTimeSheetConfigDto, SendMessageToPunishUserConfigDto } from './../../configuration/configuration.component';
import { HttpClient } from '@angular/common/http';
import { BaseApiService } from '@app/service/api/base-api.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService extends BaseApiService {
  changeUrl() {
    return "Configuration";
  }

  constructor(http: HttpClient) {
    super(http)
  }
  getAll(): Observable<any> {
    return this.http.get(this.rootUrl + "/Get");
  }
  change(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/Change', data);
  }
  GetWorkingTimeConfigAllBranch(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetWorkingTimeConfigAllBranch");
  }
  GetHRMConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetHRMConfig");
  }
  SetHRMConfig(config: HRMConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetHRMConfig", config);

  }
  SetProjectConfig(config: ProjectConfigDto): Observable<any>{
    return this.http.post(this.rootUrl + "/SetProjectConfig", config);
  }
  GetProjectConfig(): Observable<any>{
    return this.http.get(this.rootUrl + "/GetProjectConfig");
  }
  GetKomuConfig(): Observable<any>{
    return this.http.get<any>(this.rootUrl + '/GetKomuConfig');
  }
  SetKomuConfig(config: KomuDto): Observable<any>{
    return this.http.post(this.rootUrl + "/SetKomuConfig", config);
  }
  GetNotificationSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetNotificationSetting');
  }
  SetNotificationSetting(data : object) : Observable<any>{
    return this.http.post(this.rootUrl+ '/SetNotificationSetting', data);
   }

  GetNRITConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetNRITConfig");
  }
  SetNRITConfig(config: NRITConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetNRITConfig", config);
  }
  GetUnlockTimesheetConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetUnlockTimesheetConfig");
  }
  SetUnlockTimesheetConfig(config: UnlockTimesheetConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetUnlockTimesheetConfig", config);
  }

  checkConnectToProject(): Observable<any>{
    return this.http.post(this.rootUrl + '/CheckConnectToProject', {});
  }

  checkConnectToHRM(): Observable<any>{
    return this.http.post(this.rootUrl + '/CheckConnectToHRM', {});
  }

  getRetroNotifyConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetRetroNotifyConfig");
  }

  setRetroNotifyConfig(config: RetroNotifyConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetRetroNotifyConfig", config);
  }
  GetTeamBuildingConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetTeamBuildingConfig");
  }
  SetTeamBuildingConfig(config: TeamBuildingConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetTeamBuildingConfig", config);
  }
  getApproveTimesheetNotifyConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetApproveTimesheetNotifyConfig");
  }
  setApproveTimesheetNotifyConfig(config: ApproveTimesheetNotifyConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetApproveTimesheetNotifyConfig", config);
  }
  getApproveRequestOffNotifyConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetApproveRequestOffNotifyConfig");
  }
  setApproveRequestOffNotifyConfig(config: ApproveRequestOffNotifyConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetApproveRequestOffNotifyConfig", config);
  }
  getSendMessageRequestPendingTeamBuildingToHRConfig(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetSendMessageRequestPendingTeamBuildingToHRConfig");
  }
  setSendMessageRequestPendingTeamBuildingToHRConfig(config: SendMessageRequestPendingTeamBuildingToHRConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetSendMessageRequestPendingTeamBuildingToHRConfig", config);
  }
  getConfigNotifyHRTheEmployeeMayHaveLeft(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetConfigNotifyHRTheEmployeeMayHaveLeft");
  }
  setConfigNotifyHRTheEmployeeMayHaveLeft(config: NotifyHRTheEmployeeMayHaveLeftConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetConfigNotifyHRTheEmployeeMayHaveLeft", config);
  }
  getConfigMoneyPMUnlockTimeSheet(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetConfigMoneyPMUnlockTimeSheet");
  }
  setConfigMoneyPMUnlockTimeSheet(config: MoneyPMUnlockTimeSheetConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetConfigMoneyPMUnlockTimeSheet", config);
  }
  getConfigSendMessageToPunishUser(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetConfigSendMessageToPunishUser");
  }
  setConfigSendMessageToPunishUser(config: SendMessageToPunishUserConfigDto): Observable<any> {
    return this.http.post(this.rootUrl + "/SetConfigSendMessageToPunishUser", config);
  }
}

