import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface UserPunishmentPaidDto {
  dateAt: string;
  amount: number;
  txHash: string;
}

export interface MarkPaidTransactionResultDto {
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserPunishmentPaidService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl() {
    return 'UserPunishmentPaid';
  }

  getForCurrentUser(year: number, month: number): Observable<UserPunishmentPaidDto[]> {
    const url = `${this.rootUrl}/GetForCurrentUser?year=${year}&month=${month}`;
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return [];
      })
    );
  }
  
  markPaidTransaction(transactionHash: string, year: number, month: number): Observable<MarkPaidTransactionResultDto> {
    const url = `${this.rootUrl}/MarkPaidTransaction`;
    return this.http.post<any>(url, { transactionHash, year, month }).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return { success: false, message: 'Unknown error occurred' };
      })
    );
  }
}
