import { HttpClient } from '@angular/common/http';
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

  getAll(startDate: string, endDate: string, status: number): Observable<any> {
    // return this.http.get(this.rootUrl + '/GetAll?startDate=' + startDate + '&endDate=' + endDate + '&status=' + status);
    return this.http.get(this.getUrl(`GetAll?startDate=${startDate}&endDate=${endDate}&status=${status}`));
  }
  GetQuantityTimesheetSupervisorStatus(startDate: string, endDate: string){
    return this.http.get(this.getUrl(`GetQuantityTimesheetSupervisorStatus?startDate=${startDate}&endDate=${endDate}`));
  }
}
