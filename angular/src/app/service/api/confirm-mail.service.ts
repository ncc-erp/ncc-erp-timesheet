import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class ConfirmMailService extends BaseApiService {
  changeUrl() {
    return "HRMv2";
  }

  constructor(http: HttpClient) {
    super(http);
  }

  public ConfirmPayslipMail(payslipId: number): Observable<any> {
    return this.http.get(this.rootUrl + `/ConfirmPayslipMail?payslipId=${payslipId}`);
  }

  public ComplainPayslipMail(id: number, note: string): Observable<any> {
    return this.http.post(this.rootUrl + "/ComplainPayslipMail", { payslipId: id, complainNote: note });
  }
}
