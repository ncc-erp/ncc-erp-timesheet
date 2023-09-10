import { PagedRequestDto } from '@shared/paged-listing-component-base';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReviewDetailService extends BaseApiService{

  constructor(
    http: HttpClient
  ) {
    super(http);
   }

   changeUrl() {
    return 'ReviewDetail';
  }

  getInternshipToReview(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/GetUnReviewIntership?reviewId=' + id);
  }

  getReviewToUpdate(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/Get?Id=' + id);
  }

  getSubLevelToUpdate(): Observable<any> {
    return this.http.get(this.rootUrl + '/GetLevelSetting');
  }

  sendEmail( id: number): Observable<any> {
    return this.http.post(this.rootUrl + '/SendEmail?Id='+ id, {});
  }

  approve( id: number): Observable<any> {
    return this.http.post(this.rootUrl + '/Approve?Id='+ id, {});
  }

  reject( id: number): Observable<any> {
    return this.http.post(this.rootUrl + '/Reject?Id='+ id, {});
  }

  sendEmailHRM( id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/UpdateLevelHRM?detailId='+ id);
  }

  getAllPaging(request:PagedRequestDto, reviewId,branchId): Observable<any> {
    return this.http.post(this.rootUrl + "/GetAllDetails?reviewId=" + reviewId+`&branchId=${branchId <= 0 ? '' : branchId}`, request);
  }

  saveReviewDetail(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/Update', data);
  }

  saveReviewInternship(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/PMReview', data);
  }

  createReviewDetail(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/Create', data);
  }

  deleteReviewDetail(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/Delete?id=' + id)
  }
  rejectSentMail(id: any): Observable<any> {
    return this.http.post(this.rootUrl + `/RejectSentMail?Id=${id}`, {});
  }

  getUserConfirmSalalry(id: number): Observable<any>{
    return this.http.get(this.rootUrl+ `/Get?Id=${id}`);
  }

  saveConfirmDueSalary(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/ConfirmApproveSalary', data);
  }
  getReviewToUpdateInternCapability(id: number): Observable<any> {
    return this.http.get(this.rootUrl + '/GetInternCapability?Id=' + id);
  }
  saveReviewInternCapability(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/PMReviewInternCapability', data);
  }
  createInternCapability(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/CreateInternCapability', data);
  }
  deleteReviewDetailInternCapability(id: number): Observable<any> {
    return this.http.delete(this.rootUrl + '/DeleteInternCapability?id=' + id)
  }
  changeReviewerOfDetail(data: object): Observable<any> {
    return this.http.post(this.rootUrl + '/UpdateReviewer', data);
  }
}
