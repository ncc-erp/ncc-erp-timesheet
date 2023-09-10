import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class MyWorkingTimeService extends BaseApiService{

  constructor(http: HttpClient) {
    super(http);
   }
  changeUrl() {
    return 'MyWorkingTime';
  }
  getMyCurrentWorkingTime(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetMyCurrentWorkingTime');
  }
  getAllMyHistoryWorkingTime(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetAllMyHistoryWorkingTime');
  }
  submitNewWorkingTime(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/SubmitNewWorkingTime', data);
  }

  editWorkingTime(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/EditWorkingTime', data);
  }
  
  deleteWorkingTime(id: any): Observable<any>{
    return this.http.delete<any>(this.rootUrl + '/DeleteWorkingTime', {
      params: new HttpParams().set('Id', id)
  })
  }
}
