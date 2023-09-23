import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class LevelSettingService extends BaseApiService{

  changeUrl() {
    return 'LevelSetting';
  }
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
  set(input: Object): Observable<any> {
    return this.http.post(this.rootUrl + '/Set', input);
  }

}
