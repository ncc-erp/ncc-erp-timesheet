import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BaseApiService } from './base-api.service';

export interface AnomaliesReportSettingDto {
  enable: boolean;
  hour: number;
  minute: number;
  dayofweek: string;
  botUri: string;
  branchCodes?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AnomaliesReportSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  getAnomaliesReportSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetAnomaliesReportSetting');
  }

  setAnomaliesReportSetting(input: AnomaliesReportSettingDto): Observable<AnomaliesReportSettingDto | { success: boolean; error: any }> {
    return this.http.post<AnomaliesReportSettingDto>(this.rootUrl + '/SetAnomaliesReportSetting', input)
      .pipe(
        catchError(error => {
          console.error('Error setting anomalies report configuration:', error);
          return of({ success: false, error: error.message || 'An error occurred' });
        })
      );
  }
}