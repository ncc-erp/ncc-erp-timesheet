import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { WarningMyTimesheetDto } from './model/mytimesheet-dto';
import { ApiResponse } from './model/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class MyTimesheetService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'MyTimesheets';
  }

  // createOrGetMyTimesheet() {
  //   return this.http.post(this.getUrl('GetAllTimeSheetOfUser'), {});
  // }
  getAllTimeSheet(startDate, endDate): Observable<any> {
    return this.http.get(this.getUrl(`GetAllTimeSheetOfUser?startDate=${startDate}&endDate=${endDate}`))
  }

  getTimesheetReport(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetReportHours?startDate=${fromDate}&endDate=${toDate}`));
  }

  getProjectList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetStatisticProjects?startDate=${fromDate}&endDate=${toDate}`));
  }

  getTaskList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetStatisticTasks?startDate=${fromDate}&endDate=${toDate}`));
  }

  getTeamList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetStatisticMembers?startDate=${fromDate}&endDate=${toDate}`));
  }

  getCustomerList(fromDate, toDate): Observable<any> {
    return this.http.get(this.getUrl(`GetTimesheetStatisticClients?startDate=${fromDate}&endDate=${toDate}`));
  }

  SubmitToPending(startDate, endDate): Observable<any> {
    return this.http.post(this.rootUrl + '/SubmitToPending', { startDate: startDate, endDate: endDate });
  }
  SaveList(item: any): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/SaveList', item);
  }
  saveAndReset(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/SaveAndReset', data);
  }

  warningMyTimesheet(dateAt:string, workingTime:number, timesheetId:number): Observable<ApiResponse<WarningMyTimesheetDto>> {
    return this.http.get<any>(this.rootUrl + `/WarningMyTimesheet?dateAt=${dateAt}&workingTime=${workingTime}&timesheetId=${timesheetId == null ? '' :timesheetId}`);
  }
}
