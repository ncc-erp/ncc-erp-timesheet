import { BaseApiService } from './base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { formatDate } from '@angular/common';
import { toDate } from '@angular/common/src/i18n/format_date';

@Injectable({
  providedIn: 'root'
})
export class ProjectDetailService extends BaseApiService {
  constructor(
   http: HttpClient
  ) {
    super(http);
   }
  changeUrl() {
    return 'TimeSheetProject';
  }


  GetTimeSheetStatisticTasks(projectId:number,fromDate,toDate): Observable<any>{

    return this.http.get(this.rootUrl + `/GetTimeSheetStatisticTasks?projectId=${projectId}&startDate=${fromDate}&endDate=${toDate}`);
  }
  getExportBillableTimesheets(projectId:number, fromDate, toDate): Observable<any> {
    return this.http.get(this.rootUrl + `/ExportBillableTimesheets?projectId=${projectId}&startDate=${fromDate}&endDate=${toDate}`);
  }
  ExportExcel(fromDate, toDate,projectId:number): Observable<any>{
    return this.http.get(this.rootUrl + `/ExportExcel?projectId=${projectId}&startDate=${fromDate}&endDate=${toDate}`);
  }

  getProjectDetailTeam(projectId:number,fromDate, toDate): Observable<any>{
    return this.http.get(this.rootUrl + `/GetTimeSheetStatisticTeams?projectId=${projectId}&startDate=${fromDate}&endDate=${toDate}`);
  }


}
