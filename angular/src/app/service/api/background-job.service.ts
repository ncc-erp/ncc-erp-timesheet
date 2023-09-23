import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { extend } from 'lodash';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BackgroundJobService extends BaseApiService{
  changeUrl() {
    return "BackgroundJob"
  }

  constructor(http: HttpClient) {
    super(http);
  }
  getAllBackgroundJob(input):Observable<any>{
    return this.http.post(this.rootUrl + "/GetAllPaging", input);
  }
}
