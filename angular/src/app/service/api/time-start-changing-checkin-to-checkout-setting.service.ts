import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimeStartChangingCheckinToCheckoutSettingService extends BaseApiService {


  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl() {
    return 'TimeStartChangingCheckinToCheckoutSetting';
  }
  get(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
  change(data : object) : Observable<any>{
    var a=this.rootUrl;
    return this.http.put(this.rootUrl+ '/Change', data);
   }
}
