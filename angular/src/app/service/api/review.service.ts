import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedRequestDto } from '@shared/paged-listing-component-base';

@Injectable({
  providedIn: 'root'
})
export class ReviewService extends BaseApiService {

  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  changeUrl() {
    return 'ReviewIntern';
  }

  createReview(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/Create', data);
  }

  changeActive(id: number): Observable<any> {
    return this.http.post(this.rootUrl + `/Active?Id=${id}`, {});
  }

  changeDeActive(id: number): Observable<any> {
    return this.http.post(this.rootUrl + `/DeActive?Id=${id}`, {});
  }

  getListReview(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetAll');
  }

  getAllReport(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetAllReport');
  }
  getReport(year: any, month: any, level: any, isCurrentInternOnly: any, keyword: any): Observable<any> {
    //if (month) {
      return this.http.get(this.rootUrl + `/GetReport?year=${year}&month=${month}&level=${level}&isCurrentInternOnly=${isCurrentInternOnly}&keyword=${keyword}`);
    // }
    // else {
    //   return this.http.get(this.rootUrl + `/GetReport?year=${year}`);
    // }
  }
  deleteListReview(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/Delete?id=' + id)
  }

  sendAllEmailsIntern(id: number, check: boolean): Observable<any> {
    return this.http.post(this.rootUrl + `/SendEmailsIntern?reviewId=${id}&isCheckToOffical=${check}`, {})
  }

  sendAllApproves(request:PagedRequestDto, reviewId, branchId): Observable<any> {
    return this.http.post(this.rootUrl + "/ApproveAll?reviewId=" + reviewId+`&branchId=${branchId <= 0 ? '' : branchId}`, request);
  }

  sendAllEmailHRM(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/UpdateLevelHRMs?reviewId=' + id);
  }

  ExportReportIntern(year: number, month: any, level: any, isCurrentInternOnly: any, keyword: any): Observable<any> {
    return this.http.post(this.rootUrl + `/ExportReportIntern?year=${year}&month=${month}&level=${level}&isCurrentInternOnly=${isCurrentInternOnly}&keyword=${keyword}`, {})
  }
  UpdateStarProject(id: number): Observable<any>{
    return this.http.get(this.rootUrl +'/UpdateStarProject?reviewId=' + id);
  }

  UpdateLevelHRMv2ForAll(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/UpdateLevelHRMv2ForAll?reviewId=' + id);
  }
  UpdateLevelHRMv2ForOne(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/UpdateLevelHRMv2ForOne?reviewDetailId=' + id);
  }
  createReviewInternCapability(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/CreateInternCapability', data);
  }
  deleteListReviewInternCapability(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/DeleteInternCapability?id=' + id)
  }
}
