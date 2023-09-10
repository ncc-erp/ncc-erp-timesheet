import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RetroService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Retro';
  }
  changeStatus(id: number): Observable<any> {
    return this.http.put(this.rootUrl + '/ChangeStatus', { id });
  }
}
