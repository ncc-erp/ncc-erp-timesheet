import { Injectable } from "@node_modules/@angular/core";
import { HttpClient, HttpParams } from "@node_modules/@angular/common/http";
import { BaseApiService } from "./base-api.service";
import { Observable } from "@node_modules/rxjs";
import { map } from 'rxjs/operators';

export interface OfficeWorkingItem {
  userId: number;
  userName: string;
  officeName: string;
  officeCode: string;
  totalAllLW: number;
  officeLW: number;
  wfhLW: number;
  totalAllLM: number;
  officeLM: number;
  wfhLM: number;
  totalAllLWHours: number;
  officeLWHours: number;
  wfhLWHours: number;
  totalAllLMHours: number;
  officeLMHours: number;
  wfhLMHours: number;
}

export interface DailyProjectTimelogReportResponse {
  lastWeekStart: string;
  lastWeekEnd: string;
  lastMonth: string;
  officeWorkingData: OfficeWorkingItem[];
}

@Injectable({
  providedIn: "root",
})
export class DailyEmployeeReportService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "OfficeReport";
  }

  getDailyProjectTimelogReport(
    branchIds: number[],
    limit?: number
  ): Observable<DailyProjectTimelogReportResponse> {
    let params = new HttpParams();
    branchIds.forEach(id  => {
      params = params.append("BranchId", id.toString());
    });
    if (limit !== undefined) {
      params = params.set("Limit", limit.toString());
    }

    console.log('API URL:', this.getUrl("GetOfficeWorkingTimelogReport"));
    console.log('API Params:', params.toString());

    return this.http.get<any>(
      this.getUrl("GetOfficeWorkingTimelogReport"),
      { params }
    ).pipe(
      map(response => response.result ? response.result as DailyProjectTimelogReportResponse : response as DailyProjectTimelogReportResponse)
    );
  }

}
