import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { KomuPunishCheckInDto } from '@app/configuration/configuration.component';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class SendKomuPunishedCheckInService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl() {
    return 'SendKomuPunishedUserCheckInSetting';
  }
  getPunishedCheckInConfig(): Observable<any>{
    return this.http.get(this.rootUrl + "/GetPunishedCheckInConfig");
  }

  changePunishedCheckInConfig(data: KomuPunishCheckInDto): Observable<any>{
    return this.http.post(this.rootUrl+"/ChangePunishedCheckInConfig", data);
  }
}
