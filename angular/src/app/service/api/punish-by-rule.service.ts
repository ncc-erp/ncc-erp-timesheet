import { BaseApiService } from '@app/service/api/base-api.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CheckInCheckOutPunishmentSettingService extends BaseApiService{

  changeUrl() {
    return 'CheckInCheckOutPunishmentSetting';
  }
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  getCheckInCheckOutPunishmentSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetCheckInCheckOutPunishmentSetting');
  }
  setCheckInCheckOutPunishmentSetting(input: Object): Observable<any> {
    return this.http.post(this.rootUrl + '/SetCheckInCheckOutPunishmentSetting', input);
  }
  setPercentOfTrackerOnWorkingSetting(input: string): Observable<any> {
    return this.http.post(this.rootUrl + '/SetPercentOfTrackerOnWorkingSetting?' + `input=${input}`, undefined);
  }
}