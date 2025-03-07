import { Injectable, InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class MezonLoginService extends BaseApiService {
  changeUrl(): string {
     return 'Mezon';
  }

  constructor(
    http: HttpClient
  ) {
    super(http);
  }

  redirectToOAuth() {
    window.location.href = `${this.baseUrl}/api/TokenAuth/MezonRedirect`;
  }

  mezonAuthenticate(token: string): Observable<any> {
    return this.http.post(this.baseUrl + '/api/TokenAuth/MezonAuthenticate', {token: token});
  }
}