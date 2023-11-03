import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { ProjectByCurrentUserDto } from './model/project-Dto';

@Injectable({
  providedIn: 'root'
})

export class AppServiceBase extends BaseApiService{
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Account';
  }

  getAllProjectByCurrentUser(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllProjectIdByCurrentPM");
  }

  getAllCurrentBranch(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllCurrentBranch");
  }
}
