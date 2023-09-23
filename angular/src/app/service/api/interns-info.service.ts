import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InternsInfoService extends BaseApiService {

  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'InternsInfo';
  }

  getInternInfo(request: any): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/GetAll', request);
}
  getAllBasicTraner(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetAllBasicTraner');
  }
  getAllBranch(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/GetAllBranch');
  }
}
