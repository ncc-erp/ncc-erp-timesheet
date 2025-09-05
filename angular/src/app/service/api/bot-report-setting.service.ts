import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

export interface BotReportSettingDto {
  enable: boolean;
  everyday: boolean;
  hour: number;
  dayofweek: string;
  botUri: string;
  officeId: number;
  minHours: number;
  topN: number;
}

@Injectable({
  providedIn: 'root'
})
export class BotReportSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetBotReportSetting');
  }

  change(input: BotReportSettingDto): Observable<BotReportSettingDto> {
    return this.http.post<BotReportSettingDto>(this.rootUrl + '/SetBotReportSetting', input);
  }
}
