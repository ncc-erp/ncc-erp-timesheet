import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { BaseApiService } from './base-api.service';
import { PagedRequestDto } from '@shared/paged-listing-component-base';

@Injectable({
  providedIn: 'root'
})
export class ManageUserForBranchService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(){
    return 'ManageUserForBranch';
  }

  getAllUserPagging(page: PagedRequestDto, startDate?: Date, endDate?: Date): Observable<any>{
    return this.http.post(this.rootUrl + `/GetAllUserPagging?startDate=${startDate}&endDate=${endDate}`, page);
  }

  getAllValueOfUserInProjectByUserId(page: PagedRequestDto, branchId: number): Observable<any> {
    return this.http.post(this.rootUrl + `/GetStatisticNumOfUsersInProject?branchId=${branchId}`, page);
  }
}
