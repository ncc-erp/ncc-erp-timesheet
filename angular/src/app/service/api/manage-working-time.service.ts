import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { BaseApiService } from "./base-api.service";

@Injectable({
  providedIn: "root",
})
export class ManageWorkingTimeService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return "ManageWorkingTime";
  }

  getAll(userName, projectIds, status?: Number): Observable<any> {
    if(status === 0) {
      status = null;
    }
      let data = {
        userName,
        status,
        projectIds
      }
      return this.http.post<any>(this.rootUrl + "/GetAll", data);

  }

  updateApproveWorkingTime(id: any): Observable<any> {
    return this.http.post(this.rootUrl + `/ApproveWorkingTime?id=${id}`, {});
  }

  updateRejectWorkingTime(id: any): Observable<any> {
    return this.http.post(this.rootUrl + `/RejectWorkingTime?id=${id}`, {});
  }
}
