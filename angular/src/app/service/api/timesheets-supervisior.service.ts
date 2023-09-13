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

  getAll(startDate: string, endDate: string, status: number, projectId: number, userId: number): Observable<any> {
    return this.http.get(this.getUrl(`GetAll?startDate=${startDate}&endDate=${endDate}&status=${status}&projectID=${this.getPara(projectId)}&userId=${this.getPara(userId)}`));
  }
  private getPara(value){
    if(value <=0) return '';
    return value
  }

  GetQuantityTimesheetSupervisorStatus(startDate: string, endDate: string){
    return this.http.get(this.getUrl(`GetQuantityTimesheetSupervisorStatus?startDate=${startDate}&endDate=${endDate}`));
  }

  GetAllActiveProject(): Observable<any> {
    return this.http.get(this.getUrl(`GetAllActiveProject`));
  }

  GetAllActiveUser(): Observable<any> {
    return this.http.get(this.getUrl(`GetAllActiveUser`))
  }
}
