import { BaseApiService } from './base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AutoLockTimesheetService extends BaseApiService {
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'AutoLockTimesheetSetting';
  }
  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
  change(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/Change', data);
  }

}
