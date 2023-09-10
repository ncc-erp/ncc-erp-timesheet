import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { PagedRequestDto } from '@shared/paged-listing-component-base';

@Injectable({
  providedIn: 'root'
})
export class ManageNewsService extends BaseApiService {  

  constructor(http: HttpClient) {
    super(http);
  }
  
  changeUrl() {
    return 'Posts';
  }
   get(id: any): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get?id=' + id);
}
}