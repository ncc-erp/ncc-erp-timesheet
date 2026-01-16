import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimesheetsSupervisiorService extends BaseApiService{

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'TimesheetsSupervisor';
  }

  getAll(startDate: string, endDate: string, status: number, projectId: number, userId: number, OpenTalkJoinTime : number, OpenTalkJoinTimeType: boolean, overtimeType: number): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", startDate);
    params = params.append("endDate", endDate);
    params = params.append("status", status.toString());
    params = params.append("projectID", this.getPara(projectId));
    params = params.append("userId", this.getPara(userId));
    params = params.append("opentalkTime", this.getPara(OpenTalkJoinTime));
    params = params.append("opentalkTimeType", this.getPara(OpenTalkJoinTimeType));
    params = params.append("overtimeType", this.addOvertimeTypeParam(overtimeType));
    return this.http.get(this.getUrl("GetAll"), { params : params });
    //return this.http.get(this.getUrl(`GetAll?startDate=${startDate}&endDate=${endDate}&status=${status}&projectID=${this.getPara(projectId)}&userId=${this.getPara(userId)}`));
  }

  private addOvertimeTypeParam(overtimeType: number) {
    if (overtimeType === 1) {
      return "true";
    } else if (overtimeType === 0) {
      return "false";
    }
    return "";
  }

  private getPara(value){
    if(value <=0 || value == void 0) return '';
    return value
  }

  GetQuantityTimesheetSupervisorStatus(startDate: string, endDate: string, projectId: number, userId: number, OpenTalkJoinTime : number, OpenTalkJoinTimeType: boolean, overtimeType: number): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", startDate);
    params = params.append("endDate", endDate);
    params = params.append("projectID", this.getPara(projectId));
    params = params.append("userId", this.getPara(userId));
    params = params.append("opentalkTime", this.getPara(OpenTalkJoinTime));
    params = params.append("opentalkTimeType", this.getPara(OpenTalkJoinTimeType));
    params = params.append("overtimeType", this.addOvertimeTypeParam(overtimeType));
    return this.http.get(this.getUrl("GetQuantityTimesheetSupervisorStatus"), { params : params });
    //return this.http.get(this.getUrl(`GetQuantityTimesheetSupervisorStatus?startDate=${startDate}&endDate=${endDate}`));
  }

}
