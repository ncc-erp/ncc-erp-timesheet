import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { AppConsts } from '@shared/AppConsts';

export interface PMReportPunishSettingDto {
  enable: boolean;
  hour: number;
  dayofweek: string;
}

@Injectable({
  providedIn: 'root'
})
export class PMReportPunishSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetPMReportPunishSetting');
  }

  change(input: PMReportPunishSettingDto): Observable<PMReportPunishSettingDto> {
    return this.http.post<PMReportPunishSettingDto>(this.rootUrl + '/SetPMReportPunishSetting', input);
  }

  triggerManualPunishment(): Observable<any> {
    return this.http.post( AppConsts.remoteServiceBaseUrl + '/api/services/app/UserPunishment/ApplyPMReportPunishmentsAsync', {});
  }
}
