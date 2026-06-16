import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { GetTimesheetsInputDto } from './model/get-timesheets-input-dto';

@Injectable({
  providedIn: 'root'
})
export class TimesheetService extends BaseApiService {

  submitTimesheet(timesheetId: any): any {
    return this.http.post(this.getUrl('SubmitTimesheet?timesheetId=' + timesheetId), {});
  }

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Timesheet';
  }

  getAllTimesheets(input: GetTimesheetsInputDto): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", input.startDate);
    params = params.append("endDate", input.endDate);
    params = params.append("status", input.status.toString());
    params = params.append("projectId", this.getPara(input.projectId));
    params = params.append("checkInFilter", this.getPara(input.checkInFilter));
    params = params.append("searchText", input.searchText);
    params = params.append("branchId", this.getPara(input.branchId));
    params = params.append("opentalkTime", this.getPara(input.opentalkTime));
    params = params.append("workLocation", this.getPara(input.workLocation));
    
    const typeOfWorkParam = input.typeOfWork >= 0 ? input.typeOfWork.toString() : "";
    params = params.append("typeOfWork", typeOfWorkParam);

    const isChargedParam = input.isCharged === 1 ? "true" : (input.isCharged === 0 ? "false" : "");
    params = params.append("isCharged", isChargedParam);

    const opentalkTimeTypeParam = input.opentalkTimeType === 1 ? "true" : (input.opentalkTimeType === 0 ? "false" : "");
    params = params.append("opentalkTimeType", opentalkTimeTypeParam);
    return this.http.get(this.getUrl("GetAll"), { params : params });
    // return this.http.get(this.getUrl(`GetAll?startDate=${startDate}&endDate=${endDate}&status=${status}&projectId=${this.getPara(projectId)}&checkInFilter=${this.getPara(checkInFilter)}&searchText=${searchText}&branchId=${this.getPara(branchId)}`));
  }

  private getPara(value){
    if(value <=0 || value == void 0) return '';
    return value
  }
  approveTimesheet(listId) {
    return this.http.post(this.rootUrl + '/ApproveTimesheets', listId );
  }

  rejectTimesheet(listId){
    return this.http.post(this.rootUrl + '/RejectTimesheets', listId );
  }

  unlockTimesheet(body){
    return this.http.post(this.rootUrl + '/UnlockTimesheet', body);
  }

  lockTimesheet(body){
    return this.http.post(this.rootUrl + '/LockTimesheet', body);
  }

  createOrGetMyTimesheet() {
    return this.http.post(this.getUrl('CreateMyTimesheet'), {});
  }

  changeStatusService(id: number, statusCode: number): Observable<any> {
    return this.http.post(this.getUrl('ChangeStatusTimesheet?id=' + id), statusCode);
  }

  getMyTimesheet(status: any): Observable<any> {
    return this.http.get(this.getUrl('GetMyTimesheet?Status=' + status));
  }

  getPendingTimesheet(): Observable<any> {
    return this.http.get(this.getUrl('getPendingTimesheet'));
  }

  getTimesheetReport(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetReport?fromDate=${fromDate}&toDate=${toDate}`));
  }

  getProjectList(fromDate, toDate): Observable<any>{
    return this.http.get(this.getUrl(`GetProjectList?fromDate=${fromDate}&toDate=${toDate}`));
  }


  getTaskList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTaskList?fromDate=${fromDate}&toDate=${toDate}`));
  }

  getTeamList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTeamList?fromDate=${fromDate}&toDate=${toDate}`));
  }

  getCustomerList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetCustomerList?fromDate=${fromDate}&toDate=${toDate}`));
  }

  checkAdmin():Observable<any> {
    return this.http.get(this.getUrl(`CheckAdmin`));
  }
  getAllTimeSheetOrRemote(day, type): Observable<any>{
    return this.http.get(this.getUrl(`GetAllTimeSheetOrRemote?day=${day}&type=${type}`));
  }
  getQuantiyTimesheetStatus(input: GetTimesheetsInputDto): Observable<any> {
    let params : HttpParams = new HttpParams();
    params = params.append("startDate", input.startDate);
    params = params.append("endDate", input.endDate);
    params = params.append("projectId", this.getPara(input.projectId));
    params = params.append("checkInFilter", this.getPara(input.checkInFilter));
    params = params.append("searchText", input.searchText);
    params = params.append("branchId", input.branchId.toString());
    params = params.append("opentalkTime", this.getPara(input.opentalkTime));
    params = params.append("workLocation", this.getPara(input.workLocation));
    
    const typeOfWorkParam = input.typeOfWork >= 0 ? input.typeOfWork.toString() : "";
    params = params.append("typeOfWork", typeOfWorkParam);

    const isChargedParam = input.isCharged === 1 ? "true" : (input.isCharged === 0 ? "false" : "");
    params = params.append("isCharged", isChargedParam);

    const opentalkTimeTypeParam = input.opentalkTimeType === 1 ? "true" : (input.opentalkTimeType === 0 ? "false" : "");
    params = params.append("opentalkTimeType", opentalkTimeTypeParam);
    return this.http.get(this.getUrl("GetQuantiyTimesheetStatus"), { params : params });
  }

  getTimesheetWarning(myTimesheetIds) {
    return this.http.post(this.rootUrl + '/GetTimesheetWarning', myTimesheetIds );
  }
}
