import { HttpClient, HttpHeaders, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class TimekeepingService extends BaseApiService {

  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'Timekeeping';
  }

  getDetailTimekeeping(year: number, month: number, day: number, userId: number, branchId : number, isPunished : any, isComplain : any, statusPunish?: number): Observable<any> {
    let apiString = `${this.rootUrl}/GetDetailTimekeeping?year=${year}&month=${month}&day=${day}`
    if (userId!=-1) {
      apiString += `&userId=${userId}`
    }
    if(isPunished!=-1){
      apiString += `&isPunished=${isPunished}`
    }
    if(isComplain!=-1){
      apiString += `&isComplain=${isComplain}`
    }
    if(branchId!=0){
      apiString += `&branchId=${branchId}`
    }
    if(statusPunish != undefined && statusPunish!=-1){
      apiString += `&statusPunish=${statusPunish}`
    }
    return this.http.get<any>(apiString);
  }

  getMyDetails(year: number, month: number): Observable<any> {
    let apiString = `${this.rootUrl}/GetMyDetails?year=${year}&month=${month}`
    return this.http.get<any>(apiString);
  }

  // @ts-ignore
  getAllPagging(request: PagedRequestDto, year: number, month: number, branchId: number, userId: number): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/GetAllPagging?year=' + year + '&month=' + month + `&branchId=${branchId == 0 ? '' : branchId}` + `&userId=${userId < 1 ? '' : userId}`, request);
  }

  getDataCheckInInternal(sDate, eDate): Observable<any> {
    return this.http.get(this.rootUrl + '/GetApiCheckInInternal?startDate=' + sDate + '&endDate=' + eDate);
  }

  getAddTimeByDay(request: any): Observable<any> {
    return this.http.post<any>(this.rootUrl + `/AddTimekeepingByDay?date=${request}`, {});
  }

  ImportTimekeepingFromFile(file: File): Observable<any> {
    const formData = new FormData();
    const url = '/api/services/app/Timekeeping/ImportTimekeepingFromFile';
    formData.append('file', file);
    const uploadReq = new HttpRequest('POST', AppConsts.remoteServiceBaseUrl + url,
      formData,
      {
        reportProgress: true
      }
    );
    return this.http.request(uploadReq);
  }

  updateTimekeeping(param): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/UpdateTimekeeping', param);
  }
  addComplain(param): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/UserKhieuLai', param);
  }
  answerComplain(param): Observable<any> {
    return this.http.post<any>(this.rootUrl + '/TraLoiKhieuLai', param);

  }
}
