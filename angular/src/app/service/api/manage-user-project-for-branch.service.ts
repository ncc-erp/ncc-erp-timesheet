import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ManageUserProjectForBranchService extends BaseApiService {
  changeUrl() {
    return 'ManageUserProjectForBranch';
  }
  constructor(http: HttpClient) {
    super(http);
  }

  getAllValueOfUserInProjectByUserId(userId: number, fromDate, toDate): Observable<any>{
    return this.http.get(this.rootUrl + `/GetAllValueOfUserInProjectByUserId?userId=${userId}&startDate=${fromDate}&endDate=${toDate}`);
  }

  createValueOfUser(data: any): Observable<any>{
    return this.http.post(this.rootUrl + '/CreateValueOfUser', data);
  }
}
