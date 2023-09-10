import { BaseApiService } from '@app/service/api/base-api.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TimesCanLateAndEarlyInMonthSettingDto } from '@app/configuration/configuration.component';

@Injectable({
  providedIn: 'root'
})
export class TimesCanLateAndEarlyInMonthSettingService extends BaseApiService{

  changeUrl() {
    return 'TimesCanLateAndEarlyInMonthSetting';
  }
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  getTimesCanLateAndEarlyInMonthSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
  setTimesCanLateAndEarlyInMonthSetting(input: TimesCanLateAndEarlyInMonthSettingDto): Observable<any> {
    return this.http.post(this.rootUrl + '/Change', input);
  }
}
