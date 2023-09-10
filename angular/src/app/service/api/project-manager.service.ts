import { BaseApiService } from './base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectManagerService extends BaseApiService {
  constructor(
   http: HttpClient
  ) {
    super(http);
   }
  changeUrl() {
    return 'Project';
  }

  deactiveProject(param: any): Observable<any> {
    return this.http.post(this.rootUrl + `/Inactive`, param );
  }

  activeProject(param: any): Observable<any> {
    return this.http.post(this.rootUrl + `/Active`, param );
  }

  deleteProject(projectId: number){
    return this.http.delete(this.rootUrl + `/Delete?Id=${projectId}`)
  }
  
  getAll(status: number, search: string): Observable<any>{
    if (status <= 1) {
      return this.http.get(this.rootUrl +`/getAll?status=${status}&search=${search}`);
    } else {
      return this.http.get(this.rootUrl + `/getAll?search=${search}`);
    }  
  }

  getProject(projectId: number): Observable<any> {
    return this.http.get(this.rootUrl + `/Get?input=${projectId}`);
  }

  
  getProjectFilter(): Observable<any>{
      return this.http.get(this.rootUrl + '/GetFilter');
  }
  getProjectDetailTask(projectId:number): Observable<any>{
    return this.http.get(this.rootUrl + '/GetTimeSheetStatisticTasks?projectId='+projectId);
  }
  ExportExcel(fromDate, toDate,projectId:number): Observable<any>{
    return this.http.get(this.rootUrl + `/ExportExcel?projectId=${projectId}&startDate=${fromDate}&endDate=${toDate}`);
  }

  getProjectDetailTeam(projectId:number): Observable<any>{
    return this.http.get(this.rootUrl + '/ViewTimeByTeam?projectId='+projectId);
  }
  
  GetProjectsIncludingTasks():Observable<any>{
    return this.http.get(this.rootUrl + '/GetProjectsIncludingTasks');
  }

  getProjectPM(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetProjectPM");
  }
  getProjectWorkingTimePM(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetProjectWorkingTimePM");
  }

  getProjectUser(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetProjectUser");
  }
  
  getQuantityProject(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetQuantityProject");
  }

  updateDefaultProjectTask(taskId: any): Observable<any>{
    return this.http.post(this.rootUrl + `/UpdateDefaultProjectTask`, {
      "id": taskId
    });
  } 

  clearDefaultProjectTask(): Observable<any>{
    return this.http.post(this.rootUrl + `/ClearDefaultProjectTask`, null);
  } 
}
