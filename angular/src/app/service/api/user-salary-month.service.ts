import { HttpClient } from '@angular/common/http';
import { BaseApiService } from '@app/service/api/base-api.service';
import { Injectable, inject, Injector } from '@angular/core';
import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserSalaryMonthService extends BaseApiService{
  changeUrl() {
   return "UserSalaryMonth"
  }

  constructor(http : HttpClient) {
    super(http)
   }
   getAll(request : any,year: number, month: number):Observable<any> {
    return this.http.post(this.rootUrl + '/GetAllPagging?' + `year=${year}&month=${month}`,request)
  }
  RecalculateSalary(year: number, month: number,id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/RecalculateSalary?year=' + year + '&month=' + month + '&input=' + id)
}
  ResendNotification(year: number, month: number,id :number): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/ResendNotification?year=' + year + '&month=' + month + '&input=' + id,id);
  }
  delete(id : any) : Observable<any> {
    return this.http.delete<any>(this.rootUrl + '/Delete?Id='+ id)
  }
  CalculateSalary(year: number, month: number,branch: number): Observable<any> {
    return this.http.post(this.rootUrl + '/CalculateSalary?year=' + year + '&month=' + month + '&branch=' + branch,branch)
}
confirm(year: number, month: number,branch: number): Observable<any> {
  return this.http.post(this.rootUrl + '/Confirm?year=' + year + '&month=' + month + '&branch=' + branch,branch)
}
getNotifyAll(year: number, month: number,branch: number): Observable<any> {
  return this.http.post(this.rootUrl + '/NotifyAll?year=' + year + '&month=' + month + '&branch=' + branch,branch)
}
}
