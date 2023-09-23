import { Observable } from 'rxjs';
import { BaseApiService } from '@app/service/api/base-api.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class WfhSettingService extends BaseApiService{

  changeUrl() {
    return 'WFHSetting';
  }
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
  change(input: Object): Observable<any> {
    return this.http.post(this.rootUrl + '/Change', input);
  }
}
