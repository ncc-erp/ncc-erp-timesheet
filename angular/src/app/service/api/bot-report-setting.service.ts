import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

export interface BotReportSettingDto {
  enable: boolean;
  everyday: boolean;
  hour: number;
  dayofweek: string;
  botUri: string;
  branchCodes?: string[];
  minHours: number;
  topN: number;
  projectIds?: number[];
  projectNames?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class BotReportSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration'; // Thay đổi để khớp với ConfigurationAppService
  }

  getAnomaliesSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetBotReportSetting'); // Gọi GetBotReportSetting
  }

  updateAnomaliesSetting(setting: BotReportSettingDto): Observable<any> {
    // Sử dụng POST thay vì GET để gửi dữ liệu (phù hợp hơn với SetBotReportSetting)
    return this.http.post<any>(this.rootUrl + '/SetBotReportSetting', setting);
  }
}