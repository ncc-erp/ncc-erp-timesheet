import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ViewHistoryService extends BaseApiService{

  constructor(
    http: HttpClient
  ) {
    super(http);
   }

   changeUrl() {
    return 'ReviewDetail';
  }
  getHistory(internshipId : number, reviewId): Observable<any> {
    return this.http.get(this.rootUrl + '/GetHistories?internshipId='+ internshipId + '&reviewId=' + reviewId);
  }
}
