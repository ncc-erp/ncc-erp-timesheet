import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { BaseApiService } from './base-api.service';
import { PagedRequestDto, PageProjectUserDto } from '@shared/paged-listing-component-base';

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

  getAllUserPagging(page: PagedRequestDto, startDate: string, endDate: string): Observable<any>{
    return this.http.post(this.rootUrl + `/GetAllUserPagging?startDate=${startDate}&endDate=${endDate}`, page);
  }

  getAllValueOfUserInProjectByUserId(page: PagedRequestDto, branchId: number, startDate: string, endDate: string): Observable<any> {
    return this.http.post(this.rootUrl + `/GetStatisticNumOfUsersInProject?branchId=${branchId}&startDate=${startDate}&endDate=${endDate}`, page);
  }

  getAllUserInProject(branchId: number, projectId: number, startDate: string, endDate: string, page: PageProjectUserDto): Observable<any> {
    return this.http.post(this.rootUrl + `/GetAllUserInProject?branchId=${branchId}&projectId=${projectId}&startDate=${startDate}&endDate=${endDate}`, page);
  }
}

