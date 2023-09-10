import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OvertimeSettingService extends BaseApiService{
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'OvertimeSetting';
  }
  
  getAll(request, dateAt, projectId): Observable<any> {
    return this.http.post(this.rootUrl + "/GetAllPagging?" + `dateAt=${dateAt}&projectId=${projectId < 1 ? '' : projectId}`, request);
  }
}
