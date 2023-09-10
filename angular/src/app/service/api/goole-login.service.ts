import { BaseApiService } from './base-api.service';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GoogleLoginService extends BaseApiService {
  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  

  changeUrl() {
    return 'Task';
  }

 
  googleAuthenticate(googleToken: string, secretCode:string): Observable<any> {
    return this.http.post(this.baseUrl + '/api/TokenAuth/GoogleAuthenticate', {googleToken: googleToken, secretCode: secretCode});
  }


}
