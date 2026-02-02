import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

export interface PMOtherPunishSettingDto {
  enable: boolean;
  hour: number;
  dayOfMonth: number;
  adminClanName: string;
}

@Injectable({
  providedIn: 'root'
})
export class PMOtherPunishSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return 'Configuration';
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetPMOtherPunishSetting');
  }

  change(input: PMOtherPunishSettingDto): Observable<PMOtherPunishSettingDto> {
    return this.http.post<PMOtherPunishSettingDto>(this.rootUrl + '/SetPMOtherPunishSetting', input);
  }
}