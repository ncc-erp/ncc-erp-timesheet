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

export interface GetUserPunishmentBalanceDto {
  totalPunishmentMoney: number;
  remainPoints: number;
  effectiveAmount: number;
}

export interface PreviewAndApplyPunishmentPointsDto {
  success: boolean;
  message: string;
  totalHashAmount: number;
  totalPunishmentMoney: number;
  remainPoints: number;
  effectivePunishmentAmount: number;
  punishmentsMarkedAsPaid: number;
  hasSufficientFunds: boolean;
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

  getCurrentUserBalance(): Observable<GetUserPunishmentBalanceDto> {
    const url = `${this.rootUrl}/GetCurrentUserBalance`;
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return { totalPunishmentMoney: 0, remainPoints: 0, effectiveAmount: 0 };
      })
    );
  }

  getTotalRemainPointsUsedInMonth(year: number, month: number): Observable<number> {
    const url = `${this.rootUrl}/GetTotalRemainPointsUsedInMonth?year=${year}&month=${month}`;
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return 0;
      })
    );
  }

  getTotalPaidPunishmentInMonth(year: number, month: number): Observable<number> {
    const url = `${this.rootUrl}/GetTotalPaidPunishmentInMonth?year=${year}&month=${month}`;
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && response.result !== undefined) {
          return response.result;
        }
        return 0;
      })
    );
  }

  previewAndApplyPunishmentPoints(year: number, month: number): Observable<PreviewAndApplyPunishmentPointsDto> {
    const url = `${this.rootUrl}/PreviewAndApplyPunishmentPoints`;
    const body = { year: year, month: month };
    return this.http.post<any>(url, body).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return { 
          success: false, 
          message: 'Unknown error occurred', 
          totalHashAmount: 0,
          totalPunishmentMoney: 0,
          remainPoints: 0,
          effectivePunishmentAmount: 0,
          punishmentsMarkedAsPaid: 0,
          hasSufficientFunds: false
        };
      })
    );
  }
}
