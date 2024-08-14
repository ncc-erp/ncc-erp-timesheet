import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class MezonSettingService extends BaseApiService {

  changeUrl() {
    return 'MezonSetting';
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
