

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { ApiResponse } from './model/api-response.model';
import { DisburseDto, DisburseTeamBuildingRequestInfoDto, EditRequestDto, InputGetAllDetailByRequestIdDto, InputGetUserOtherProjectDto, ResponseDetailTeamBuildingHistoryDto } from '@app/modules/team-building/const/const';


@Injectable({
    providedIn: 'root'
  })
  export class TeamBuildingRequestService extends BaseApiService{
    constructor(http: HttpClient) {
        super(http);
      }

    changeUrl() {
      return 'TeamBuildingRequestHistories';
    }

    getAllRequest(request): Observable<any> {
      return this.http.post(this.rootUrl + "/GetAllPagging?", request);
    }

    rejectRequest(requestId: number): Observable<any> {
      return this.http.post(this.rootUrl + `/RejectRequest?requestId=${requestId}` , {});
    }

    cancelRequest(requestId: number): Observable<any> {
      return this.http.post(this.rootUrl + `/CancelRequest?requestId=${requestId}` , {});
    }
    disburseRequest(request : DisburseDto): Observable<any> {
      return this.http.post(this.rootUrl + "/DisburseRequest", request);
    }
    getBillPercentageConfig(): Observable<ApiResponse<any>> {
      return this.http.get<any>(this.rootUrl + "/GetBillPercentageConfig");
    }
    getDetailOfHistory(teamBuildingHistoryId: number): Observable<ApiResponse<ResponseDetailTeamBuildingHistoryDto>> {
    return this.http.get<any>(this.rootUrl + `/GetDetailOfHistory?teamBuildingHistoryId=${teamBuildingHistoryId}` , {});
    }
    getAllRequestHistoryFileById(historyId: number): Observable<ApiResponse<any>> {
    return this.http.get<any>(this.rootUrl + `/GetAllRequestHistoryFileById?historyId=${historyId}` , {});
    }
    getAllDetailByHistoryId(request: InputGetAllDetailByRequestIdDto): Observable<ApiResponse<any>> {
      return this.http.post<any>(this.rootUrl + "/GetAllDetailByHistoryId" , request);
    }
    editRequest(request: EditRequestDto): Observable<ApiResponse<any>> {
      return this.http.post<any>(this.rootUrl + "/EditRequest" , request);
    }
    reOpenRequest(requestId: number): Observable<any> {
      return this.http.post(this.rootUrl + `/ReOpenRequest?requestId=${requestId}` , {});
    }

    getRequestMoneyInfoUserEdit(listEmpIds: number[], teamBuildingRequestId: number): Observable<any> {
      return this.http.post(this.rootUrl + `/GetRequestMoneyInfoUserEdit?teamBuildingRequestId=${teamBuildingRequestId}`, listEmpIds);
    }
    getUserNotPaggingRequestMoneyEdit(input: InputGetUserOtherProjectDto): Observable<any> {
      return this.http.post(this.rootUrl + "/GetUserNotPaggingRequestMoneyEdit", input);
    }
    getTeamBuildingRequestForDisburse(teamBuildingRequestId : number) : Observable<any> {
      let params : HttpParams = new HttpParams();
      params = params.append("teamBuildingRequestId", teamBuildingRequestId.toString());
      return this.http.get(this.rootUrl + "/GetTeamBuildingRequestForDisburse", { params : params });
    }
  }
