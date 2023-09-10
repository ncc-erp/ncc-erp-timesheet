import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EmailSettingService extends BaseApiService{

  constructor(http: HttpClient) {
    super(http);
  }
  changeUrl(){
    return 'EmailSetting';
  }
  getMail(): Observable<any> {
    return this.http.get<any>(this.rootUrl + '/Get');
  }
   change(data : object) : Observable<any>{
    return this.http.post(this.rootUrl+ '/Change', data);
   }
 }
