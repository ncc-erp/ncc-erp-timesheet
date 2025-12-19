import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { BaseApiService } from "./base-api.service";

export interface AnomaliesTimelogReportResponse {
  result: AnomaliesTimelogReportResponse;
  yesterdayAnomalies: {
    userId: number;
    employeeName: string;
    userName: string;
    date: string;
    actualHours: string;
    notes: string;
    branch: {
      branchName: string;
      branchColor: string;
    };
  }[];
  lastWeekAnomalies: {
    userId: number;
    employeeName: string;
    userName: string;
    datesMissed: string[];
    datesNoTrackerTime: string[];
    datesBelowThreshold: string[];
    count: number;
    notes: string;
    branch: {
      branchName: string;
      branchColor: string;
    };
  }[];
}

@Injectable({
  providedIn: "root",
})
export class AnomaliesReportService extends BaseApiService {
  constructor(protected http: HttpClient) {
    super(http);
  }

  changeUrl(): string {
    return "Report";
  }

  getAnomaliesTimelogReport(branchIds: number[]): Observable<AnomaliesTimelogReportResponse> {
    let params = new HttpParams();

    if (branchIds && branchIds.length > 0) {
      branchIds.forEach(id => {
        params = params.append("BranchIds", id.toString());
      });
    }

    return this.http
      .get<AnomaliesTimelogReportResponse>(
        this.getUrl("GetAnomaliesTimelogReport"),
        { params }
      )
      .pipe(map(res => res.result as AnomaliesTimelogReportResponse));
  }
}
