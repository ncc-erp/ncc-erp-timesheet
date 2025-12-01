import { Injectable } from "@node_modules/@angular/core";
import { HttpClient, HttpParams } from "@node_modules/@angular/common/http";
import { BaseApiService } from "./base-api.service";
import { Observable } from "@node_modules/rxjs";
import { map } from 'rxjs/operators';

export interface OfficeWorkingItem {
  userId: number;
  fullName: string;
  userName: string;
  branchName: string;
  branchCode: string;
  branchColor: string;
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

@Injectable({
  providedIn: "root",
})
export class DailyEmployeeReportService extends BaseApiService {
  constructor(http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "Report";
  }

  getDailyProjectTimelogReport(
    branchIds: number[],
    limit?: number
  ): Observable<OfficeWorkingItem[]> {
    let params = new HttpParams();
    branchIds.forEach(id  => {
      params = params.append("BranchId", id.toString());
    });
    if (limit !== undefined) {
      params = params.set("Limit", limit.toString());
    }

    return this.http.get<any>(
      this.getUrl("GetOfficeWorkingTimelogReport"),
      { params }
    ).pipe(
      map(response => response.result ? response.result as OfficeWorkingItem[] : response as OfficeWorkingItem[])
    );
  }

}
