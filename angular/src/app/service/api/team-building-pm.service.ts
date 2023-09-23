import { AppConsts } from './../../../shared/AppConsts';
import { ApiResponse } from "./model/api-response.model";
import { HttpClient, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseApiService } from "./base-api.service";
import { Observable } from "rxjs";
import { InputGetUserOtherProjectDto, PMRequestDto } from '@app/modules/team-building/const/const';

@Injectable({
  providedIn: "root",
})
export class TeamBuildingPMService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return "TeamBuildingDetailsPM";
  }

  getAllRequest(projectId?: number, month?: number, branchId?: number): Observable<any> {
    let url = this.rootUrl + '/GetRequestMoney';
    let arr: string [] = [];

    if(projectId) {
      arr.push(`projectId=${projectId}`);
    }
    if(branchId) {
      arr.push(`branchId=${branchId}`);
    }
    if(month) {
      arr.push(`month=${month}`);
    }
    if(arr.length > 0){
      url += '?' + arr.join('&');
    }

    return this.http.get(url);
  }

  getAll(request): Observable<ApiResponse<any>> {
    return this.http.post<any>(this.rootUrl + "/GetAllPagging?", request);
  }
  getAllRequesterEmailAddressInTeamBuildingDetailPM(): Observable<any> {
    return this.http.get(
      this.rootUrl + "/GetAllRequesterEmailAddressInTeamBuildingDetailPM"
    );
  }

  addDataToTeamBuildingDetail(data: PMRequestDto, fileArray : Map<number, File>): Observable<any> {
    const formData = new FormData();
    
    formData.append("PMRequest", JSON.stringify(data));

    fileArray.forEach((value, key) => {
      formData.append("listFile", value, key.toString() + "." + value.name.split(".")[1]);
    })

    const uploadReq = new HttpRequest(
      'POST', AppConsts.remoteServiceBaseUrl + '/api/services/app/TeamBuildingDetailsPM/SubmitRequestMoney', formData,
      {
          reportProgress: true,
      }
    );
    return this.http.request(uploadReq)
  }
  
  getAllProjectInTeamBuildingDetailPM(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllProjectInTeamBuildingDetailPM");
  }

  getAllRequestByBranch(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllBranchTeamBuildingFilter");
  }

  getAllUser(input: InputGetUserOtherProjectDto): Observable<any> {
    return this.http.post(this.rootUrl + "/GetUserNotPaggingRequestMoney", input);
  }

  GetRequestMoneyInfoUser(listEmpIds: number[]): Observable<any> {
    return this.http.post(this.rootUrl + `/GetRequestMoneyInfoUser`, listEmpIds);
  }

}
