import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BranchService extends BaseApiService{
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Branch';
  }
  
  getAll(request): Observable<any> {
    return this.http.post(this.rootUrl + "/GetAllPagging?", request);
  }
  
  getAllBranchFilter(isAll: boolean): Observable<any> {
    return this.http.get(this.rootUrl + `/GetAllBranchFilter?isAll=${isAll}`);
  } 
  GetAllNotPagging(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllNotPagging");
  }
}
