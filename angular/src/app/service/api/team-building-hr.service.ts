import { ApiResponse } from './model/api-response.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
  })
  export class TeamBuildingHRService extends BaseApiService{
    constructor(http: HttpClient) {
        super(http);
      }

  changeUrl() {
    return 'TeamBuildingDetails';
  }

  getAll(request): Observable<any> {
    return this.http.post(this.rootUrl + "/GetAllPagging?", request);
  }
  getAllRequesterEmailAddressInTeamBuildingDetail(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllRequesterEmailAddressInTeamBuildingDetail");
  }
  getAllProjectInTeamBuildingDetail(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllProjectInTeamBuildingDetail");
  }
  addDataToTeamBuildingDetail(data: object): Observable<any> {
    return this.http.post(this.rootUrl + "/AddDataToTeamBuildingDetail", data);
  }
  addNew(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/AddNew', data);
  }

  getAllProjectUser(): Observable<ApiResponse<any>> {
    return this.http.get<any>(this.rootUrl + `/GetAllEmployeeTeamBuilding`);
  }


}
