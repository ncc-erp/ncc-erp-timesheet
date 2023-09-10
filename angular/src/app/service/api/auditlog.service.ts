import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuditlogService extends BaseApiService{
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'Auditlog';
  }

  getAll(request): Observable<any> {
    return this.http.post(this.rootUrl + "/GetAllPagging?", request);
  }

  getAllEmailAddressInAuditLog(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllEmailAddressInAuditLog");
  }
  getAllServiceNameInAuditLog(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllServiceNameInAuditLog");
  }
  getAllMethodNameInAuditLog(): Observable<any> {
    return this.http.get(this.rootUrl + "/GetAllMethodNameInAuditLog");
  }
}
