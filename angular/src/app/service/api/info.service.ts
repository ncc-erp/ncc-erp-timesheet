import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InfoService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Info';
  }

  unlockToLogTimesheet(emailAddress: string): Observable<any> {
    const params = new HttpParams().set('emailAddress', emailAddress);
    return this.http.post(this.rootUrl + '/UnlockToLogTimesheet', null, { params });
  }

  unlockToApproveTimesheet(emailAddress: string): Observable<any> {
    const params = new HttpParams().set('emailAddress', emailAddress);
    return this.http.post(this.rootUrl + '/UnlockToApproveTimesheet', null, { params });
  }
}
