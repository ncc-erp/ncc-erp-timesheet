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

export interface UserPunishmentSummaryDto {
  success: boolean;
  message: string;
  userBalance: GetUserPunishmentBalanceDto;
  totalRemainPointsUsedInMonth: number;
  totalPaidPunishmentInMonth: number;
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

  previewApplyAndGetSummary(year: number, month: number): Observable<UserPunishmentSummaryDto> {
    const url = `${this.rootUrl}/PreviewApplyAndGetSummary`;
    const body = { year: year, month: month };
    return this.http.post<any>(url, body).pipe(
      map(response => {
        if (response && response.result) {
          return response.result;
        }
        return {
          success: false,
          message: 'No data received',
          userBalance: { totalPunishmentMoney: 0, remainPoints: 0, effectiveAmount: 0 },
          totalRemainPointsUsedInMonth: 0,
          totalPaidPunishmentInMonth: 0
        };
      })
    );
  }
}
