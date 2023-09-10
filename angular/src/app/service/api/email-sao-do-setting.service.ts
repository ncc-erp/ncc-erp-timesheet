import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class EmailSaoDoSettingService extends BaseApiService {

  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl(){
    return 'EmailSaoDoSetting';
  }
  getSetting(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
   change(data : object) : Observable<any>{
    return this.http.post(this.rootUrl+ '/Change', data);
   }
}
