import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { GetTimesheetsInputDto } from './model/get-timesheets-input-dto';

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

  getAll(input: GetTimesheetsInputDto): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", input.startDate);
    params = params.append("endDate", input.endDate);
    params = params.append("status", input.status.toString());
    params = params.append("projectID", this.getPara(input.projectId));
    params = params.append("userId", this.getPara(input.userId));
    params = params.append("opentalkTime", this.getPara(input.opentalkTime));
    params = params.append("opentalkTimeType", this.getPara(input.opentalkTimeType));

    const typeOfWorkParam = input.typeOfWork >= 0 ? input.typeOfWork.toString() : "";
    params = params.append("typeOfWork", typeOfWorkParam);

    const isChargedParam = input.isCharged === 1 ? "true" : (input.isCharged === 0 ? "false" : "");
    params = params.append("isCharged", isChargedParam);
    return this.http.get(this.getUrl("GetAll"), { params : params });
    //return this.http.get(this.getUrl(`GetAll?startDate=${startDate}&endDate=${endDate}&status=${status}&projectID=${this.getPara(projectId)}&userId=${this.getPara(userId)}`));
  }

  private getPara(value){
    if(value <=0 || value == void 0) return '';
    return value
  }

  GetQuantityTimesheetSupervisorStatus(input: GetTimesheetsInputDto): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", input.startDate);
    params = params.append("endDate", input.endDate);
    params = params.append("projectID", this.getPara(input.projectId));
    params = params.append("userId", this.getPara(input.userId));
    params = params.append("opentalkTime", this.getPara(input.opentalkTime));
    params = params.append("opentalkTimeType", this.getPara(input.opentalkTimeType));

    const typeOfWorkParam = input.typeOfWork >= 0 ? input.typeOfWork.toString() : "";
    params = params.append("typeOfWork", typeOfWorkParam);

    const isChargedParam = input.isCharged === 1 ? "true" : (input.isCharged === 0 ? "false" : "");
    params = params.append("isCharged", isChargedParam);
    return this.http.get(this.getUrl("GetQuantityTimesheetSupervisorStatus"), { params : params });
    //return this.http.get(this.getUrl(`GetQuantityTimesheetSupervisorStatus?startDate=${startDate}&endDate=${endDate}`));
  }

}
