import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { AppConsts } from '@shared/AppConsts';

export interface LateInternReviewSettingDto {
  enable: boolean;
  deadlineDay: number;
  startDayOfMonth: number;
  nextRunDate: number;
}

@Injectable({
  providedIn: 'root'
})
export class LateInternReviewSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetLateInternReviewSetting');
  }

  change(input: LateInternReviewSettingDto): Observable<LateInternReviewSettingDto> {
    return this.http.post<LateInternReviewSettingDto>(this.rootUrl + '/SetLateInternReviewSetting', input);
  }

  triggerManualPunishment(month: number, year: number): Observable<any> {
    return this.http.post( AppConsts.remoteServiceBaseUrl + '/api/services/app/ReviewIntern/CheckAndPunishLateReview', { month, year }
    );
  }
}